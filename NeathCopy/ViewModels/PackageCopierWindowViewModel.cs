using LongPathDirectory = LongPath.Directory;
using LongPathFile = LongPath.File;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

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

    public sealed class PackageCopierStartEventArgs : EventArgs
    {
        public RequestInfo RequestInfo { get; }

        public PackageCopierStartEventArgs(RequestInfo requestInfo)
        {
            RequestInfo = requestInfo;
        }
    }

    public sealed class PackageCopierWindowViewModel : ViewModelBase
    {
        private readonly RelayCommand startCommand;
        private string threadsText;
        private string statusText;
        private string logText;

        public ObservableCollection<SourceEntry> Sources { get; }
        public ObservableCollection<DestinationEntry> Destinations { get; }

        public ICommand StartCommand => startCommand;

        public event EventHandler<PackageCopierStartEventArgs> StartRequested;

        public string ThreadsText
        {
            get => threadsText;
            set => SetProperty(ref threadsText, value);
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

        public PackageCopierWindowViewModel()
        {
            Sources = new ObservableCollection<SourceEntry>();
            Destinations = new ObservableCollection<DestinationEntry>();
            ThreadsText = "2";
            StatusText = "Idle";
            LogText = string.Empty;

            startCommand = new RelayCommand(StartCopy);
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
            // Package Copier is configuration-only in this workflow.
        }

        private void StartCopy()
        {
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

            var plan = BuildCopyPlan(sourceSnapshot);
            if (plan.Count == 0)
            {
                AppendLog("No files found in selected sources.");
                StatusText = "Nothing to copy.";
                return;
            }

            var request = new MultiDestinationCopyRequest
            {
                Items = plan,
                DestinationRoots = destinationSnapshot.Select(d => d.Path).ToList(),
                Threads = threads
            };

            var info = new RequestInfo
            {
                Operation = "copy",
                Destiny = request.DestinationRoots[0],
                Sources = sourceSnapshot.Select(s => s.Path).ToList(),
                Content = RquestContent.MultiDestinationPackageCopy,
                MultiDestinationRequest = request
            };

            StatusText = "Starting VisualCopy...";
            AppendLog(string.Format("Prepared {0} files for {1} destinations.", request.Items.Count, request.DestinationRoots.Count));
            StartRequested?.Invoke(this, new PackageCopierStartEventArgs(info));
        }

        private static List<MultiDestinationCopyItem> BuildCopyPlan(IEnumerable<SourceEntry> sources)
        {
            var files = new List<MultiDestinationCopyItem>();

            foreach (var source in sources)
            {
                if (!source.IsFolder)
                {
                    var sourceFile = source.IoPath;
                    if (!LongPathFile.Exists(sourceFile))
                        continue;

                    files.Add(new MultiDestinationCopyItem
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
                foreach (var file in EnumerateFilesRecursiveLongPath(rootIo))
                {
                    var relativeWithin = file.StartsWith(rootIo, StringComparison.OrdinalIgnoreCase)
                        ? file.Substring(rootIo.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        : Path.GetFileName(file);

                    var relativePath = string.IsNullOrWhiteSpace(relativeWithin)
                        ? rootName
                        : Path.Combine(rootName, relativeWithin);

                    files.Add(new MultiDestinationCopyItem
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

        private static IEnumerable<string> EnumerateFilesRecursiveLongPath(string rootIo)
        {
            var pending = new Stack<string>();
            pending.Push(rootIo);

            while (pending.Count > 0)
            {
                var current = pending.Pop();
                var entries = LongPathDirectory.GetFileSystemEntries(current);
                if (entries == null || entries.Length == 0)
                    continue;

                foreach (var rawEntry in entries)
                {
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
    }
}
