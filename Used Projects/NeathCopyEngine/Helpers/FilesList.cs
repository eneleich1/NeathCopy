using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.DataTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace NeathCopyEngine.Helpers
{
    public class FilesList
    {
        #region Fields

        public enum DiscoverState
        {
            Normal,Discovering
        }

        public DiscoverState DiscoveringState { get; set; }
        public string FileNameOnDisk { get; set; }

        public List<string> Sources { get; set; }

        /// <summary>
        ///Get List of DiscoverFile to Copy.
        /// </summary>
        public ObservableCollection<FileDataInfo> Files { get; set; }
        /// <summary>
        /// Get the List of all Emptys directories.
        /// </summary>
        public List<DirectoryDataInfo> EmptyDirectories { get; set; }
        /// <summary>
        /// Get the Count of files in the list.
        /// </summary>
        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                OnPropertyChanged("Count");
            }
        }
        int count;
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
        /// <summary>
        /// Get the Total size of all files in the list to copy.
        /// </summary>
        /// 
        public MySize Size { get; set; }
        /// <summary>
        /// Get the current Size of list without include TransactionSize.
        /// </summary>
        public MySize TrueSize
        {
            get
            {
                return ExecutingTransaction ? (Size - TransactionSize) : Size;
            }
        }
        public MySize TrueFilesToCopySize
        {
            get
            {
                return ExecutingTransaction ? (SizeOfFilesToCopy - TransactionSize) : SizeOfFilesToCopy;
            }
        }
        /// <summary>
        /// Get the size of all files wich are not be copieds
        /// </summary>
        public MySize SizeOfFilesToCopy{ get; set; }
        /// <summary>
        /// Get the size of all files wich was copieds.
        /// </summary>
        public MySize SizeOfCopiedsFiles { get; set; }
        /// <summary>
        /// Get the list of sources Directories. Used in Move operation.
        /// </summary>
        public List<string> SourcesDirectories { get;  set; }
        /// <summary>
        /// 
        /// </summary>
        public string Operation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Destinys { get; set; }
        /// <summary>
        /// Get or set the index of file wich is copiying.
        /// </summary>
        public int Index { get; set; }

        //Transaction Fields
        public bool ExecutingTransaction { get;  set; }
        public int StartTransactionIndex { get;  set; }
        public int TransactionFilesCount { get;  set; }
        public MySize TransactionSize { get;  set; }

        private static Mutex mut = new Mutex();

        #endregion

        #region Events

        public delegate void DiscoverFinishedEventHandler();
        /// <summary>
        /// Occurs when discovery operation has finished.
        /// </summary>
        public event DiscoverFinishedEventHandler DiscoverFinished;
        protected void RaiseDiscoverFinished()
        {
            if (DiscoverFinished != null)
                DiscoverFinished();
        }

        #endregion

        public FilesList()
        {
            Files = new ObservableCollection<FileDataInfo>();
            EmptyDirectories = new List<DirectoryDataInfo>();
            SourcesDirectories = new List<string>();
            Destinys = new List<string>();
            Sources = new List<string>();
        }

        #region Methods

        public void Discover(RequestInfo info, Dispatcher dispatcher)
        {
            try
            {
                DiscoveringState = DiscoverState.Discovering;
                Sources = Sources.Concat(info.Sources).ToList();
                DataInfo.DestinyPathsNames.Clear();

                //Set some fields
                Destinys.Add(info.Destiny);
                Operation = info.Operation.ToLower();

                foreach (var dataInfo in info.Sources.Select(s => DataInfo.Parse(s, info.Destiny)))
                {
                    var files = dataInfo.GetFiles(ref count);

                    foreach (var file in files)
                    {
                        dispatcher.Invoke(() => { Files.Add(file); });
                        
                        Size += file.Size;
                        SizeOfFilesToCopy += file.Size;

                        if (ExecutingTransaction)
                        {
                            TransactionSize += file.Size;
                            TransactionFilesCount++;
                        }
                    }

                    EmptyDirectories = EmptyDirectories.Concat(dataInfo.EmptyDirs).ToList();

                    //If this dataInfo is a emptydirectori
                    if (dataInfo is DirectoryDataInfo && (files.Count == 0))
                    {
                        EmptyDirectories.Add((DirectoryDataInfo)dataInfo);
                    }
                }

                SourcesDirectories = new List<string>(info.Sources.Where(s => System.IO.Directory.Exists(s)));
                DiscoveringState = DiscoverState.Normal;
                RaiseDiscoverFinished();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopyEngine", "FilesList", "Discover"));

                using (var w = new StreamWriter(new FileStream("Errors Log.txt", FileMode.Append, FileAccess.Write)))
                {
                    w.WriteLine("-------------------------------");
                    w.WriteLine(System.DateTime.Now);
                    w.WriteLine(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "Discover"));
                }
            }
        }

        /// <summary>
        /// Discover all files into specific RequestInfo info.
        /// </summary>
        /// <param name="info"></param>
        public void Discover(RequestInfo info)
        {
            try
            {
                DiscoveringState = DiscoverState.Discovering;
                Sources = Sources.Concat(info.Sources).ToList();
                DataInfo.DestinyPathsNames.Clear();

                //Set some fields
                Destinys.Add(info.Destiny);
                Operation = info.Operation.ToLower();

                foreach (var dataInfo in info.Sources.Select(s => DataInfo.Parse(s, info.Destiny)))
                {
                    var files = dataInfo.GetFiles(ref count);

                    foreach (var file in files)
                    {
                        Files.Add(file);
                        Size += file.Size;
                        SizeOfFilesToCopy += file.Size;

                        if (ExecutingTransaction)
                        {
                            TransactionSize += file.Size;
                            TransactionFilesCount++;
                        }
                    }

                    EmptyDirectories = EmptyDirectories.Concat(dataInfo.EmptyDirs).ToList();

                    //If this dataInfo is a emptydirectori
                    if (dataInfo is DirectoryDataInfo && (files.Count == 0))
                    {
                        EmptyDirectories.Add((DirectoryDataInfo)dataInfo);
                    }
                }

                SourcesDirectories = new List<string>(info.Sources.Where(s => System.IO.Directory.Exists(s)));
                DiscoveringState = DiscoverState.Normal;
                RaiseDiscoverFinished();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopyEngine", "FilesList", "Discover"));

                using (var w = new StreamWriter(new FileStream("Errors Log.txt", FileMode.Append, FileAccess.Write)))
                {
                    w.WriteLine("-------------------------------");
                    w.WriteLine(System.DateTime.Now);
                    w.WriteLine(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "Discover"));
                }
            }
        }

        #region Save List Methdos

        public void SaveListOneDestiny(string fileName)
        {
            try
            {
                var files = Files.Select(f => new FileOnList { From = f.FullName, To = f.DestinyPath}).ToList();

                var list = new SerializableFilesList { Files = files, MultipleDestiny = false , Operation=this.Operation};

                MySerializer.Serialize(list, typeof(SerializableFilesList), fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "SaveList"));
            }
        }
        public static SerializableFilesList LoadListOneDestiny(string fileName)
        {
            try
            {
                if (!LongPath.File.Exists(fileName))
                    throw new ArgumentException("The specific file not exist");

                return (SerializableFilesList)MySerializer.Deserialize(typeof(SerializableFilesList), fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "Load"));
                return null;
            }

        }

        public void SaveList(string fileName)
        {
            try
            {
                var files = Files.Select(f => new FileOnList { From = f.FullName, To=f.DestinyPath}).ToList();

                var list = new SerializableFilesList { Files = files, MultipleDestiny = true, Operation = this.Operation };

                MySerializer.Serialize(list, typeof(SerializableFilesList), fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "SaveList"));
            }
        }
        public static SerializableFilesList Load(string fileName)
        {
            try {

                if (!LongPath.File.Exists(fileName))
                    throw new ArgumentException("The specific file not exist");

                return (SerializableFilesList)MySerializer.Deserialize(typeof(SerializableFilesList), fileName);
            }
            catch(Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "Load"));
                return null;
            }

        }

        public void SaveCompressedList(string fileName)
        {
            try
            {
                var files = Files.Select(f => new FileOnList { From= f.FullName, To = f.DestinyPath}).ToList();

                var list = new SerializableFilesList { Files = files, MultipleDestiny = true, Operation = this.Operation };

                MySerializer.SerializeCompressed(list, typeof(SerializableFilesList), fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "SaveCompressedList"));
            }
        }
        public static SerializableFilesList LoadCompressed(string fileName)
        {
            try
            {
                if (!LongPath.File.Exists(fileName))
                    throw new ArgumentException("The specific file not exist");

                return (SerializableFilesList)MySerializer.DeserializeCompressed(typeof(SerializableFilesList), fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "LoadCompressed"));
                return null;
            }

        }

        #endregion  

        /// <summary>
        /// Beginig a AddFiles Transaction by recording the start index wich files will addeds and
        /// the size of addeds files.
        /// </summary>
        public void BegingTransaction()
        {
            StartTransactionIndex = Count;
            ExecutingTransaction = true;
        }
        /// <summary>
        /// Ensure that current transaction if any, add all it's files to this list.
        /// </summary>
        public void CommitTransaction()
        {
            if (ExecutingTransaction)
            {
                ResetTransaction();
            }
        }

        public void ResetTransaction()
        {
            StartTransactionIndex = 0;
            TransactionSize = new MySize();
            TransactionFilesCount = 0;
            ExecutingTransaction = false;
        }

        /// <summary>
        /// Discard all files addes by the current transaction.
        /// </summary>
        public void DiscardTransaction()
        {
            try
            {
                Monitor.Enter(Files);

                Files.RemoveRange(StartTransactionIndex, TransactionFilesCount);
                Size -= TransactionSize;
                SizeOfFilesToCopy -= TransactionSize;
                Count -= TransactionFilesCount;

                ResetTransaction();

                Monitor.Exit(Files);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "DiscardTransaction"));
            }
        }
        /// <summary>
        /// Move files to the bigining of Files list.
        /// </summary>
        /// <param name="files"></param>
        public void MoveToBegining(IEnumerable<FileDataInfo> files)
        {
            try
            {
                Monitor.Enter(Files);

                foreach (var f in files.Reverse())
                    Files.Remove(f);

                //Index make the work
                foreach (var f in files.Reverse())
                    Files.Insert(Index + 1, f);

                Monitor.Exit(Files);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "MoveToBegining"));
            }
        }
        public void MoveUp(IEnumerable<FileDataInfo> files)
        {
            try
            {
                Monitor.Enter(Files);

                if (files.Count() == 0) return;

                int index = 0;
                FileDataInfo aux = null;

                foreach (var f in files)
                {
                    index = Files.IndexOf(f);

                    if (index > 0)
                    {
                        if (Files[index - 1].CopyState != CopyState.Waiting)
                        {
                            Monitor.Exit(this);
                            return;
                        }

                        aux = Files[index - 1];
                        Files[index - 1] = f;
                        Files[index] = aux;
                    }
                }

                Monitor.Exit(Files);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "MoveUp"));
            }
        }
        public void MoveDown(IEnumerable<FileDataInfo> files)
        {
            try
            {
                Monitor.Enter(Files);

                if (files.Count() == 0) return;

                int index = Files.IndexOf(files.First());

                //Nothing to move
                if (files.Count() == Files.Count - index) return;

                FileDataInfo aux = null;

                foreach (var f in files.Reverse())
                {
                    index = Files.IndexOf(f);

                    if (index < Files.Count - 1)
                    {
                        aux = Files[index + 1];
                        Files[index + 1] = f;
                        Files[index] = aux;
                    }
                }

                Monitor.Exit(Files);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "MoveDown"));
            }
        }
        public void MoveToEnd(IEnumerable<FileDataInfo> files)
        {
            try
            {
                Monitor.Enter(Files);

                foreach (var f in files)
                    Files.Remove(f);

                foreach (var f in files)
                    Files.Add(f);

                Monitor.Exit(Files);

            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "MoveToEnd"));
            }
        }
        /// <summary>
        /// Move files to the bigining of Files list.
        /// </summary>
        /// <param name="files"></param>
        public void Remove(IEnumerable<FileDataInfo> files)
        {
            try
            {
                Monitor.Enter(Files);

                foreach (var f in files)
                {
                    Files.Remove(f);
                    Count--;
                    Size -= f.Size;

                    if (f.CopyState == CopyState.Waiting)
                        SizeOfFilesToCopy -= f.Size;
                }

                Monitor.Exit(Files);

            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "Remove"));
            }
        }
        public long RemoveAt(int index)
        {
            try
            {
                Monitor.Enter(this);
                long length = Files[index].Size;

                if (Files[index].CopyState != CopyState.Processing)
                {
                    Files.RemoveAt(index);
                    Count--;
                    Size -= length;
                    SizeOfFilesToCopy -= length;
                }

                Monitor.Exit(Files);

                return length;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "RemoveAt"));
                return 0;
            }
        }

        /// <summary>
        /// Remove the last files of this list and return it's size.
        /// </summary>
        /// <returns></returns>
        public long RemoveLast()
        {
            try
            {

                long length = Files[Files.Count - 1].Size;

                Monitor.Enter(Files);

                if (Files.Last().CopyState == CopyState.Waiting)
                {
                    Files.RemoveAt(Files.Count - 1);
                    Count--;
                    Size -= length;
                    SizeOfFilesToCopy -= length;
                }

                Monitor.Exit(Files);

                return length;

            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "FilesList", "RemoveLast"));
                return 0;
            }
        }

        #endregion

    }

    public class FileOnList
    {
        public string From;
        public string To;
    }
    public class SerializableFilesList
    {
        public string Operation;
        public bool MultipleDestiny;
        public List<FileOnList> Files = new List<FileOnList>();
    }

    public enum TranssactionState
    {
        None,Active
    }
    public class Transsaction
    {
        public TranssactionState State { get; set; }
        /// <summary>
        /// The Size of transsaction
        /// </summary>
        public MySize Size { get; set; }
        /// <summary>
        /// The count of files including in the Transsaction
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Mark of the start Index of Transsaction.
        /// </summary>
        public int StartIndex { get; set; }
        /// <summary>
        /// Mark of th End Index of Transsaction.
        /// </summary>
        public int EndIndex { get; set; }

        public void Reset()
        {
            State = TranssactionState.None;
            Count = 0;
            Size = new MySize();
            StartIndex = -1;
            EndIndex = -1;
        }

        public void Start(int index)
        {
            StartIndex = index;
            State = TranssactionState.Active;
        }
    }
}
