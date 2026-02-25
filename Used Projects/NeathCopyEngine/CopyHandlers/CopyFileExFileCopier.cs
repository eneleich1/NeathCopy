using NeathCopyEngine.DataTools;
using NeathCopyEngine.Helpers;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace NeathCopyEngine.CopyHandlers
{
    /// <summary>
    /// File copier that uses CopyFileExW to get native progress updates.
    /// </summary>
    public sealed class CopyFileExFileCopier : BufferFileCopier
    {
        private const int ERROR_REQUEST_ABORTED = 1235;
        private readonly object totalsLock = new object();
        private int cancelRequested;

        public CopyFileExFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "CopyFileExFileCopier";
        }

        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;
            FileBytesTransferred = 0;
            Interlocked.Exchange(ref cancelRequested, 0);
            Interlocked.Exchange(ref skipRequested, 0);

            var src = LongPathHelper.Normalize(file.FullName);
            var dst = LongPathHelper.Normalize(file.DestinyPath);

            long lastTransferred = 0;
            bool canceled = false;
            bool skipped = false;
            bool pbCancel = false;

            // LPPROGRESS_ROUTINE signature must match WinAPI; cancellation is via return value / pbCancel.
            CopyProgressRoutine callback = (long total, long transferred, long streamSize, long streamTransferred, uint streamNumber,
                CopyProgressCallbackReason reason, IntPtr srcHandle, IntPtr dstHandle, IntPtr data) =>
                HandleProgress(total, transferred, ref lastTransferred, ref canceled, ref skipped, ref pbCancel);

            var ok = CopyFileExW(src, dst, callback, IntPtr.Zero, ref pbCancel, CopyFileFlags.COPY_FILE_RESTARTABLE);
            if (!ok)
            {
                var error = Marshal.GetLastWin32Error();
                if (pbCancel || canceled || skipped || error == ERROR_REQUEST_ABORTED)
                {
                    TryDeletePartial(dst);
                    ConsumeSkipRequested();
                    return;
                }

                throw new IOException(string.Format("CopyFileEx failed ({0}): {1}", error, new Win32Exception(error).Message));
            }

            // Ensure final progress is consistent.
            FileBytesTransferred = file.Size;
            var finalDelta = file.Size - lastTransferred;
            if (finalDelta > 0)
                TotalBytesTransferred += finalDelta;
        }

        public override FileCopier Clone()
        {
            return new CopyFileExFileCopier(BufferSize);
        }

        public override void Cancel()
        {
            Interlocked.Exchange(ref cancelRequested, 1);
        }

        public override void Skip()
        {
            Interlocked.Exchange(ref skipRequested, 1);
        }

        private static void TryDeletePartial(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // Ignore cleanup failures.
            }
        }

        private delegate CopyProgressResult CopyProgressRoutine(
            long totalFileSize,
            long totalBytesTransferred,
            long streamSize,
            long streamBytesTransferred,
            uint streamNumber,
            CopyProgressCallbackReason reason,
            IntPtr sourceFile,
            IntPtr destinationFile,
            IntPtr data);

        private CopyProgressResult HandleProgress(long totalBytesTransferred, long currentBytesTransferred, ref long lastTransferred,
            ref bool canceled, ref bool skipped, ref bool pbCancel)
        {
            var current = currentBytesTransferred;
            FileBytesTransferred = current;

            var delta = current - lastTransferred;
            if (delta > 0)
            {
                lock (totalsLock)
                {
                    TotalBytesTransferred += delta;
                }
            }
            lastTransferred = current;

            if (Volatile.Read(ref cancelRequested) != 0)
            {
                canceled = true;
                pbCancel = true;
                return CopyProgressResult.PROGRESS_CANCEL;
            }

            if (IsSkipRequested())
            {
                skipped = true;
                pbCancel = true;
                return CopyProgressResult.PROGRESS_CANCEL;
            }

            try
            {
                WaitForResumeOrCancel();
            }
            catch (OperationCanceledException)
            {
                canceled = true;
                pbCancel = true;
                return CopyProgressResult.PROGRESS_CANCEL;
            }

            return CopyProgressResult.PROGRESS_CONTINUE;
        }

        private enum CopyProgressResult : uint
        {
            PROGRESS_CONTINUE = 0,
            PROGRESS_CANCEL = 1,
            PROGRESS_STOP = 2,
            PROGRESS_QUIET = 3
        }

        private enum CopyProgressCallbackReason : uint
        {
            CALLBACK_CHUNK_FINISHED = 0x00000000,
            CALLBACK_STREAM_SWITCH = 0x00000001
        }

        [Flags]
        private enum CopyFileFlags : uint
        {
            COPY_FILE_RESTARTABLE = 0x00000002
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CopyFileExW(
            string lpExistingFileName,
            string lpNewFileName,
            CopyProgressRoutine lpProgressRoutine,
            IntPtr lpData,
            ref bool pbCancel,
            CopyFileFlags dwCopyFlags);
    }
}
