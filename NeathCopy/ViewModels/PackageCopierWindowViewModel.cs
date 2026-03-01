using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeathCopy.ViewModels
{
    public sealed class SourceEntry : ViewModelBase
    {
        public string Path { get; }
        public bool IsFolder { get; }
        public string Type => IsFolder ? "Folder" : "File";

        public SourceEntry(string path, bool isFolder)
        {
            Path = path;
            IsFolder = isFolder;
        }
    }

    public sealed class DestinationEntry : ViewModelBase
    {
        private long bytesCopied;

        public string Path { get; }

        public long BytesCopied
        {
            get => bytesCopied;
            set => SetProperty(ref bytesCopied, value);
        }

        public DestinationEntry(string path)
        {
            Path = path;
        }
    }

    internal sealed class PackageCopyFile
    {
        public string SourcePath { get; set; }
        public string RelativePath { get; set; }
        public long Length { get; set; }
    }

    public sealed class PackageCopierWindowViewModel : ViewModelBase
    {
        private sealed class DestinationWriter : IDisposable
        {
            public DestinationEntry Destination { get; set; }
            public FileStream Stream { get; set; }

            public void Dispose()
            {
                Stream?.Dispose();
            }
        }

        private readonly RelayCommand startCommand;
        private readonly RelayCommand cancelCommand;
        private CancellationTokenSource copyCts;
        private bool isRunning;
        private string threadsText;
        private double overallProgress;
        private string statusText;
        private string logText;
        private long totalBytesToWrite;
        private long totalBytesWritten;

        public ObservableCollection<SourceEntry> Sources { get; }
        public ObservableCollection<DestinationEntry> Destinations { get; }

        public ICommand StartCommand => startCommand;
        public ICommand CancelCommand => cancelCommand;

        public string ThreadsText
        {
            get => threadsText;
            set => SetProperty(ref threadsText, value);
        }

        public double OverallProgress
        {
            get => overallProgress;
            private set => SetProperty(ref overallProgress, value);
        }

        public string StatusText
        {
            get => statusText;
            private set => SetProperty(ref statusText, value);
        }

        public string LogText
        {
            get => logText;
            private set => SetProperty(ref logText, value);
        }

        public bool IsRunning
        {
            get => isRunning;
            private set
            {
                if (!SetProperty(ref isRunning, value))
                    return;

                OnPropertyChanged(nameof(IsIdle));
                startCommand.RaiseCanExecuteChanged();
                cancelCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsIdle => !IsRunning;

        public PackageCopierWindowViewModel()
        {
            Sources = new ObservableCollection<SourceEntry>();
            Destinations = new ObservableCollection<DestinationEntry>();
            ThreadsText = "2";
            StatusText = "Idle";
            LogText = string.Empty;

            startCommand = new RelayCommand(() => _ = StartCopyAsync(), () => !IsRunning);
            cancelCommand = new RelayCommand(CancelCopy, () => IsRunning);
        }

        public void AddSources(IEnumerable<string> paths)
        {
            if (paths == null)
                return;

            foreach (var path in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                var full = SafeGetFullPath(path);
                if (string.IsNullOrWhiteSpace(full))
                    continue;

                var isFolder = Directory.Exists(full);
                var isFile = File.Exists(full);
                if (!isFolder && !isFile)
                    continue;

                if (Sources.Any(s => s.Path.Equals(full, StringComparison.OrdinalIgnoreCase)))
                    continue;

                Sources.Add(new SourceEntry(full, isFolder));
            }
        }

        public void RemoveSources(IEnumerable<SourceEntry> items)
        {
            if (items == null)
                return;

            foreach (var item in items.ToList())
                Sources.Remove(item);
        }

        public void ClearSources()
        {
            Sources.Clear();
        }

        public void AddDestinations(IEnumerable<string> paths)
        {
            if (paths == null)
                return;

            foreach (var path in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                var full = SafeGetFullPath(path);
                if (string.IsNullOrWhiteSpace(full))
                    continue;

                if (!Directory.Exists(full))
                    continue;

                if (Destinations.Any(d => d.Path.Equals(full, StringComparison.OrdinalIgnoreCase)))
                    continue;

                Destinations.Add(new DestinationEntry(full));
            }
        }

        public void RemoveDestinations(IEnumerable<DestinationEntry> items)
        {
            if (items == null)
                return;

            foreach (var item in items.ToList())
                Destinations.Remove(item);
        }

        public void ClearDestinations()
        {
            Destinations.Clear();
        }

        public void CancelCopy()
        {
            if (!IsRunning)
                return;

            AppendLog("Cancel requested.");
            copyCts?.Cancel();
        }

        private async Task StartCopyAsync()
        {
            if (IsRunning)
                return;

            if (!TryParseThreads(out var threads))
            {
                AppendLog("Threads must be an integer >= 1.");
                StatusText = "Invalid Threads value.";
                return;
            }

            var sourceSnapshot = Sources.ToList();
            var destinationSnapshot = Destinations.ToList();

            if (sourceSnapshot.Count == 0)
            {
                AppendLog("No sources selected.");
                StatusText = "No sources.";
                return;
            }

            if (destinationSnapshot.Count == 0)
            {
                AppendLog("No destinations selected.");
                StatusText = "No destinations.";
                return;
            }

            foreach (var destination in destinationSnapshot)
                destination.BytesCopied = 0;

            OverallProgress = 0;
            totalBytesToWrite = 0;
            totalBytesWritten = 0;
            IsRunning = true;
            StatusText = "Building copy plan...";
            copyCts = new CancellationTokenSource();

            try
            {
                var token = copyCts.Token;
                var files = await Task.Run(() => BuildCopyPlan(sourceSnapshot, token), token);
                if (files.Count == 0)
                {
                    AppendLog("No files found in selected sources.");
                    StatusText = "Nothing to copy.";
                    return;
                }

                totalBytesToWrite = files.Sum(f => f.Length) * destinationSnapshot.Count;
                totalBytesWritten = 0;
                UpdateOverallProgress();

                var totalBatches = (int)Math.Ceiling(destinationSnapshot.Count / (double)threads);
                for (var batchIndex = 0; batchIndex < totalBatches; batchIndex++)
                {
                    token.ThrowIfCancellationRequested();

                    var batch = destinationSnapshot
                        .Skip(batchIndex * threads)
                        .Take(threads)
                        .ToList();

                    StatusText = string.Format(
                        "Copying batch {0}/{1} ({2} destination(s))",
                        batchIndex + 1,
                        totalBatches,
                        batch.Count);
                    AppendLog(StatusText);

                    await CopyBatchAsync(files, batch, token);
                }

                OverallProgress = 100;
                StatusText = "Completed.";
                AppendLog("Package copy completed successfully.");
            }
            catch (OperationCanceledException)
            {
                StatusText = "Canceled.";
                AppendLog("Package copy canceled.");
            }
            catch (Exception ex)
            {
                StatusText = "Failed.";
                AppendLog("Copy failed: " + ex.Message);
            }
            finally
            {
                IsRunning = false;
                copyCts?.Dispose();
                copyCts = null;
            }
        }

        private static List<PackageCopyFile> BuildCopyPlan(IEnumerable<SourceEntry> sources, CancellationToken token)
        {
            var files = new List<PackageCopyFile>();

            foreach (var source in sources)
            {
                token.ThrowIfCancellationRequested();

                if (!source.IsFolder)
                {
                    if (!File.Exists(source.Path))
                        continue;

                    files.Add(new PackageCopyFile
                    {
                        SourcePath = source.Path,
                        RelativePath = System.IO.Path.GetFileName(source.Path),
                        Length = new FileInfo(source.Path).Length
                    });
                    continue;
                }

                if (!Directory.Exists(source.Path))
                    continue;

                var root = source.Path.TrimEnd(
                    System.IO.Path.DirectorySeparatorChar,
                    System.IO.Path.AltDirectorySeparatorChar);

                foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                {
                    token.ThrowIfCancellationRequested();

                    var relative = file.Substring(root.Length).TrimStart(
                        System.IO.Path.DirectorySeparatorChar,
                        System.IO.Path.AltDirectorySeparatorChar);

                    files.Add(new PackageCopyFile
                    {
                        SourcePath = file,
                        RelativePath = relative,
                        Length = new FileInfo(file).Length
                    });
                }
            }

            return files;
        }

        private async Task CopyBatchAsync(
            IReadOnlyList<PackageCopyFile> files,
            IReadOnlyList<DestinationEntry> destinations,
            CancellationToken token)
        {
            foreach (var file in files)
            {
                token.ThrowIfCancellationRequested();
                await CopyFileToDestinationsAsync(file, destinations, token);
            }
        }

        private async Task CopyFileToDestinationsAsync(
            PackageCopyFile file,
            IReadOnlyList<DestinationEntry> destinations,
            CancellationToken token)
        {
            const int bufferSize = 1024 * 1024;
            var writers = new List<DestinationWriter>(destinations.Count);

            try
            {
                foreach (var destination in destinations)
                {
                    var destinationFile = System.IO.Path.Combine(destination.Path, file.RelativePath);
                    var destinationDir = System.IO.Path.GetDirectoryName(destinationFile);
                    if (!string.IsNullOrWhiteSpace(destinationDir))
                        Directory.CreateDirectory(destinationDir);

                    var destinationStream = new FileStream(
                        destinationFile,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize,
                        FileOptions.Asynchronous);

                    writers.Add(new DestinationWriter
                    {
                        Destination = destination,
                        Stream = destinationStream
                    });
                }

                using (var sourceStream = new FileStream(
                    file.SourcePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize,
                    FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    var buffer = new byte[bufferSize];
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        var read = await sourceStream.ReadAsync(buffer, 0, buffer.Length, token);
                        if (read == 0)
                            break;

                        var writeTasks = writers
                            .Select(w => w.Stream.WriteAsync(buffer, 0, read, token))
                            .ToArray();

                        await Task.WhenAll(writeTasks);

                        foreach (var writer in writers)
                            writer.Destination.BytesCopied += read;

                        Interlocked.Add(ref totalBytesWritten, read * writers.Count);
                        UpdateOverallProgress();
                    }
                }
            }
            finally
            {
                foreach (var writer in writers)
                    writer.Dispose();
            }
        }

        private void UpdateOverallProgress()
        {
            if (totalBytesToWrite <= 0)
            {
                OverallProgress = 0;
                return;
            }

            var progress = (double)totalBytesWritten / totalBytesToWrite * 100.0;
            if (progress < 0)
                progress = 0;
            if (progress > 100)
                progress = 100;

            OverallProgress = progress;
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var builder = new StringBuilder(LogText);
            if (builder.Length > 0)
                builder.AppendLine();
            builder.AppendFormat("[{0:HH:mm:ss}] {1}", DateTime.Now, message.Trim());
            LogText = builder.ToString();
        }

        private bool TryParseThreads(out int value)
        {
            if (!int.TryParse(ThreadsText, out value))
            {
                value = 0;
                return false;
            }

            if (value < 1)
            {
                value = 0;
                return false;
            }

            return true;
        }

        private static string SafeGetFullPath(string path)
        {
            try
            {
                return System.IO.Path.GetFullPath(path);
            }
            catch
            {
                return null;
            }
        }
    }
}
