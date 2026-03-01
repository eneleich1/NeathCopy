using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using NeathCopyEngine.DataTools;
using System.Threading;
using NeathCopyEngine.Exceptions;
using NeathCopyEngine.Helpers;

namespace NeathCopyEngine.CopyHandlers
{
    /// <summary>
    /// Allow to copy a file to a destiny path.
    /// </summary>
    public abstract class FileCopier
    {
        /// <summary>
        /// Object For locking. Use this object in your action loop to allow pause: lock (syncObj) { }.
        /// </summary>
        public string Name { get; set; }
        public string Description { get; set; }
        public FileDataInfo CurrentFile { get; protected set; }
        public FileStream Reader { get; protected set; }
        public FileStream Writer { get; protected set; }
        protected ManualResetEventSlim PauseGate { get; private set; }
        protected CancellationToken CancellationToken { get; private set; }
        protected int skipRequested;

        /// <summary>
        /// Get the total bytes transferred of file is been copy.
        /// </summary>
        public long FileBytesTransferred { get; set; }
        /// <summary>
        /// Get the total bytes transferred since this instance was created.
        /// </summary>
        public long TotalBytesTransferred { get; set; }

        /// <summary>
        /// Copy the specific sourceFile to destinyPath new location.
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="destinyPath"></param>
        public abstract void CopyFile(FileDataInfo file);
        public abstract FileCopier Clone();
        public abstract void Cancel();

        public abstract void Skip();
        public void ConfigureExecution(ManualResetEventSlim pauseGate, CancellationToken token)
        {
            PauseGate = pauseGate ?? new ManualResetEventSlim(true);
            CancellationToken = token;
        }
        protected void WaitForResumeOrCancel()
        {
            var gate = PauseGate;
            if (gate != null)
                gate.Wait(CancellationToken);

            CancellationToken.ThrowIfCancellationRequested();
        }
        protected bool IsSkipRequested()
        {
            return System.Threading.Volatile.Read(ref skipRequested) != 0;
        }
        protected bool ConsumeSkipRequested()
        {
            return Interlocked.Exchange(ref skipRequested, 0) == 1;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    public enum FileCopyOptions
    {
        OverwriteIfFileExist, SkipIfFileExist, AllwaysSkip, AllwaysOverride, AllwaysAsk, Cancel,OverrideDifferent,AllwaysOverrideDifferent
    }

    /// <summary>
    /// FileCopier based on buffer.
    /// </summary>
    public class BufferFileCopier : FileCopier
    {
        protected int bufferSize;
        public int BufferSize
        {
            get
            {
                return bufferSize;
            }
            set
            {
                bufferSize = value;
                buffer = new byte[bufferSize];
            }
        }
      
        protected byte[] buffer;
        protected int readBytes;

        public BufferFileCopier(int bufferSize)
            : base()
        {
            BufferSize = bufferSize;
            buffer = new byte[BufferSize];

            Name = "BufferFileCopier";
        }

        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;

            Reader = file.GetStreamToRead();
            Writer = file.GetStreamToWrite(FileMode.Create);

            readBytes = 0;
            FileBytesTransferred = 0;

            unsafe
            {
                while (true)
                {
                    WaitForResumeOrCancel();
                    if (IsSkipRequested()) break;

                    //Dinamic bufferSize. Dynamic buffer size.
                    readBytes = Reader.Read(buffer, 0, (int)(file.Size > BufferSize ? BufferSize : file.Size));
                    if (readBytes <= 0) break;

                    WaitForResumeOrCancel();
                    if (IsSkipRequested()) break;
                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;
                }
            }

            Reader.Close();
            Reader.Dispose();

            if (ConsumeSkipRequested() && Writer != null && Writer.CanWrite)
                Writer.SetLength(FileBytesTransferred);

            Writer.Flush();
            Writer.Close();
            Writer.Dispose();

            Writer = null;

        }

        public override FileCopier Clone()
        {
            return new BufferFileCopier(BufferSize);
        }

        public virtual void ReleaseResources()
        {
            Reader.Dispose();

            Monitor.Enter(Writer);

            if (Writer != null && Writer.CanWrite)
            {
                //Writer.SetLength(FileBytesTransferred);
                Writer.Flush();
                Writer.Close();
            }

            Monitor.Exit(Writer);

            if (CurrentFile.CopyState == CopyState.Processing)
                System.IO.File.Delete(LongPathHelper.Normalize(CurrentFile.DestinyPath));
        }

        public override void Cancel()
        {
            // Capture a stable reference; CurrentFile may change during cancel.
            var file = CurrentFile;
            if (file == null)
                return;

            try
            {
                // Close writer safely. Writer can already be disposed/closed.
                if (Writer != null)
                {
                    try
                    {
                        if (Writer.CanWrite)
                        {
                            Writer.SetLength(FileBytesTransferred);
                            Writer.Flush();
                        }
                    }
                    catch
                    {
                        // Ignore: writer may be closed or not writable.
                    }

                    try { Writer.Close(); } catch { }
                }
            }
            catch (Exception ex)
            {
                // Avoid MessageBox in engine threads if possible.
                System.Windows.MessageBox.Show(ex.Message + " in FileCopier.Cancel");
            }
            finally
            {
                try
                {
                    // Use the captured file reference, not CurrentFile.
                    var dest = string.IsNullOrWhiteSpace(file.TempDestinyPath) ? file.DestinyPath : file.TempDestinyPath;
                    if (!string.IsNullOrWhiteSpace(dest))
                    {
                        System.IO.File.Delete(LongPathHelper.Normalize(dest));
                    }
                }
                catch
                {
                    // Ignore cleanup failures (file may not exist, path invalid, permissions, etc.)
                }
            }
        }

        public override void Skip()
        {
            if (CurrentFile == null) return;

            try
            {
                Interlocked.Exchange(ref skipRequested, 1);
            }
            catch (Exception) { }
            finally
            {
                // Streams are closed by the copy loop after it observes the skip flag.
            }
        }

    }
    /// <summary>
    /// FileCopier based on ProducerConsumer threading framework.
    /// </summary>
    public class ProducerConsumerFileCopier : BufferFileCopier
    {
        public ProducerConsumerFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "ProducerConsumerFileCopier";
        }
        public override void CopyFile(FileDataInfo file)
        {
            base.CopyFile(file);
        }
        public override FileCopier Clone()
        {
            return new ProducerConsumerFileCopier(BufferSize);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class BufferBinaryWriterFileCopier : BufferFileCopier
    {
        public BufferBinaryWriterFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "BufferBinaryWriterFileCopier";
        }

        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;
            Reader = file.GetStreamToRead();
            Writer = file.GetStreamToWrite(FileMode.Create);
            var writer1 = new BinaryWriter(Writer);

            readBytes = 0;
            FileBytesTransferred = 0;

            unsafe
            {
                while (true)
                {
                    WaitForResumeOrCancel();
                    if (IsSkipRequested()) break;

                    //Dinamic bufferSize. Dynamic buffer size.
                    readBytes = Reader.Read(buffer, 0, (int)(file.Size > BufferSize ? BufferSize : file.Size));
                    if (readBytes <= 0) break;

                    WaitForResumeOrCancel();
                    if (IsSkipRequested()) break;
                    writer1.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;
                }
            }

            Reader.Close();
            Reader.Dispose();

            if (ConsumeSkipRequested() && Writer != null && Writer.CanWrite)
                Writer.SetLength(FileBytesTransferred);

            Writer.Flush();
            Writer.Close();
            Writer.Dispose();
        }
        public override FileCopier Clone()
        {
            return new BufferBinaryWriterFileCopier(BufferSize);
        }

    }
    /// <summary>
    /// Recomended 1 Mb of BufferSize.
    /// Based on FileStream.
    /// SetLength of new FileStream.
    /// Remove Flush call, use only dispose.
    /// If source & destination is on different HDDs, then reading and writing are processed respectively in parallel by separate threads.
    /// But if they are on the same HDDs, then reading is processed until the user-defined buffer size is filled. Once filled, data can be written anywhere immediately.
    /// </summary>
    public class FasterBufferFileCopier : BufferFileCopier
    {
        System.IO.DriveInfo driverInfo;
        string fileName;
        public FasterBufferFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "FasterBufferFileCopier";
        }

        public override void ReleaseResources()
        {
            Reader.Dispose();
            Writer.Dispose();

            //if (CurrentFile != null)
            //    File.Delete(CurrentFile.DestinyPath);
        }
        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;
            fileName = PathDisplayHelper.GetRootForDriveInfo(file.DestinyPath);
            driverInfo = new System.IO.DriveInfo(fileName);

            using (Reader = file.GetStreamToRead())
            {
                using (Writer = file.GetStreamToWrite(FileMode.Create))
                    {
                        //if (driverInfo.DriveType == DriveType.Removable && driverInfo.DriveFormat=="FAT32")
                        //{
                        //    ExecuteCopySafe(driverInfo, file);
                        //}
                        //else ExecuteCopy(driverInfo, file);

                        ExecuteCopy(driverInfo, file);
                    }
                //}
            }

        }

        /// <summary>
        /// Execute copy without set the file length before copy.
        /// </summary>
        /// <param name="driverInfo"></param>
        /// <param name="file"></param>
        protected virtual void ExecuteCopySafe(System.IO.DriveInfo driverInfo, FileDataInfo file)
        {
            if (driverInfo.TotalFreeSpace < file.Size)
            {
                ReleaseResources();

                throw new NotEnoughSpaceException(string.Format("{0} ({1})", driverInfo.VolumeLabel, driverInfo.Name));
            }

            //Writer.SetLength(Reader.Length);

            readBytes = 0;
            FileBytesTransferred = 0;

            while (true)
            {
                WaitForResumeOrCancel();
                if (IsSkipRequested()) break;

                readBytes = Reader.Read(buffer, 0, BufferSize);
                if (readBytes <= 0) break;

                WaitForResumeOrCancel();
                if (IsSkipRequested()) break;

                // Request the lock, and block until it is obtained.
                Monitor.Enter(Writer);

                //If operation was cancelled, them the writer has been released
                //and this statement produce: The Thread has been aborted exception.
                //Them capture the exception and terminate the current file copy process.
                //try
                //{
                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;
                //}
                //catch (Exception ex)
                //{
                //    System.Windows.MessageBox.Show(ex.Message + " in FileCopier.Copy");
                //}

                // Ensure that the lock is released.
                Monitor.Exit(Writer);

            }

            CurrentFile = null;
        }

        Exception executeCopyException;
        protected virtual void ExecuteCopy(System.IO.DriveInfo driverInfo, FileDataInfo file)
        {
            if (driverInfo.TotalFreeSpace < file.Size)
            {
                ReleaseResources();

                throw new NotEnoughSpaceException(string.Format("{0} ({1})", driverInfo.VolumeLabel, driverInfo.Name));
            }

            Writer.SetLength(Reader.Length);

            readBytes = 0;
            FileBytesTransferred = 0;

            executeCopyException = null;

            try
            {

                while (true)
                {
                    WaitForResumeOrCancel();
                    if (IsSkipRequested()) break;

                    readBytes = Reader.Read(buffer, 0, BufferSize);
                    if (readBytes <= 0) break;

                    WaitForResumeOrCancel();
                    if (IsSkipRequested()) break;
                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;
                }

            }
            catch (Exception ex)
            {
                executeCopyException = ex;
            }
            finally
            {
                if (ConsumeSkipRequested() && Writer != null && Writer.CanWrite)
                {
                    try { Writer.SetLength(FileBytesTransferred); } catch { }
                }
                ReleaseResources();
                CurrentFile = null;
                if(executeCopyException!=null)throw executeCopyException;
            }
        }
        public override FileCopier Clone()
        {
            return new FasterBufferFileCopier(BufferSize);
        }
    }


    /// <summary>
    /// Change the buffer dynamic.
    /// </summary>
    public class DynamicBufferFileCopier : FasterBufferFileCopier
    {
        public DynamicBufferFileCopier(int bufferSize)
           : base(bufferSize)
        {
            Name = "DynamicBufferFileCopier";
        }

        protected override void ExecuteCopy(System.IO.DriveInfo driverInfo, FileDataInfo file)
        {
            buffer = new byte[CalculateBufferSize(file.Size)];
            base.ExecuteCopy(driverInfo, file);
        }

        protected virtual int CalculateBufferSize(long size)
        {
            //return (int)Math.Log(size, 2) + 1;
            return 4*1024*1024;//1Mb
        }
    }

    /// <summary>
    /// Best Gain Copy Performance by Mdonatas
    /// </summary>
    public class MdonatasBufferFileCopier : BufferFileCopier
    {
        byte[] bufferForWrite, bufferForRead;

        public MdonatasBufferFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "MdonatasBufferFileCopier";

            bufferForWrite = new byte[bufferSize];
            bufferForRead = new byte[bufferSize];
        }

        public override void ReleaseResources()
        {
            Reader.Dispose();
            Writer.Dispose();

            if (CurrentFile != null)
                System.IO.File.Delete(LongPathHelper.Normalize(CurrentFile.DestinyPath));
        }
        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;
            var driverInfo = new System.IO.DriveInfo(PathDisplayHelper.GetRootForDriveInfo(file.DestinyPath));
            string tmp_file = "";

            using (Reader = file.GetStreamToRead())
            {
                if (file.DestinyPath.Length > 248)
                {
                    //Create the file in short name
                    tmp_file = Path.Combine(file.DestinyDirectoryPath, "tmp_a19");

                    using (Writer = new FileStream(LongPathHelper.Normalize(tmp_file), FileMode.Create, FileAccess.Write))
                    {
                        //if (driverInfo.DriveType == DriveType.Removable)
                        //{
                        //    ExecuteCopySafe(driverInfo, file);
                        //}
                        //else ExecuteCopy(driverInfo, file);
                        ExecuteCopy(driverInfo, file);
                    }

                    //Move the file to it's original destinyPath
                    if (System.IO.File.Exists(LongPathHelper.Normalize(file.DestinyPath)))
                        System.IO.File.Delete(LongPathHelper.Normalize(file.DestinyPath));
                    System.IO.File.Move(LongPathHelper.Normalize(tmp_file), LongPathHelper.Normalize(file.DestinyPath));
                }
                else
                {
                    using (Writer = file.GetStreamToWrite(FileMode.Create))
                    {
                        //if (driverInfo.DriveType == DriveType.Removable && driverInfo.DriveFormat=="FAT32")
                        //{
                        //    ExecuteCopySafe(driverInfo, file);
                        //}
                        //else ExecuteCopy(driverInfo, file);

                        ExecuteCopy(driverInfo, file);
                    }
                }
            }

        }

        /// <summary>
        /// Execute copy without set the file length before copy.
        /// </summary>
        /// <param name="driverInfo"></param>
        /// <param name="file"></param>
        private void ExecuteCopySafe(System.IO.DriveInfo driverInfo, FileDataInfo file)
        {
            if (driverInfo.TotalFreeSpace < file.Size)
            {
                ReleaseResources();

                throw new NotEnoughSpaceException(string.Format("{0} ({1})", driverInfo.VolumeLabel, driverInfo.Name));
            }

            //Writer.SetLength(Reader.Length);

            readBytes = 0;
            FileBytesTransferred = 0;

            while (true)
            {
                WaitForResumeOrCancel();
                if (IsSkipRequested()) break;

                readBytes = Reader.Read(buffer, 0, BufferSize);
                if (readBytes <= 0) break;

                WaitForResumeOrCancel();
                if (IsSkipRequested()) break;

                // Request the lock, and block until it is obtained.
                Monitor.Enter(Writer);

                //If operation was cancelled, them the writer has been released
                //and this statement produce: The Thread has been aborted exception.
                //Them capture the exception and terminate the current file copy process.
                try
                {
                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;
                }
                catch (Exception)
                {
                    return;
                }

                // Ensure that the lock is released.
                Monitor.Exit(Writer);

            }

            CurrentFile = null;
        }
        private void ExecuteCopy(System.IO.DriveInfo driverInfo, FileDataInfo file)
        {
            if (driverInfo.TotalFreeSpace < file.Size)
            {
                ReleaseResources();

                throw new NotEnoughSpaceException(string.Format("{0} ({1})", driverInfo.VolumeLabel, driverInfo.Name));
            }

            Writer.SetLength(Reader.Length);

            readBytes = 0;
            FileBytesTransferred = 0;

            while (true)
            {
                WaitForResumeOrCancel();
                if (IsSkipRequested()) break;

                readBytes = Reader.Read(buffer, 0, BufferSize);
                if (readBytes <= 0) break;

                WaitForResumeOrCancel();
                if (IsSkipRequested()) break;

                // Request the lock, and block until it is obtained.
                Monitor.Enter(Writer);

                //If operation was cancelled, them the writer has been released
                //and this statement produce: The Thread has been aborted exception.
                //Them capture the exception and terminate the current file copy process.
                try
                {
                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;
                }
                catch (Exception)
                {
                    return;
                }

                // Ensure that the lock is released.
                Monitor.Exit(Writer);

            }

            CurrentFile = null;
        }
        public override FileCopier Clone()
        {
            return new FasterBufferFileCopier(BufferSize);
        }
    }
    /// <summary>
    /// Only Copy the files, not it's content
    /// </summary>
    public class NotCopyFileCopier : FasterBufferFileCopier
    {
        public NotCopyFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "NotCopyFileCopier";
        }

        protected override void ExecuteCopySafe(System.IO.DriveInfo driverInfo, FileDataInfo file)
        {
            CurrentFile = null;
        }
        protected override void ExecuteCopy(System.IO.DriveInfo driverInfo, FileDataInfo file)
        {
            CurrentFile = null;
        }
        public override FileCopier Clone()
        {
            return new NotCopyFileCopier(BufferSize);
        }
    }

    public class SaveBufferFileCopier : BufferFileCopier
    {
        public SaveBufferFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "SaveBufferFileCopier";
        }

        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;
            Reader = file.GetStreamToRead();

            using (Writer = file.GetStreamToWrite(FileMode.Create))
            {
                readBytes = 0;
                FileBytesTransferred = 0;

                while (true)
                {
                    WaitForResumeOrCancel();
                    if (IsSkipRequested()) break;

                    readBytes = Reader.Read(buffer, 0, BufferSize);
                    if (readBytes <= 0) break;

                    WaitForResumeOrCancel();
                    if (IsSkipRequested()) break;

                    Writer.SetLength(Writer.Length + readBytes);

                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;

                }
            }

            if (ConsumeSkipRequested() && Writer != null && Writer.CanWrite)
                Writer.SetLength(FileBytesTransferred);

            Reader.Dispose();
        }
        public override FileCopier Clone()
        {
            return new SaveBufferFileCopier(BufferSize);
        }
    }
}
