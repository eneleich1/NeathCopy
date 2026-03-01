using LongPathDirectory = LongPath.Directory;
using LongPathFile = LongPath.File;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeathCopy.ViewModels
{
    public sealed class SourceEntry : ViewModelBase
    {
        public string Path { get; }
        public string IoPath { get; }
        public bool IsFolder { get; }
        public string Type => IsFolder ? "Folder" : "File";

        public SourceEntry(string path, bool isFolder)
        {
            Path = path;
            IoPath = LongPathHelper.Normalize(path);
            IsFolder = isFolder;
        }
    }

    public sealed class DestinationEntry : ViewModelBase
    {
        public string Path { get; }
        public string IoPath { get; }

        public DestinationEntry(string path)
        {
            Path = path;
            IoPath = LongPathHelper.Normalize(path);
        }
    }

    internal sealed class PackageCopyFile
    {
        public string SourcePath { get; set; }
        public string SourceDisplayPath { get; set; }
        public string RelativePath { get; set; }
        public long Length { get; set; }
    }

    internal sealed class SpeedSample
    {
        public double TimeSeconds { get; set; }
        public long Bytes { get; set; }
    }

    public sealed class PackageCopierWindowViewModel : ViewModelBase
    {
        private sealed class DestinationWriter : IDisposable
        {
            public FileStream Stream { get; set; }

            public void Dispose()
            {
                Stream?.Dispose();
            }
        }

        private readonly Dispatcher uiDispatcher;
        private readonly RelayCommand startCommand;
        private readonly RelayCommand cancelCommand;
        private readonly RelayCommand pauseResumeCommand;
        private readonly RelayCommand skipCommand;
        private readonly ManualResetEventSlim pauseGate = new ManualResetEventSlim(true);
        private readonly Stopwatch speedStopwatch = new Stopwatch();
        private readonly Queue<SpeedSample> speedSamples = new Queue<SpeedSample>();

        private CancellationTokenSource copyCts;
        private bool isRunning;
        private bool isPaused;
        private volatile bool skipCurrentFileRequested;
        private volatile bool pauseAfterSkipRequested;
        private string threadsText;
        private string statusText;
        private string logText;
        private string currentFileName;
        private string currentFromPath;
        private string currentToPath;
        private string speedText;
        private long fileBytesTransferred;
        private long fileBytesTotal;
        private long overallBytesTransferred;
        private long overallBytesTotal;
        private double filePercent;
        private double overallPercent;
        private int fileIndex;
        private int fileCount;
        private string remainingTimeText;
        private string fileProgressLeftText;
        private string fileProgressCenterText;
        private string fileProgressRightText;
        private string overallProgressLeftText;
        private string overallProgressCenterText;
        private string overallProgressRightText;
        private long lastUiProgressUpdateMs;

        public ObservableCollection<SourceEntry> Sources { get; }
        public ObservableCollection<DestinationEntry> Destinations { get; }

        public ICommand StartCommand => startCommand;
        public ICommand CancelCommand => cancelCommand;
        public ICommand PauseResumeCommand => pauseResumeCommand;
        public ICommand SkipCommand => skipCommand;

        public string ThreadsText
        {
            get => threadsText;
            set => SetProperty(ref threadsText, value);
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
                pauseResumeCommand.RaiseCanExecuteChanged();
                skipCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsIdle => !IsRunning;

        public bool IsPaused
        {
            get => isPaused;
            private set
            {
                if (!SetProperty(ref isPaused, value))
                    return;

                OnPropertyChanged(nameof(PauseResumeButtonText));
            }
        }

        public string PauseResumeButtonText => IsPaused ? "Resume" : "Pause";

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

        public string CurrentFileName
        {
            get => currentFileName;
            private set => SetProperty(ref currentFileName, value);
        }

        public string CurrentFromPath
        {
            get => currentFromPath;
            private set => SetProperty(ref currentFromPath, value);
        }

        public string CurrentToPath
        {
            get => currentToPath;
            private set => SetProperty(ref currentToPath, value);
        }

        public string SpeedText
        {
            get => speedText;
            private set => SetProperty(ref speedText, value);
        }

        public long FileBytesTransferred
        {
            get => fileBytesTransferred;
            private set => SetProperty(ref fileBytesTransferred, value);
        }

        public long FileBytesTotal
        {
            get => fileBytesTotal;
            private set => SetProperty(ref fileBytesTotal, value);
        }

        public long OverallBytesTransferred
        {
            get => overallBytesTransferred;
            private set => SetProperty(ref overallBytesTransferred, value);
        }

        public long OverallBytesTotal
        {
            get => overallBytesTotal;
            private set => SetProperty(ref overallBytesTotal, value);
        }

        public double FilePercent
        {
            get => filePercent;
            private set => SetProperty(ref filePercent, value);
        }

        public double OverallPercent
        {
            get => overallPercent;
            private set => SetProperty(ref overallPercent, value);
        }

        public int FileIndex
        {
            get => fileIndex;
            private set => SetProperty(ref fileIndex, value);
        }

        public int FileCount
        {
            get => fileCount;
            private set => SetProperty(ref fileCount, value);
        }

        public string RemainingTimeText
        {
            get => remainingTimeText;
            private set => SetProperty(ref remainingTimeText, value);
        }

        public string FileProgressLeftText
        {
            get => fileProgressLeftText;
            private set => SetProperty(ref fileProgressLeftText, value);
        }

        public string FileProgressCenterText
        {
            get => fileProgressCenterText;
            private set => SetProperty(ref fileProgressCenterText, value);
        }

        public string FileProgressRightText
        {
            get => fileProgressRightText;
            private set => SetProperty(ref fileProgressRightText, value);
        }

        public string OverallProgressLeftText
        {
            get => overallProgressLeftText;
            private set => SetProperty(ref overallProgressLeftText, value);
        }

        public string OverallProgressCenterText
        {
            get => overallProgressCenterText;
            private set => SetProperty(ref overallProgressCenterText, value);
        }

        public string OverallProgressRightText
        {
            get => overallProgressRightText;
            private set => SetProperty(ref overallProgressRightText, value);
        }

        public PackageCopierWindowViewModel()
        {
            uiDispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

            Sources = new ObservableCollection<SourceEntry>();
            Destinations = new ObservableCollection<DestinationEntry>();
            ThreadsText = "2";
            StatusText = "Idle";
            LogText = string.Empty;

            startCommand = new RelayCommand(() => _ = StartCopyAsync(), () => !IsRunning);
            cancelCommand = new RelayCommand(CancelCopy, () => IsRunning);
            pauseResumeCommand = new RelayCommand(TogglePauseResume, () => IsRunning);
            skipCommand = new RelayCommand(SkipCurrentFile, () => IsRunning);

            ResetTransferPanel();
        }

        public void AddSources(IEnumerable<string> paths)
        {
            if (paths == null)
                return;

            foreach (var path in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                var fullPath = GetFullPath(path);
                if (string.IsNullOrWhiteSpace(fullPath))
                    continue;

                var ioPath = LongPathHelper.Normalize(fullPath);
                var isFolder = Directory.Exists(ioPath) || Directory.Exists(fullPath);
                var isFile = File.Exists(ioPath) || File.Exists(fullPath);
                if (!isFolder && !isFile)
                    continue;

                if (Sources.Any(s => s.Path.Equals(fullPath, StringComparison.OrdinalIgnoreCase)))
                    continue;

                Sources.Add(new SourceEntry(fullPath, isFolder));
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
                var fullPath = GetFullPath(path);
                if (string.IsNullOrWhiteSpace(fullPath))
                    continue;

                var ioPath = LongPathHelper.Normalize(fullPath);
                if (!Directory.Exists(ioPath) && !Directory.Exists(fullPath))
                    continue;

                if (Destinations.Any(d => d.Path.Equals(fullPath, StringComparison.OrdinalIgnoreCase)))
                    continue;

                Destinations.Add(new DestinationEntry(fullPath));
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
            pauseGate.Set();
            copyCts?.Cancel();
        }

        private void TogglePauseResume()
        {
            if (!IsRunning)
                return;

            if (IsPaused)
            {
                pauseGate.Set();
                pauseAfterSkipRequested = false;
                IsPaused = false;
                StatusText = "Running.";
                AppendLog("Resumed.");
            }
            else
            {
                pauseGate.Reset();
                IsPaused = true;
                StatusText = "Paused.";
                AppendLog("Paused.");
            }
        }

        private void SkipCurrentFile()
        {
            if (!IsRunning)
                return;

            skipCurrentFileRequested = true;
            if (IsPaused)
            {
                pauseAfterSkipRequested = true;
                pauseGate.Set();
            }
            AppendLog("Skip requested for current file.");
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

            IsRunning = true;
            IsPaused = false;
            skipCurrentFileRequested = false;
            pauseGate.Set();
            copyCts = new CancellationTokenSource();

            RunOnUi(() =>
            {
                StatusText = "Building copy plan...";
                LogText = string.Empty;
                ResetTransferPanel();
            });

            speedSamples.Clear();
            speedStopwatch.Reset();
            speedStopwatch.Start();
            speedSamples.Enqueue(new SpeedSample { TimeSeconds = 0, Bytes = 0 });
            lastUiProgressUpdateMs = 0;

            try
            {
                var token = copyCts.Token;
                var files = await Task.Run(() => BuildCopyPlan(sourceSnapshot, token), token).ConfigureAwait(false);
                if (files.Count == 0)
                {
                    RunOnUi(() =>
                    {
                        StatusText = "Nothing to copy.";
                    });
                    AppendLog("No files found in selected sources.");
                    return;
                }

                var totalBatches = (int)Math.Ceiling(destinationSnapshot.Count / (double)threads);
                var totalFileOperations = files.Count * totalBatches;
                var totalBytes = files.Sum(f => f.Length) * destinationSnapshot.Count;

                RunOnUi(() =>
                {
                    FileCount = totalFileOperations;
                    OverallBytesTotal = totalBytes;
                    OverallBytesTransferred = 0;
                    OverallPercent = 0;
                    RemainingTimeText = "--:--:--";
                    OverallProgressLeftText = string.Format("{0} of {1}", FormatBytes(0), FormatBytes(totalBytes));
                    OverallProgressCenterText = "0.0%";
                    OverallProgressRightText = "Remaining: --:--:--";
                    SpeedText = "Speed: 0.0 MB/s";
                });

                var fileOperationIndex = 0;
                for (var batchIndex = 0; batchIndex < totalBatches; batchIndex++)
                {
                    token.ThrowIfCancellationRequested();

                    var batch = destinationSnapshot
                        .Skip(batchIndex * threads)
                        .Take(threads)
                        .ToList();

                    AppendLog(string.Format(
                        "Starting batch {0}/{1} with {2} destination(s).",
                        batchIndex + 1,
                        totalBatches,
                        batch.Count));

                    foreach (var file in files)
                    {
                        token.ThrowIfCancellationRequested();

                        fileOperationIndex++;
                        await CopyFileToDestinationsAsync(file, batch, token, fileOperationIndex, totalFileOperations).ConfigureAwait(false);
                    }
                }

                RunOnUi(() =>
                {
                    StatusText = "Completed.";
                    OverallPercent = 100;
                    OverallProgressCenterText = "100.0%";
                    OverallProgressRightText = "Remaining: 00:00:00";
                    RemainingTimeText = "00:00:00";
                });
                AppendLog("Package copy completed successfully.");
            }
            catch (OperationCanceledException)
            {
                RunOnUi(() =>
                {
                    StatusText = "Canceled.";
                });
                AppendLog("Package copy canceled.");
            }
            catch (Exception ex)
            {
                RunOnUi(() =>
                {
                    StatusText = "Failed.";
                });
                AppendLog("Copy failed: " + ex.Message);
            }
            finally
            {
                speedStopwatch.Stop();
                pauseGate.Set();
                copyCts?.Dispose();
                copyCts = null;

                RunOnUi(() =>
                {
                    IsPaused = false;
                    IsRunning = false;
                });
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
                    var sourceFile = source.IoPath;
                    if (!LongPathFile.Exists(sourceFile))
                        continue;

                    files.Add(new PackageCopyFile
                    {
                        SourcePath = sourceFile,
                        SourceDisplayPath = source.Path,
                        RelativePath = Path.GetFileName(source.Path),
                        Length = new FileInfo(sourceFile).Length
                    });
                    continue;
                }

                var rootDisplay = source.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var rootIo = source.IoPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (!LongPathDirectory.Exists(rootIo))
                    continue;

                var rootName = Path.GetFileName(rootDisplay);
                foreach (var file in EnumerateFilesRecursiveLongPath(rootIo, token))
                {
                    token.ThrowIfCancellationRequested();

                    var relativeWithin = file.StartsWith(rootIo, StringComparison.OrdinalIgnoreCase)
                        ? file.Substring(rootIo.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        : Path.GetFileName(file);
                    var relativePath = string.IsNullOrWhiteSpace(relativeWithin)
                        ? rootName
                        : Path.Combine(rootName, relativeWithin);

                    files.Add(new PackageCopyFile
                    {
                        SourcePath = file,
                        SourceDisplayPath = LongPathToDisplay(file),
                        RelativePath = relativePath,
                        Length = new FileInfo(file).Length
                    });
                }
            }

            return files;
        }

        private async Task CopyFileToDestinationsAsync(
            PackageCopyFile file,
            IReadOnlyList<DestinationEntry> destinations,
            CancellationToken token,
            int fileOperationIndex,
            int totalFileOperations)
        {
            const int bufferSize = 1024 * 1024;
            var writers = new List<DestinationWriter>(destinations.Count);
            var fileTotalBytes = file.Length * destinations.Count;
            long fileTransferred = 0;
            var skipped = false;

            UpdateCurrentFileContext(file, destinations, fileOperationIndex, totalFileOperations, fileTransferred, fileTotalBytes, true);

            try
            {
                foreach (var destination in destinations)
                {
                    var destinationFile = CombineNormalizedPath(destination.IoPath, file.RelativePath);
                    var destinationDir = Path.GetDirectoryName(destinationFile);
                    LongPathDirectory.CreateDirectoriesInPath(destinationDir);

                    var destinationStream = new FileStream(
                        destinationFile,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize,
                        FileOptions.Asynchronous);

                    writers.Add(new DestinationWriter { Stream = destinationStream });
                }

                var normalizedSource = LongPathHelper.Normalize(file.SourcePath);
                using (var sourceStream = new FileStream(
                    normalizedSource,
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
                        pauseGate.Wait(token);

                        if (skipCurrentFileRequested)
                        {
                            skipCurrentFileRequested = false;
                            if (pauseAfterSkipRequested)
                            {
                                pauseAfterSkipRequested = false;
                                pauseGate.Reset();
                            }
                            skipped = true;
                            break;
                        }

                        var read = await sourceStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                        if (read == 0)
                            break;

                        var writeTasks = writers
                            .Select(w => w.Stream.WriteAsync(buffer, 0, read, token))
                            .ToArray();
                        await Task.WhenAll(writeTasks).ConfigureAwait(false);

                        var deltaBytes = read * writers.Count;
                        fileTransferred += deltaBytes;
                        Interlocked.Add(ref overallBytesTransferred, deltaBytes);

                        UpdateCurrentFileContext(file, destinations, fileOperationIndex, totalFileOperations, fileTransferred, fileTotalBytes, false);
                    }
                }
            }
            finally
            {
                foreach (var writer in writers)
                    writer.Dispose();
            }

            if (skipped)
            {
                AppendLog(string.Format("Skipped file: {0}", file.SourceDisplayPath));
            }
            else
            {
                UpdateCurrentFileContext(file, destinations, fileOperationIndex, totalFileOperations, fileTotalBytes, fileTotalBytes, true);
            }
        }

        private void UpdateCurrentFileContext(
            PackageCopyFile file,
            IReadOnlyList<DestinationEntry> destinations,
            int fileOperationIndex,
            int totalFileOperations,
            long fileTransferred,
            long fileTotal,
            bool forceUiUpdate)
        {
            var totalTransferred = Interlocked.Read(ref overallBytesTransferred);
            var totalBytes = OverallBytesTotal;
            var nowMs = speedStopwatch.ElapsedMilliseconds;

            if (!forceUiUpdate && (nowMs - lastUiProgressUpdateMs) < 120)
                return;

            lastUiProgressUpdateMs = nowMs;

            var speed = UpdateAndGetSpeed(totalTransferred);
            var remaining = ComputeRemainingTime(totalTransferred, totalBytes, speed);

            var filePercentValue = fileTotal <= 0 ? 0 : (fileTransferred * 100d / fileTotal);
            if (filePercentValue < 0) filePercentValue = 0;
            if (filePercentValue > 100) filePercentValue = 100;

            var overallPercentValue = totalBytes <= 0 ? 0 : (totalTransferred * 100d / totalBytes);
            if (overallPercentValue < 0) overallPercentValue = 0;
            if (overallPercentValue > 100) overallPercentValue = 100;

            var currentTo = BuildDestinationLabel(destinations);
            RunOnUi(() =>
            {
                CurrentFileName = Path.GetFileName(file.SourceDisplayPath);
                CurrentFromPath = file.SourceDisplayPath;
                CurrentToPath = currentTo;

                FileIndex = fileOperationIndex;
                FileCount = totalFileOperations;

                FileBytesTransferred = fileTransferred;
                FileBytesTotal = fileTotal;
                OverallBytesTransferred = totalTransferred;

                FilePercent = filePercentValue;
                OverallPercent = overallPercentValue;

                RemainingTimeText = remaining;
                SpeedText = "Speed: " + FormatSpeed(speed);

                FileProgressLeftText = string.Format("{0} of {1}", FormatBytes(fileTransferred), FormatBytes(fileTotal));
                FileProgressCenterText = string.Format("{0:0.0}%", filePercentValue);
                FileProgressRightText = string.Format("{0} of {1}", fileOperationIndex, totalFileOperations);

                OverallProgressLeftText = string.Format("{0} of {1}", FormatBytes(totalTransferred), FormatBytes(totalBytes));
                OverallProgressCenterText = string.Format("{0:0.0}%", overallPercentValue);
                OverallProgressRightText = "Remaining: " + remaining;
            });
        }

        private double UpdateAndGetSpeed(long totalTransferredBytes)
        {
            var now = speedStopwatch.Elapsed.TotalSeconds;
            speedSamples.Enqueue(new SpeedSample
            {
                TimeSeconds = now,
                Bytes = totalTransferredBytes
            });

            while (speedSamples.Count > 2 && (now - speedSamples.Peek().TimeSeconds) > 3.0)
                speedSamples.Dequeue();

            if (speedSamples.Count < 2)
                return 0;

            var first = speedSamples.Peek();
            var last = speedSamples.Last();
            var elapsed = last.TimeSeconds - first.TimeSeconds;
            if (elapsed <= 0.0001)
                return 0;

            var deltaBytes = last.Bytes - first.Bytes;
            if (deltaBytes <= 0)
                return 0;

            return deltaBytes / elapsed;
        }

        private static string ComputeRemainingTime(long transferred, long total, double speedBytesPerSecond)
        {
            if (total <= 0 || transferred >= total)
                return "00:00:00";

            if (speedBytesPerSecond <= 0.1)
                return "--:--:--";

            var remainingSeconds = (total - transferred) / speedBytesPerSecond;
            if (remainingSeconds < 0)
                remainingSeconds = 0;

            var span = TimeSpan.FromSeconds(remainingSeconds);
            if (span.TotalHours >= 100)
                return string.Format("{0:000}:{1:00}:{2:00}", (int)span.TotalHours, span.Minutes, span.Seconds);

            return string.Format("{0:00}:{1:00}:{2:00}", (int)span.TotalHours, span.Minutes, span.Seconds);
        }

        private void ResetTransferPanel()
        {
            CurrentFileName = "-";
            CurrentFromPath = "-";
            CurrentToPath = "-";
            SpeedText = "Speed: 0.0 MB/s";

            FileBytesTransferred = 0;
            FileBytesTotal = 0;
            OverallBytesTransferred = 0;
            OverallBytesTotal = 0;
            FilePercent = 0;
            OverallPercent = 0;

            FileIndex = 0;
            FileCount = 0;
            RemainingTimeText = "--:--:--";

            FileProgressLeftText = string.Format("{0} of {1}", FormatBytes(0), FormatBytes(0));
            FileProgressCenterText = "0.0%";
            FileProgressRightText = "0 of 0";
            OverallProgressLeftText = string.Format("{0} of {1}", FormatBytes(0), FormatBytes(0));
            OverallProgressCenterText = "0.0%";
            OverallProgressRightText = "Remaining: --:--:--";
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            RunOnUi(() =>
            {
                var builder = new StringBuilder(LogText);
                if (builder.Length > 0)
                    builder.AppendLine();
                builder.AppendFormat("[{0:HH:mm:ss}] {1}", DateTime.Now, message.Trim());
                LogText = builder.ToString();
            });
        }

        private static string FormatBytes(long bytes)
        {
            return new MySize(bytes).ToString();
        }

        private static string FormatSpeed(double bytesPerSecond)
        {
            if (bytesPerSecond <= 0)
                return "0.0 MB/s";

            var mbPerSecond = bytesPerSecond / (1024d * 1024d);
            if (mbPerSecond >= 1024d)
                return string.Format("{0:0.0} GB/s", mbPerSecond / 1024d);

            if (mbPerSecond >= 1d)
                return string.Format("{0:0.0} MB/s", mbPerSecond);

            var kbPerSecond = bytesPerSecond / 1024d;
            if (kbPerSecond >= 1d)
                return string.Format("{0:0.0} KB/s", kbPerSecond);

            return string.Format("{0:0.0} B/s", bytesPerSecond);
        }

        private static string BuildDestinationLabel(IReadOnlyList<DestinationEntry> destinations)
        {
            if (destinations == null || destinations.Count == 0)
                return "-";

            if (destinations.Count == 1)
                return destinations[0].Path;

            return string.Format("{0} (+{1} more)", destinations[0].Path, destinations.Count - 1);
        }

        private static IEnumerable<string> EnumerateFilesRecursiveLongPath(string rootIo, CancellationToken token)
        {
            var pending = new Stack<string>();
            pending.Push(rootIo);

            while (pending.Count > 0)
            {
                token.ThrowIfCancellationRequested();
                var current = pending.Pop();
                var entries = LongPathDirectory.GetFileSystemEntries(current);
                if (entries == null || entries.Length == 0)
                    continue;

                foreach (var rawEntry in entries)
                {
                    token.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(rawEntry))
                        continue;

                    var entry = rawEntry;
                    if (!entry.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase) && !Path.IsPathRooted(entry))
                        continue;

                    if (LongPathDirectory.Exists(entry))
                    {
                        pending.Push(entry);
                        continue;
                    }

                    if (LongPathFile.Exists(entry))
                        yield return entry;
                }
            }
        }

        private static string CombineNormalizedPath(string normalizedRoot, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(normalizedRoot))
                return normalizedRoot;

            if (string.IsNullOrWhiteSpace(relativePath))
                return normalizedRoot;

            var root = normalizedRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var relative = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var combined = root + Path.DirectorySeparatorChar + relative;

            if (combined.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
                return combined;

            return LongPathHelper.Normalize(combined);
        }

        private static string LongPathToDisplay(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
                return @"\\" + path.Substring(8);

            if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
                return path.Substring(4);

            return path;
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

        private static string GetFullPath(string path)
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

        private void RunOnUi(Action action)
        {
            if (action == null)
                return;

            if (uiDispatcher.CheckAccess())
            {
                action();
                return;
            }

            uiDispatcher.Invoke(action);
        }
    }
}
