using LongPathDirectory = LongPath.Directory;
using NeathCopyEngine.DataTools;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeathCopyEngine.CopyHandlers
{
    public sealed class MultiDestinationFileCopier : FileCopier
    {
        public int BufferSize { get; set; }

        public MultiDestinationFileCopier(int bufferSize)
        {
            BufferSize = bufferSize > 0 ? bufferSize : 1024 * 1024;
            Name = "MultiDestinationFileCopier";
            Description = "Read once per chunk and write to multiple destinations.";
        }

        public override FileCopier Clone()
        {
            return new MultiDestinationFileCopier(BufferSize);
        }

        public override void CopyFile(FileDataInfo file)
        {
            throw new NotSupportedException("Use CopyFileToDestinationsAsync for multi-destination copy.");
        }

        public async Task<bool> CopyFileToDestinationsAsync(
            MultiDestinationCopyItem item,
            IReadOnlyList<string> destinationRoots,
            int threads)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (destinationRoots == null || destinationRoots.Count == 0)
                throw new ArgumentException("At least one destination root is required.", nameof(destinationRoots));

            var batchSize = Math.Max(1, threads);
            var sourcePath = LongPathHelper.Normalize(item.SourcePath);

            CurrentFile = new FileDataInfo
            {
                FullName = item.SourceDisplayPath ?? item.SourcePath,
                DestinyPath = Path.Combine(destinationRoots[0], item.RelativePath),
                Name = Path.GetFileName(item.SourceDisplayPath ?? item.SourcePath),
                Size = item.Length
            };
            FileBytesTransferred = 0;
            var totalBeforeFile = TotalBytesTransferred;
            var totalBatches = (int)Math.Ceiling(destinationRoots.Count / (double)batchSize);

            for (var batchIndex = 0; batchIndex < totalBatches; batchIndex++)
            {
                WaitForResumeOrCancel();
                if (IsSkipRequested())
                    break;

                var offset = batchIndex * batchSize;
                var batch = destinationRoots.Skip(offset).Take(batchSize).ToList();
                await CopyToBatchAsync(sourcePath, item.RelativePath, batch, readInBatch =>
                {
                    var normalizedProgress = ((batchIndex * item.Length) + readInBatch) / totalBatches;
                    if (normalizedProgress > item.Length)
                        normalizedProgress = item.Length;

                    FileBytesTransferred = normalizedProgress;
                    TotalBytesTransferred = totalBeforeFile + normalizedProgress;
                }).ConfigureAwait(false);
            }

            var skipped = ConsumeSkipRequested();
            if (!skipped)
            {
                FileBytesTransferred = item.Length;
                TotalBytesTransferred = totalBeforeFile + item.Length;
            }

            return skipped;
        }

        public override void Cancel()
        {
            Interlocked.Exchange(ref skipRequested, 1);
        }

        public override void Skip()
        {
            Interlocked.Exchange(ref skipRequested, 1);
        }

        private async Task CopyToBatchAsync(
            string sourcePath,
            string relativePath,
            IReadOnlyList<string> destinationRoots,
            Action<long> onReadProgress)
        {
            var writers = new List<FileStream>(destinationRoots.Count);
            try
            {
                foreach (var root in destinationRoots)
                {
                    var destinationFile = LongPathHelper.Normalize(Path.Combine(root, relativePath));
                    var destinationDirectory = Path.GetDirectoryName(destinationFile);
                    LongPathDirectory.CreateDirectoriesInPath(destinationDirectory);

                    writers.Add(new FileStream(
                        destinationFile,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        BufferSize,
                        FileOptions.Asynchronous));
                }

                using (var reader = new FileStream(
                    sourcePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    BufferSize,
                    FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    var buffer = new byte[BufferSize];
                    long readInBatch = 0;
                    while (true)
                    {
                        WaitForResumeOrCancel();
                        if (IsSkipRequested())
                            break;

                        var read = await reader.ReadAsync(buffer, 0, buffer.Length, CancellationToken).ConfigureAwait(false);
                        if (read <= 0)
                            break;

                        WaitForResumeOrCancel();
                        if (IsSkipRequested())
                            break;

                        var writes = writers.Select(w => w.WriteAsync(buffer, 0, read, CancellationToken)).ToArray();
                        await Task.WhenAll(writes).ConfigureAwait(false);
                        readInBatch += read;
                        onReadProgress?.Invoke(readInBatch);
                    }
                }
            }
            finally
            {
                foreach (var writer in writers)
                {
                    try { writer.Dispose(); } catch { }
                }
            }
        }
    }
}
