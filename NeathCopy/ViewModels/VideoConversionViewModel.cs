using NeathCopy.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace NeathCopy.ViewModels
{
    public class VideoConversionItemViewModel : ViewModelBase
    {
        private string status;
        private double progress;
        private string outputPath;

        public string FileName { get; }
        public string FullPath { get; }

        public string Status
        {
            get => status;
            set => SetProperty(ref status, value);
        }

        public double Progress
        {
            get => progress;
            set => SetProperty(ref progress, value);
        }

        public string OutputPath
        {
            get => outputPath;
            set => SetProperty(ref outputPath, value);
        }

        public VideoConversionItemViewModel(string path)
        {
            FullPath = path;
            FileName = Path.GetFileName(path);
            Status = "Pending";
            Progress = 0;
        }
    }

    public class VideoConversionViewModel : ViewModelBase
    {
        private readonly VideoConversionService service;
        private readonly Dispatcher dispatcher;
        private CancellationTokenSource conversionCts;
        private DispatcherTimer uiTimer;
        private readonly ConcurrentQueue<string> logQueue;
        private readonly Dictionary<string, VideoConversionProgress> progressQueue;
        private readonly object progressLock = new object();
        private string selectedOutputFormat;
        private string bitrate;
        private string resolution;
        private string outputFolder;
        private string maxParallelText;
        private double overallProgress;
        private string logText;
        private bool isConverting;

        private readonly RelayCommand startCommand;
        private readonly RelayCommand cancelCommand;
        private int runId;

        public ObservableCollection<VideoConversionItemViewModel> Items { get; }
        public ObservableCollection<string> OutputFormats { get; }

        public string SelectedOutputFormat
        {
            get => selectedOutputFormat;
            set => SetProperty(ref selectedOutputFormat, value);
        }

        public string Bitrate
        {
            get => bitrate;
            set => SetProperty(ref bitrate, value);
        }

        public string Resolution
        {
            get => resolution;
            set => SetProperty(ref resolution, value);
        }

        public string OutputFolder
        {
            get => outputFolder;
            set => SetProperty(ref outputFolder, value);
        }

        public string MaxParallelText
        {
            get => maxParallelText;
            set => SetProperty(ref maxParallelText, value);
        }

        public double OverallProgress
        {
            get => overallProgress;
            set => SetProperty(ref overallProgress, value);
        }

        public string LogText
        {
            get => logText;
            set => SetProperty(ref logText, value);
        }

        public bool IsConverting
        {
            get => isConverting;
            private set
            {
                if (SetProperty(ref isConverting, value))
                {
                    startCommand.RaiseCanExecuteChanged();
                    cancelCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanStart => Items.Count > 0 && !IsConverting;

        public ICommand AddFilesCommand { get; }
        public ICommand ClearListCommand { get; }
        public ICommand BrowseOutputFolderCommand { get; }
        public ICommand StartConversionCommand => startCommand;
        public ICommand CancelCommand => cancelCommand;

        public VideoConversionViewModel()
        {
            service = new VideoConversionService();
            dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

            logQueue = new ConcurrentQueue<string>();
            progressQueue = new Dictionary<string, VideoConversionProgress>(StringComparer.OrdinalIgnoreCase);

            Items = new ObservableCollection<VideoConversionItemViewModel>();
            Items.CollectionChanged += Items_CollectionChanged;

            OutputFormats = new ObservableCollection<string> { "mpg", "mp4", "avi" };
            SelectedOutputFormat = "mpg";
            MaxParallelText = "2";
            LogText = string.Empty;

            AddFilesCommand = new RelayCommand(OpenFileDialog);
            ClearListCommand = new RelayCommand(ClearItems);
            BrowseOutputFolderCommand = new RelayCommand(BrowseOutputFolder);

            startCommand = new RelayCommand(() => _ = StartConversionAsync(), () => CanStart);
            cancelCommand = new RelayCommand(CancelConversion, () => IsConverting);
        }

        public void AddFiles(IEnumerable<string> paths)
        {
            if (paths == null)
                return;

            var candidates = paths
                .Where(File.Exists)
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var path in candidates)
            {
                if (!IsVideoFile(path))
                {
                    AppendLog(string.Format("Skipped non-video file: {0}", path));
                    continue;
                }

                if (Items.Any(i => i.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    AppendLog(string.Format("Skipped duplicate: {0}", path));
                    continue;
                }

                Items.Add(new VideoConversionItemViewModel(path));
            }
        }

        public void RemoveItems(IEnumerable<VideoConversionItemViewModel> items)
        {
            if (items == null)
                return;

            foreach (var item in items.ToList())
                Items.Remove(item);
        }

        public void CancelConversion()
        {
            conversionCts?.Cancel();
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CanStart));
            startCommand.RaiseCanExecuteChanged();
        }

        private void OpenFileDialog()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.mpeg;*.mpg;*.m4v;*.webm|All Files|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
                AddFiles(dialog.FileNames);
        }

        private void BrowseOutputFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    OutputFolder = dialog.SelectedPath;
            }
        }

        private void ClearItems()
        {
            Items.Clear();
            OverallProgress = 0;
        }

        private async Task StartConversionAsync()
        {
            if (IsConverting)
                return;

            if (!service.FfmpegExists)
            {
                AppendLog(string.Format("ffmpeg not found at: {0}", service.FfmpegPath));
                MessageBox.Show("ffmpeg.exe not found. Ensure it is copied to the output folder.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(OutputFolder) && !Directory.Exists(OutputFolder))
            {
                AppendLog(string.Format("Output folder does not exist: {0}", OutputFolder));
                MessageBox.Show("Output folder does not exist.");
                return;
            }

            if (!TryParseMaxParallel(out var maxParallel))
            {
                maxParallel = 2;
                MaxParallelText = "2";
            }

            var requests = new List<VideoConversionRequest>();

            foreach (var item in Items)
            {
                item.Status = "Pending";
                item.Progress = 0;
                item.OutputPath = BuildOutputPath(item.FullPath);
                requests.Add(new VideoConversionRequest
                {
                    InputPath = item.FullPath,
                    OutputPath = item.OutputPath
                });
            }

            IsConverting = true;
            var currentRun = ++runId;
            conversionCts = new CancellationTokenSource();
            StartUiTimer();

            AppendLog("Starting conversion...");

            var settings = new VideoConversionSettings
            {
                OutputFormat = SelectedOutputFormat,
                Bitrate = Bitrate,
                Resolution = Resolution
            };

            var canceled = false;
            try
            {
                await service.ConvertAsync(
                    requests,
                    settings,
                    maxParallel,
                    conversionCts.Token,
                    EnqueueProgress,
                    EnqueueLog)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                EnqueueLog(string.Format("Conversion error: {0}", ex.Message));
            }
            finally
            {
                canceled = conversionCts.IsCancellationRequested;
                dispatcher.Invoke(() =>
                {
                    StopUiTimer();
                    FlushUiUpdates();
                    IsConverting = false;
                });
                conversionCts?.Dispose();
                conversionCts = null;
            }

            dispatcher.Invoke(() =>
            {
                if (currentRun != runId)
                    return;

                if (canceled)
                {
                    MessageBox.Show("Conversion canceled.");
                    return;
                }

                if (Items.All(i => i.Status == "Completed"))
                    MessageBox.Show("Conversion completed successfully.");
                else
                    MessageBox.Show("Conversion finished with errors. Check the log for details.");
            });
        }

        private void ApplyProgress(VideoConversionProgress progress)
        {
            var item = Items.FirstOrDefault(i => i.FullPath.Equals(progress.InputPath, StringComparison.OrdinalIgnoreCase));
            if (item == null)
                return;

            if (!string.IsNullOrWhiteSpace(progress.Status))
                item.Status = progress.Status;

            if (progress.Percent.HasValue)
                item.Progress = progress.Percent.Value;

            if (!string.IsNullOrWhiteSpace(progress.Message))
                AppendLog(progress.Message);

            UpdateOverallProgress();
        }

        private void EnqueueLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            logQueue.Enqueue(message.Trim());
        }

        private void EnqueueProgress(VideoConversionProgress progress)
        {
            if (progress == null || string.IsNullOrWhiteSpace(progress.InputPath))
                return;

            lock (progressLock)
            {
                progressQueue[progress.InputPath] = progress;
            }
        }

        private void StartUiTimer()
        {
            if (uiTimer != null)
                return;

            uiTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Background, (s, e) =>
            {
                FlushUiUpdates();
            }, dispatcher);
            uiTimer.Start();
        }

        private void StopUiTimer()
        {
            if (uiTimer == null)
                return;

            uiTimer.Stop();
            uiTimer = null;
        }

        private void FlushUiUpdates()
        {
            var newLogs = new List<string>();
            while (logQueue.TryDequeue(out var line))
                newLogs.Add(line);

            if (newLogs.Count > 0)
            {
                var builder = new StringBuilder(LogText);
                foreach (var line in newLogs)
                {
                    if (builder.Length > 0)
                        builder.AppendLine();
                    builder.Append(line);
                }
                LogText = builder.ToString();
            }

            List<VideoConversionProgress> progressItems = null;
            lock (progressLock)
            {
                if (progressQueue.Count > 0)
                {
                    progressItems = progressQueue.Values.ToList();
                    progressQueue.Clear();
                }
            }

            if (progressItems != null)
            {
                foreach (var progress in progressItems)
                    ApplyProgress(progress);
            }
        }

        private void UpdateOverallProgress()
        {
            OverallProgress = Items.Count == 0 ? 0 : Items.Average(i => i.Progress);
        }

        private string BuildOutputPath(string sourcePath)
        {
            var format = string.IsNullOrWhiteSpace(SelectedOutputFormat) ? "mpg" : SelectedOutputFormat.Trim().TrimStart('.');
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            var folder = string.IsNullOrWhiteSpace(OutputFolder) ? Path.GetDirectoryName(sourcePath) : OutputFolder;

            return Path.Combine(folder, string.Format("{0}.{1}", fileName, format));
        }

        private bool TryParseMaxParallel(out int value)
        {
            if (int.TryParse(MaxParallelText, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                if (value < 1)
                {
                    value = 1;
                    return true;
                }

                return true;
            }

            value = 0;
            return false;
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var builder = new StringBuilder(LogText);
            if (builder.Length > 0)
                builder.AppendLine();
            builder.Append(message.Trim());
            LogText = builder.ToString();
        }

        private bool IsVideoFile(string path)
        {
            var extension = Path.GetExtension(path);
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            switch (extension.ToLowerInvariant())
            {
                case ".mp4":
                case ".avi":
                case ".mkv":
                case ".mov":
                case ".wmv":
                case ".flv":
                case ".mpeg":
                case ".mpg":
                case ".m4v":
                case ".webm":
                    return true;
                default:
                    return false;
            }
        }
    }
}
