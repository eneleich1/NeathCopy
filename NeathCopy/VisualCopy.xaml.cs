using NeathCopy.Module2_Configuration;
using NeathCopy.Themes;
using NeathCopy.Tools;
using NeathCopy.UsedWindows;
using NeathCopy.ViewModels;
using NeathCopy.VisualCopySkins;
using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.DataTools;
using NeathCopyEngine.Exceptions;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeathCopy
{

    /// <summary>
    /// Interaction logic for VisualCopy.xaml
    /// </summary>
    public partial class VisualCopy : UserControl
    {

        #region Fields

        //Public Fields
        /// <summary>
        /// Get the exactly DateTime in wich operation started.
        /// </summary>
        public DateTime StartDateTime { get; private set; }
        public RequestInfo RequestInf { get;  set; }
        /// <summary>
        /// Get or set this instance QueveState.
        /// </summary>
        public QueueState InqueveState { get; set; }
        /// <summary>
        /// Get or set the InQueve index of this VisualCopy.
        /// This members is usefull only is this VisualCopy below to any queve.
        /// </summary>
        public int InqueveId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        public enum VisualCopyState { None,Paused,Discovering, Runing,Finished };

        VisualCopyState vcState;
        public VisualCopyState State
        {
            get => vcState;
            set
            {
                vcState = value;
                if (displayInfo != null)
                    displayInfo.State = vcState;
            }
        }
        public NeathCopyHandle NeathCopy { get; set; }

        //Useds Windows
        /// <summary>
        /// Window wich display all DataInfo to copy.
        /// </summary>
        public CopyListWindow listwnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        InformationWindow infoWnd;
        /// <summary>
        /// Windows to display message
        /// </summary>
        MessageWindow messageWnd;
        DiskIsFullWindows diskFullWmd;
        UserDropUIWindow userDropWindow = new UserDropUIWindow();


        private DateTime startTime;
        /// <summary>
        /// Used to prevent drop when drop is working
        /// </summary>
        bool handleDrop;
        /// <summary>
        /// Used to display all information
        /// </summary>
        //public DisplayInfoManager displayInfo;
        public VisualCopyVM displayInfo;

        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, Action> validActions = new Dictionary<string, Action>();
        /// <summary>
        /// 
        /// </summary>
        public IDriveInfo driveInfo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        long InitialFreeSpace;
        /// <summary>
        /// 
        /// </summary>
        FileExistOptionsWindow fileExistDlg = new FileExistOptionsWindow();
        FileNotFoundWindows fileNFDlg = new FileNotFoundWindows();
        ErrorWindow errorDlg = new ErrorWindow();

        //Pause Button Issues
        public enum PauseButtonAction { None, StartNow, Pause, Resume }

        PauseButtonAction pauseAction;
        PauseButtonAction PauseAction
        {
            get { return pauseAction; }
            set
            {
                pauseAction = value;
            }
        }
        public PauseButtonAction CurrentPauseButtonAction
        {
            get { return PauseAction; }
            set
            {
                PauseAction = value;
            }
        }

        #endregion

        public int IdNum;

        public static System.Threading.Mutex mutex = new System.Threading.Mutex();

        public VisualCopy()
        {
            InitializeComponent();

            IdNum = StartupClass.Id;
            StartupClass.Id++;

            MyInitialize();

            RequestInf = new RequestInfo();

            validActions.Add("copy", NeathCopy.Copy);
            validActions.Add("move", NeathCopy.Move);
            validActions.Add("fastmove", NeathCopy.FastMove);

            //Initialize VisualCopy View Model
            displayInfo = new VisualCopyVM();
            displayInfo.VisualCopy = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = displayInfo;
            displayInfo.updateTime_timer.Start();
        }

        #region OnFinish Behavior

        public Action OnFinish { get; protected set; }

        public void DoNothing()
        {
           
        }
        public void Shutdown()
        {
            OnFinishFuncitons.Shutdown();
        }
        public void Hibernate()
        {
            OnFinishFuncitons.Hibernate();
        }
        public void EjectDrive()
        {
            OnFinishFuncitons.Eject(driveInfo.Name, false, DriveType.Removable);
            System.Threading.Thread.Sleep(500);
        }

        #endregion

        #region Events

        public delegate void AceptRequestFinishedEventHandler(VisualCopy sender);
        public event AceptRequestFinishedEventHandler AceptArgumentsFinished;
        protected void RaiseAceptRequestFinished(VisualCopy sender)
        {
            if (AceptArgumentsFinished != null)
                AceptArgumentsFinished(sender);
        }

        public delegate void FinishEventHandler(VisualCopy sender);

        /// <summary>
        /// Event wich is raize when the operation is completed
        /// </summary>
        public event FinishEventHandler Finish;
        /// <summary>
        /// Defining the Method who raise the event
        /// </summary>
        protected void RaiseFinish(VisualCopy sender)
        {
            if (Finish != null)
                Finish(sender);
        }

        public delegate void CanceledEventHandler(VisualCopy sender);
        /// <summary>
        /// Event wich is launched after operation is cancelled.
        /// </summary>
        public event CanceledEventHandler AfterCancel;
        /// <summary>
        /// Defining the Method who raise the event
        /// </summary>
        protected void RaiseCanceled(VisualCopy sender)
        {
            if (AfterCancel != null)
                AfterCancel(sender);
        }

        public delegate void BreakInqueveEventHandler(VisualCopy sender);
        /// <summary>
        ///  Event wich is raize when this instance is inqueve waiting state
        ///  and start the reques operation.
        /// </summary>
        public event BreakInqueveEventHandler BreakInqueve;
        protected void RaiseBreakInquev(VisualCopy sender)
        {
            if (BreakInqueve != null)
                BreakInqueve(sender);
        }

        #endregion

        #region My Methods


        Object pauseBtLock = new Object();

        /// <summary>
        /// 
        /// </summary>
        private void PauseButtonBehavior()
        {
            lock (pauseBtLock)
            {
                switch (PauseAction)
                {
                    case PauseButtonAction.Resume://Button say resume => is paused and need to be resume
                        Resume();
                        break;
                    case PauseButtonAction.Pause://Button say paused => is runing and may be paused
                        Pause();
                        break;
                    case PauseButtonAction.StartNow://Button say !!Start Now => is inqueve and waiting
                        StartNow();
                        break;
                    case PauseButtonAction.None:// => is inqueve and waiting
                        StartNow();
                        break;
                }
            }
        }
        public void Resume()
        {
            if (NeathCopy.State == CopyHandleState.Finished || NeathCopy.State== CopyHandleState.NotStarted)
            {
                PauseAction = PauseButtonAction.Pause;

                NeathCopy.StartOperation();
            }
            else if (NeathCopy.State == CopyHandleState.Paused)
            {
                PauseAction = PauseButtonAction.Pause;

                NeathCopy.Resume();

                if (InqueveState == QueueState.StartedPaused)
                    InqueveState = QueueState.StartedRuning;
            }

            State = VisualCopyState.Runing;
        }

        bool skiping = false;
        public void Skip()
        {
            if (!skiping)
            {
                skiping = true;

                NeathCopy.Skip(NeathCopy.DiscoverdList.Index);

                skiping = false;
            }
        }
        public void Pause()
        {
            if (NeathCopy.State == CopyHandleState.Runing)
            {
                PauseAction = PauseButtonAction.Resume;

                NeathCopy.Pause();

                if (InqueveState == QueueState.StartedRuning)
                    InqueveState = QueueState.StartedPaused;

                State = VisualCopyState.Paused;
            }
        }
        public void Cancel(string cause)
        {
            State = VisualCopyState.Finished;
          
            NeathCopy.Cancel(cause);

            RaiseCanceled(this);
        }
        public void StartNow()
        {
            if (InqueveState == QueueState.Waiting)
            {
                PauseAction = PauseButtonAction.Pause;

                RaiseBreakInquev(this);

                PerformOperation();

            }
        }

        /// <summary>
        /// Init all fields, timers and all must be initialize.
        /// </summary>
        private void MyInitialize()
        {
            InqueveState = QueueState.None;

            NeathCopy = new NeathCopyHandle();
            NeathCopy.Finished += NeathCopy_Finished;
            //NeathCopy.Canceled += NeathCopy_Canceled;
            NeathCopy.DiskFull += NeathCopy_DiskFull;
            NeathCopy.FileNotFound += new NeathCopyHandle.FileNotFoundEventHandle(NeathCopy_FileNotFound);
            NeathCopy.TransferError += new NeathCopyHandle.TransferErrorEventHandle(NeathCopy_TransferError);
            NeathCopy.FileExist += new NeathCopyHandle.FileExistEventHandle(NeathCopy_FileExist);

            infoWnd = new InformationWindow("");
            messageWnd = new MessageWindow();
            diskFullWmd = new DiskIsFullWindows();

            listwnd = new CopyListWindow(this);
            listwnd.ListLoaded += new CopyListWindow.ListLoadedEventHandle(listwnd_ListLoaded);
            listwnd.SetCurrentList(NeathCopy.DiscoverdList);

        }

        /// <summary>
        /// Initialize Copy Engine Base on Configuration
        /// </summary>
        private void InitCopyEngine()
        {
            if (Configuration.Main.CurrentFileCopier != null)
                NeathCopy.FileCopier = Configuration.Main.CurrentFileCopier.Clone();
            ((BufferFileCopier)NeathCopy.FileCopier).BufferSize = Configuration.Main.BufferSize;
        }

        TransferErrorOption NeathCopy_TransferError(NeathCopyHandle copyHandle, NeathCopyEngine.DataTools.FileDataInfo currentFile, string error)
        {
            errorDlg.Dispatcher.Invoke(new Action(() =>
            {
                // Create a simple FlowDocument to serve as content.
                FlowDocument flowDoc = new FlowDocument(new Paragraph(new Run(string.Format("An error occurs when copy the {0} file: {1}", currentFile.Name, error))));

                // This call sets the contents of the RichTextBox to the specified FlowDocument.
                errorDlg.info_tb.Document = flowDoc;

                errorDlg.ShowDialog();
            }));

            return errorDlg.Option;
        }

        FileNotFoundOption NeathCopy_FileNotFound(NeathCopyHandle copyHandle, NeathCopyEngine.DataTools.FileDataInfo currentFile)
        {
            fileNFDlg.Dispatcher.Invoke(new Action(() =>
            {
                // Create a simple FlowDocument to serve as content.
                FlowDocument flowDoc = new FlowDocument(new Paragraph(new Run(string.Format("The system cold not find the specific file: {0}", currentFile.FullName))));

                // This call sets the contents of the RichTextBox to the specified FlowDocument.
                fileNFDlg.info_tb.Document = flowDoc;

                fileNFDlg.ShowDialog();
            }));

            return fileNFDlg.Option;

        }

        void NeathCopy_DiskFull(NeathCopyEngine.DataTools.FileDataInfo currentFile)
        {
            var device = string.Format("{0} ({1})", driveInfo.VolumeLabel, driveInfo.Name.Substring(0, driveInfo.Name.Length - 1));

            diskFullWmd.ShowMessage(string.Format("The disk {0} is full.", device));

            if (diskFullWmd.Option == DiskFullOption.DeleteFile)
            {
                var file = NeathCopy.DiscoverdList.Files[NeathCopy.DiscoverdList.Index];
                if (Alphaleonis.Win32.Filesystem.File.Exists(file.DestinyPath))
                    Alphaleonis.Win32.Filesystem.File.Delete(file.DestinyPath);
            }
        }
        Action<bool> NeathCopy_FileExist(NeathCopyHandle copyHandle, NeathCopyEngine.DataTools.FileDataInfo currentFile)
        {
            fileExistDlg.Dispatcher.Invoke(new Action(() =>
            {
                // Create a simple FlowDocument to serve as content.
                FlowDocument flowDoc = new FlowDocument(new Paragraph(new Run(string.Format("The File: {0} already exist", currentFile.FullName))));

                // This call sets the contents of the RichTextBox to the specified FlowDocument.
                fileExistDlg.info_tb.Document = flowDoc;

                fileExistDlg.ShowDialog();
            }));

            switch (fileExistDlg.Option)
            {
                case FileCopyOptions.AllwaysOverride:
                    return NeathCopy.OverwriteAll;
                case FileCopyOptions.OverwriteIfFileExist:
                    return NeathCopy.OverwriteCurrentFile;
                case FileCopyOptions.AllwaysSkip:
                    return NeathCopy.SkipAll;
                case FileCopyOptions.SkipIfFileExist:
                    return NeathCopy.SkipCurrentFile;
                case FileCopyOptions.OverrideDifferent:
                    return NeathCopy.OverwriteDifferent;
                case FileCopyOptions.AllwaysOverrideDifferent:
                    return NeathCopy.OverwriteAllDifferent;
                case FileCopyOptions.Cancel:
                    return NeathCopy.CancelCollision;
            }

            return NeathCopy.AllwaysAsk;
        }

        /// <summary>
        /// 
        /// </summary>
        private DiskSpaceOptions CheckForDiskSpace(long requireSpace)
        {
            var oldState = InqueveState;

            InqueveState = (InqueveState != QueueState.None) ? QueueState.DisplayingNotEnoughSpace : QueueState.None;

            //It came from multiple destiny, so can not check from disk space
            if (driveInfo == null)
                return DiskSpaceOptions.IGNORE;

            driveInfo = driveInfo.Clone();

            while (driveInfo.TotalFreeSpace < NeathCopy.DiscoverdList.SizeOfFilesToCopy.Bytes)// while (freeSpace < requireSpace)
            {
                infoWnd.ShowMessage(driveInfo, driveInfo.TotalFreeSpace, NeathCopy.DiscoverdList.SizeOfFilesToCopy.Bytes);

                if (infoWnd.UserSelectedAction != InformationWindow.UserAction.Try)
                    InqueveState = oldState;
                if (infoWnd.UserSelectedAction == InformationWindow.UserAction.Ignore)
                    return DiskSpaceOptions.IGNORE;
                else if (infoWnd.UserSelectedAction == InformationWindow.UserAction.Cancel)
                    return DiskSpaceOptions.CANCEL;
                else if (infoWnd.UserSelectedAction == InformationWindow.UserAction.Fit)
                    return DiskSpaceOptions.FIT_CONTENT;
            }

            infoWnd.Dispatcher.Invoke(new Action(infoWnd.Hide));

            InqueveState = oldState;

            return DiskSpaceOptions.OK;
        }

        public Alphaleonis.Win32.Network.DriveConnection nd;

        /// <summary>
        /// Analize and prepare for perform a operation.
        /// args = { operation, source, destiny, sourcesContainer }.
        /// </summary>
        /// <param name="args"></param>
        public void AceptRequest(RequestInfo info, bool sameThread, bool loadFromList)
        {
            long availableFreeSpace, totalFreeSpace,totalSize;
            string rootDirectory;

            var action = new Action(() =>
              {
                  try
                  {
                      #region Actions

                      if (!validActions.ContainsKey(info.Operation))
                          throw new InvalidCopyOperationException(info.Operation);

                      string opeBackup = displayInfo.Operation;

                      RequestInf = info;

                      //Get Driver Info of Destiny
                      if (RequestInf.Destiny != null)
                          driveInfo = DriveInfoFactory.CreateDriveInfo(RequestInf.Destiny);
                         
                      if (driveInfo != null)
                          InitialFreeSpace = driveInfo.TotalFreeSpace;

                      //Discover files to copy or move
                      NeathCopy.DiscoverdList.BegingTransaction();

                      displayInfo.Operation = "Discovering";
                      State = VisualCopyState.Discovering;
                      if (loadFromList)
                          FixFilesFromList();
                      else
                          NeathCopy.DiscoverdList.Discover(RequestInf, Dispatcher);
                      displayInfo.Operation = opeBackup;

                      //Check free disk space
                      DiskSpaceOptions opt;
                      if (loadFromList && loadedList.MultipleDestiny) opt = DiskSpaceOptions.IGNORE;
                      else opt = CheckForDiskSpace(NeathCopy.DiscoverdList.Size.Bytes);

                      //Inform AceptRequest finish
                      if (opt == DiskSpaceOptions.OK || opt == DiskSpaceOptions.IGNORE)
                      {
                          NeathCopy.Operation = validActions[info.Operation];
                          NeathCopy.DiscoverdList.CommitTransaction();

                          //Make sure when Acept Argument Finished, Raise Event to Execute PerformOperation
                          if (AceptArgumentsFinished == null)
                              AceptArgumentsFinished = visualCopy_AceptArgumentsFinished;

                          RaiseAceptRequestFinished(this);
                      }
                      else if (opt == DiskSpaceOptions.FIT_CONTENT)
                      {
                          driveInfo = driveInfo.Clone();

                          long freeSpace = driveInfo.TotalFreeSpace - NeathCopy.DiscoverdList.TrueFilesToCopySize.Bytes;

                          var requiredSpace = NeathCopy.DiscoverdList.TransactionSize.Bytes;
                          long free = 0;
                          while (freeSpace < requiredSpace)
                          {
                              free = NeathCopy.DiscoverdList.RemoveLast();
                              requiredSpace -= free;
                          }
                          NeathCopy.DiscoverdList.CommitTransaction();
                          NeathCopy.Operation = validActions[info.Operation];

                          //Make sure when Acept Argument Finished, Raise Event to Execute PerformOperation
                          if (AceptArgumentsFinished == null)
                              AceptArgumentsFinished = visualCopy_AceptArgumentsFinished;

                          RaiseAceptRequestFinished(this);
                      }
                      //Since this is the only and firsth request, the operation should be Cancel.
                      else
                      {
                          NeathCopy.DiscoverdList.DiscardTransaction();
                          Cancel("There is not enought free disk space and operation must be cancelled");
                      }

                      #endregion
                  }
                  catch (Exception ex)
                  {
                      MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "VisualCopy", "AceptRequest"));
                  }
              });

            if (sameThread)
                action.Invoke();
            else
                //Run in background to allow other actions.
                Task.Factory.StartNew(action);
        }


        /// <summary>
        /// Add Data to a CopyHandle existing FilesList
        /// </summary>
        /// <param name="info"></param>
        public void AddData(RequestInfo info, bool loadFromList)
        {
            try
            {

                var action = new Action(() =>
                {
                    #region MyRegion

                    string opBackup = displayInfo.Operation;

                    //Discover files to copy or move
                    NeathCopy.DiscoverdList.BegingTransaction();

                    displayInfo.Operation = "Discovering";
                    State = VisualCopyState.Discovering;

                    if (loadFromList)
                        FixFilesFromList();
                    else
                        NeathCopy.DiscoverdList.Discover(info,Dispatcher);
                    displayInfo.Operation = opBackup;

                    //Check free disk space
                    var opt = CheckForDiskSpace(NeathCopy.DiscoverdList.TransactionSize.Bytes);

                    //Inform AceptRequest finish
                    if (opt == DiskSpaceOptions.OK || opt == DiskSpaceOptions.IGNORE)
                    {
                        //Add files to NeathCopy DiscoverdList
                        NeathCopy.DiscoverdList.CommitTransaction();
                    }
                    else if (opt == DiskSpaceOptions.FIT_CONTENT)
                    {
                        driveInfo = driveInfo.Clone();

                        long freeSpace = driveInfo.TotalFreeSpace - NeathCopy.DiscoverdList.TrueFilesToCopySize.Bytes;

                        var requiredSpace = NeathCopy.DiscoverdList.TransactionSize.Bytes;
                        long free = 0;
                        while (freeSpace < requiredSpace)
                        {
                            free = NeathCopy.DiscoverdList.RemoveLast();
                            requiredSpace -= free;
                        }
                        NeathCopy.DiscoverdList.CommitTransaction();
                    }
                    else
                        NeathCopy.DiscoverdList.DiscardTransaction();

                    #endregion
                });

                if (State == VisualCopyState.Paused)
                {
                    action.Invoke();
                }
                else
                {
                    Pause();

                    action.Invoke();

                    Resume();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "VisualCopy", "AddData"));
            }
        }

        Object transferLogStreamLock = new Object();
        private void WriteTransferLog(List<string> logs)
        {
            mutex.WaitOne();

            var path = System.IO.Path.Combine(RegisterAccess.Acces.GetLogsDir(), "Transfer.log");
            var sw = new StreamWriter(new FileStream(path,FileMode.Append, FileAccess.Write));

            foreach (var log in logs)
                sw.WriteLine(log);

            sw.Close();
            sw.Dispose();

            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Perform the operation seted by the calling of AceptArguments method.
        /// </summary>
        public void PerformOperation()
        {
            try {

                PauseAction = PauseButtonAction.Pause;

                State = VisualCopyState.Runing;

                //Run in background so can display progress information.
                //Use other thread wich will be the worker thread
                startTime = System.DateTime.Now;

                InitCopyEngine();

                NeathCopy.StartOperation();
            }
            catch(Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "VisualCopy", "PerformOperation"));
            }
        }

        void NeathCopy_Canceled(NeathCopyHandle sender, string cancelCause)
        {
            try
            {
                //Transfer Log
                WriteTransferLog(
                new List<string>
                        {
                        "<Init>-------------------------------------",
                        string.Format("FileCopier: {0}",NeathCopy.FileCopier.Name),
                        string.Format("BufferSize: {0}",((BufferFileCopier)NeathCopy.FileCopier).BufferSize),
                        string.Format("Request Operation: {0}",NeathCopy.DiscoverdList.Operation),
                        string.Format("Destiny Folder: {0}", RequestInf==null?"Destiny Variable":RequestInf.Destiny),
                        string.Format("Files: {0}", NeathCopy.DiscoverdList.Count),
                        string.Format("Size: {0}", NeathCopy.DiscoverdList.Size),
                        string.Format("Start at: {0}", startTime),
                        string.Format("Operation cancelled: {0}", cancelCause),
                        string.Format("Elapsed Time: {0}", displayInfo.ElapsedTime),
                        "<End>-------------------------------------",
                        });
            }
            catch(Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "VisualCopy", "NeathCopy_Canceled"));
            }
        }
        void NeathCopy_Finished(List<Error> errors)
        {
            try
            {
                State = VisualCopyState.Finished;

                if (errors.Count > 0)
                {
                    PauseAction = PauseButtonAction.Resume;
                    NeathCopy.Errors.Clear();

                    if (Configuration.Main.AffterErrorAction.Method.Name == "Close_AffterErrorAction")
                        RaiseFinish(this);
                    else
                        return;
                }

                float cost = 0;
                IDriveInfo di = null;
                if (RequestInf.Destiny != null)
                {
                    di = DriveInfoFactory.CreateDriveInfo(Alphaleonis.Win32.Filesystem.Path.GetPathRoot(RequestInf.Destiny));
                    cost = EstimateCopyCost(NeathCopy.DiscoverdList.Count, NeathCopy.DiscoverdList.Size, di);
                }

                //Transfer Log
                double speed = 0;
                if (displayInfo.ElapsedTime.AllMiliseconds <1000)
                    speed = NeathCopy.DiscoverdList.Size.Bytes;
                else
                    speed = (NeathCopy.DiscoverdList.Size.Bytes / displayInfo.ElapsedTime.AllMiliseconds)/1000d;


                WriteTransferLog(
                new List<string>
                    {
                        "<Init>-------------------------------------",
                        string.Format("FileCopier: {0}",NeathCopy.FileCopier.Name),
                        string.Format("BufferSize: {0}",((BufferFileCopier)NeathCopy.FileCopier).BufferSize),
                        string.Format("Request Operation: {0}",RequestInf.Operation),
                        string.Format("Destiny Folder: {0}", RequestInf==null?"Destiny Variable":RequestInf.Destiny),

                        string.Format("Volumen Name: {0} [{1} Gb]",di==null?"Multiple Volumens":di.VolumeLabel,di==null?"unknown":new MySize(di.TotalSize).ToString()),

                        string.Format("Files: {0}", NeathCopy.DiscoverdList.Count),
                        string.Format("Size: {0}", NeathCopy.DiscoverdList.Size),
                        string.Format("Estimated Cost: {0} MN", cost),
                        string.Format("Start at: {0}", startTime),
                        string.Format("Completed at: {0}", System.DateTime.Now),
                        string.Format("Real Speed: {0}/s", new MySize((long)speed)),
                        string.Format("Elapsed Time: {0}", displayInfo.ElapsedTime),
                        "<End>-------------------------------------",
                    });

                OnFinish.Invoke();

                RaiseFinish(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message,"NeathCopy","VisualCopy", "NeathCopy_Finished"));
            }
        }

        private float EstimateCopyCost(int filesCount, MySize copiedSize, IDriveInfo di)
        {
            if (filesCount == 0) return 0;
            if (copiedSize.Bytes == 0) return 0;

            var totalSize = new MySize(di.TotalSize / 1024);
            var prices = new Dictionary<string, float>();
            prices.Add("4 Gb", 5);
            prices.Add("8 Gb", 10);
            prices.Add("16 Gb", 15);
            prices.Add("More than 16 Gb", 25);

            if (totalSize.Gb <= 4)
            {
                if (copiedSize == totalSize) return prices["4 Gb"];

                return Math.Min(prices["4 Gb"], filesCount);
            }
            else if (totalSize.Gb <= 8)
            {
                if (copiedSize == totalSize) return prices["8 Gb"];

                return Math.Min(prices["8 Gb"], filesCount);
            }
            else if (totalSize.Gb <= 16)
            {
                if (copiedSize == totalSize) return prices["16 Gb"];

                return Math.Min(prices["16 Gb"], filesCount);
            }
            else
            {
                if (copiedSize == totalSize) return prices["More than 16 Gb"];

                return Math.Min(prices["More than 16 Gb"], filesCount);
            }
        }


        #endregion

        #region Event Handlers

        private void preferences_m_Click(object sender, RoutedEventArgs e)
        {
            Theme.ConfigWindow.ShowDialog();
        }
        public void pause_button_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                PauseButtonBehavior();
            });
        }
        public void skip_button_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Skip();
            });
        }
        public void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Cancel("User cancel the operation");
            });
        }
        public void more_button_Click(object sender, RoutedEventArgs e)
        {
            if (NeathCopy.DiscoverdList.DiscoveringState == FilesList.DiscoverState.Discovering) return;

            Task.Factory.StartNew(() =>
            {
                listwnd.Dispatcher.Invoke(new Action(() =>
                {
                    listwnd.DisplayList();
                }));
            });
        }
        private void doNothing_rb_Checked(object sender, RoutedEventArgs e)
        {
            OnFinish = DoNothing;

            if (IsLoaded)
                ContextMenu.Visibility = Visibility.Hidden;
        }
        private void shutdown_rb_Checked(object sender, RoutedEventArgs e)
        {
            OnFinish = Shutdown;
            if (IsLoaded)
                ContextMenu.Visibility = Visibility.Hidden;
        }
        private void hibernate_rb_Checked(object sender, RoutedEventArgs e)
        {
            OnFinish = Hibernate;
            if (IsLoaded)
                ContextMenu.Visibility = Visibility.Hidden;
        }
        private void ejectDrive_rb_Checked(object sender, RoutedEventArgs e)
        {
            OnFinish = EjectDrive;

            if (IsLoaded)
                ContextMenu.Visibility = Visibility.Hidden;
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
        void listwnd_ListLoaded(FilesList list)
        {
            try
            {
                #region Code

                if (NeathCopy.Operation == null)
                {
                    //Get Driver Info of Destiny
                    driveInfo = DriveInfoFactory.CreateDriveInfo(Delimon.Win32.IO.Path.GetPathRoot(list.Destinys[0]));
                    InitialFreeSpace = driveInfo.TotalFreeSpace;

                    //Check free disk space
                    var opt = CheckForDiskSpace(NeathCopy.DiscoverdList.Size.Bytes);

                    RequestInf = new RequestInfo(list.Operation, "List: " + list.FileNameOnDisk, list.Destinys[0]);

                    //Inform AceptRequest finish
                    if (opt == DiskSpaceOptions.OK || opt == DiskSpaceOptions.IGNORE)
                    {
                        NeathCopy.Operation = validActions[list.Operation];
                        PerformOperation();
                    }
                    else if (opt == DiskSpaceOptions.FIT_CONTENT)
                    {
                        driveInfo = driveInfo.Clone();

                        long freeSpace = driveInfo.TotalFreeSpace - NeathCopy.DiscoverdList.TrueFilesToCopySize.Bytes;

                        var requiredSpace = NeathCopy.DiscoverdList.TransactionSize.Bytes;
                        long free = 0;
                        while (freeSpace < requiredSpace)
                        {
                            free = NeathCopy.DiscoverdList.RemoveLast();
                            requiredSpace -= free;
                        }
                        NeathCopy.DiscoverdList.CommitTransaction();

                        NeathCopy.Operation = validActions[list.Operation];
                        PerformOperation();
                    }
                    //Since this is the only and firsth request, the operation should be Cancel.
                    else
                    {
                        Cancel("There is not enought free disk space and operation must be cancelled");
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "VisualCopy", "listwnd_ListLoaded"));
            }
        }
        void NeathCopy_DiscoverFinish(CopyHandle sender)
        {
            RaiseAceptRequestFinished(this);
        }
        void visualCopy_AceptArgumentsFinished(VisualCopy sender)
        {
            sender.CurrentPauseButtonAction = VisualCopy.PauseButtonAction.Pause;
            sender.PerformOperation();
        }
        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                HandleDrop(new List<string>((string[])e.Data.GetData("FileDrop")));
            });
        }

        public bool LoadFromList { get; set; }
        public void HandleDrop(List<string> sourcesArg)
        {
            if (handleDrop) return;

            handleDrop = true;

            try
            {
                #region MyRegion

                if (RequestInf.Content== RquestContent.None)
                {
                    userDropWindow.Dispatcher.Invoke(() =>
                    {
                        userDropWindow.ShowDialog();

                        if (userDropWindow.DlgResult == true)
                        {
                            RequestInf = new RequestInfo(userDropWindow.Operation, sourcesArg, userDropWindow.Destiny) {  Content= RquestContent.All};

                            //Acept the user request openation in a new thread
                            AceptRequest(RequestInf,false,false);
                        }
                    });
                   
                }
                else
                {
                    var drive = new DriveInfo(Alphaleonis.Win32.Filesystem.Path.GetPathRoot(RequestInf.Destiny));
                    if (!DriveInfo.GetDrives().Select(d=>d.Name).Contains(drive.Name))
                    {
                        MessageBox.Show(Error.GetErrorLog("The Destiny Drive do not exist", "NeathCopy", "VisualCopy", "HandleDrop"));
                        return;
                    }

                    var info = new RequestInfo
                    {
                        Operation = RequestInf.Operation,
                        Destiny = RequestInf.Destiny,
                        Sources = sourcesArg
                    };

                    //Add data to list in a new thread
                    AddData(info,false);
                }

                #endregion
            }

            catch (Exception ex)
            {
                ErrorCheck(ex);
            }
            finally
            {
                handleDrop = false;
            }
            
        }

        SerializableFilesList loadedList;
        public void HandleLoadList(SerializableFilesList list)
        {
            try
            {
                #region MyRegion

                loadedList = list;

                if (RequestInf == null || RequestInf.Content==RquestContent.None)
                {
                    userDropWindow.Dispatcher.Invoke(() =>
                    {
                        if (loadedList.MultipleDestiny)
                        {
                            RequestInf = new RequestInfo() { Operation = loadedList.Operation};

                            //Acept the user request operation in a new thread
                            AceptRequest(RequestInf, false, true);
                        }
                        else
                        {
                            userDropWindow.ShowDialog();

                            if (userDropWindow.DlgResult == true)
                            {
                                RequestInf = new RequestInfo() { Operation = userDropWindow.Operation, Destiny = userDropWindow.Destiny };

                                //Acept the user request operation in a new thread
                                AceptRequest(RequestInf, false, true);
                            }
                        }
                    });

                }
                else
                {
                    var info = new RequestInfo
                    {
                        Operation = RequestInf.Operation,
                        Destiny = RequestInf.Destiny,
                        //Sources = sourcesArg
                    };

                    //Add data to list in a new thread
                    AddData(info,true);
                }

                #endregion
            }

            catch (Exception ex)
            {
                ErrorCheck(ex);
            }

        }
        private void FixFilesFromList()
        {
            NeathCopy.DiscoverdList.DiscoveringState = FilesList.DiscoverState.Discovering;
            FileDataInfo file = null;
            Alphaleonis.Win32.Filesystem.FileInfo finfo = null;

            foreach (FileOnList f in loadedList.Files)
            {
                finfo = new Alphaleonis.Win32.Filesystem.FileInfo(f.From);
                file = new NeathCopyEngine.DataTools.FileDataInfo
                {
                    SourceDirectoryLength=2,
                    FullName = f.From,
                    Name = finfo.Name,
                    //Remove the Dirver Letter: F://Videos/Series/12 monkeys 4x07.mp4 => [Destiny]//Videos/Series/12 monkeys 4x07.mp4
                    DestinyPath = loadedList.MultipleDestiny?f.To:Alphaleonis.Win32.Filesystem.Path.Combine(RequestInf.Destiny, f.To.Remove(0,3)),
                    CreationTime = finfo.CreationTime,
                    LastAccessTime = finfo.LastAccessTime,
                    LastWriteTime = finfo.LastWriteTime,
                    Size = finfo.Length,
                    FileAttributes = (Delimon.Win32.IO.FileAttributes)finfo.Attributes
                };

                file.DestinyDirectoryPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(file.DestinyPath);

                NeathCopy.DiscoverdList.Files.Add(file);
                NeathCopy.DiscoverdList.Count++;
                NeathCopy.DiscoverdList.Size += file.Size;
                NeathCopy.DiscoverdList.SizeOfFilesToCopy += file.Size;
            }

            NeathCopy.DiscoverdList.DiscoveringState = FilesList.DiscoverState.Normal;

            RequestInf.Content = RquestContent.FromList;
        }

        private void ErrorCheck(Exception ex)
        {
            var message = string.Format("Module: {0}, Class: {1}, Method: {2}, Error: {3}", "NeathCopy", "VisualCopy", "UserControl_Drop_1:ErrorCheck", ex.Message);
            //System.Windows.MessageBox.Show("An error has ocurrend, see ErrorsLog.txt file for more information");
            //MessageBox.Show(message);

            using (var w = new StreamWriter(new FileStream("Errors Log.txt", FileMode.Append, FileAccess.Write)))
            {
                w.WriteLine("-------------------------------");
                w.WriteLine(System.DateTime.Now);
                w.WriteLine(message);
            }

            RegisterAccess.Acces.UnregisterCopyHandler();
        }
        private void UserControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu.Visibility = Visibility.Visible;
        }

        #endregion
       
    }

    public enum DiskSpaceOptions
    {
        OK, CANCEL, IGNORE, FIT_CONTENT
    }
}
