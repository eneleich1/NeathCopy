using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Text.RegularExpressions;
using NeathCopyEngine.DataTools;
using NeathCopyEngine.Helpers;
using System.Threading;
using System.Threading.Tasks;
using NeathCopyEngine.Exceptions;
using System.ComponentModel;

namespace NeathCopyEngine.CopyHandlers
{
    #region Enums
    public enum FileNotFoundOption
    {
        Cancel, Skip
    }
    public enum TransferErrorOption
    {
        SkipCurrentFile, Cancel, SkipAll,Try
    }
    public enum OperationStatus
    {
        NotStarted, Started, Finished
    }
    public enum AffterErrorAction
    {
        DoNothing, Cancel,Try
    }

    #endregion
    public class NeathCopyHandle : CopyHandle
    {
        #region Visual Information

        /// <summary>
        /// Amount of bytes copieds for all files wich have been copieds.
        /// </summary>
        public long TotalBytesTransferred
        {
            get
            {
                return (FileCopier == null) ? 0 : FileCopier.TotalBytesTransferred;
            }
        }
        /// <summary>
        /// Amount of bytes copieds of current file.
        /// </summary>
        public long CurrentFileBytesTransferred
        {
            get
            {
                return (FileCopier == null) ? 0 : FileCopier.FileBytesTransferred;
            }
        }
        /// <summary>
        /// Amount of copieds files;
        /// </summary>
        public long CopiedsFiles;

        /// <summary>
        /// Current file wich is been copied.
        /// </summary>
        public FileDataInfo CurrentFile;

        /// <summary>
        /// Total size of the file wich is been copied
        /// </summary>
        public long FileSize;
      

        #endregion

        #region Fields

        /// <summary>
        /// Retrieve the current operation status based on the processing file.
        /// </summary>
        public OperationStatus operationStaus { get; set; }
        public TransferErrorOption transferErrorOption { get; set; }

        ManualResetEventSlim pauseGate = new ManualResetEventSlim(true);
        CancellationTokenSource operationCts;
        Task operationTask;
        readonly object operationLock = new object();
        public Action Operation { get; set; }
        public Action<bool> FileCollisionAction { get; set; }
        public Action ErrorFoundAction { get; set; }

        //Make sure only one thread use DiscoverList
        private static Mutex mut = new Mutex();
        
        //Make sure only one thread curent used file
        //private static Mutex mut1 = new Mutex();

        /// <summary>
        /// State seted to a file already processed
        /// </summary>
        CopyState affeterOperationState;
        /// <summary>
        /// Action to performe once file have been copied.
        /// </summary>
        protected Action<FileDataInfo> AffterFileCopyAction;

        public static int MAX_FILE_PATH = 260;
        public static int MAX_DIR_PATH = 248;
        internal const int MAX_PATH = 260;
        string logsPath;

        AffterErrorAction opt;
        public MultiDestinationCopyRequest MultiDestinationRequest { get; private set; }
        private volatile bool pauseAfterSkipRequested;
        private MultiDestinationCollisionMode multiDestinationCollisionMode = MultiDestinationCollisionMode.Ask;
        public bool CrashRecoveryEnabled { get; set; }
        public string CrashRecoveryFolder { get; set; }
        public string CrashRecoveryCheckpointPath { get; private set; }
        SerializableFilesList crashRecoveryList;
        readonly object crashLock = new object();
        bool crashRecoveryInitialized;
        string crashRecoveryOperation;
        bool crashRecoveryMultipleDestiny;
        bool crashRecoverySavedAtCancel;

        #endregion

        private enum MultiDestinationCollisionMode
        {
            Ask,
            OverwriteAll,
            SkipAll
        }

        public NeathCopyHandle() : base()
        {
            FileCopier = new FasterBufferFileCopier(1024 * 1024);

            operationStaus = OperationStatus.NotStarted;
            transferErrorOption = TransferErrorOption.SkipCurrentFile;
            FileCollisionAction = AllwaysAsk;
            logsPath = Path.Combine(RegisterAccess.Acces.GetLogsDir(), "Errors Log.txt");
            CrashRecoveryEnabled = false;
            CrashRecoveryFolder = GetDefaultFilesListFolder();
            CrashRecoveryCheckpointPath = null;
            crashRecoveryList = null;
            crashRecoveryInitialized = false;
            crashRecoverySavedAtCancel = false;

        }

        public void InitializeCrashRecovery(string destinyLabelOrPath, string operation, bool multipleDestiny)
        {
            if (!CrashRecoveryEnabled)
                return;

            lock (crashLock)
            {
                var folder = string.IsNullOrWhiteSpace(CrashRecoveryFolder)
                    ? GetDefaultFilesListFolder()
                    : CrashRecoveryFolder;
                folder = LongPathHelper.Normalize(folder);
                Directory.CreateDirectory(folder);

                crashRecoveryOperation = string.IsNullOrWhiteSpace(operation) ? "copy" : operation;
                crashRecoveryMultipleDestiny = multipleDestiny;

                var label = SanitizeFileName(string.IsNullOrWhiteSpace(destinyLabelOrPath) ? "UnknownDestiny" : destinyLabelOrPath);
                if (label.Length > 80)
                    label = label.Substring(0, 80);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = string.Format("{0}_{1}.ncopylist", label, timestamp);
                var fullPath = LongPathHelper.Normalize(Path.Combine(folder, fileName));
                if (File.Exists(fullPath))
                {
                    var suffix = 1;
                    while (true)
                    {
                        var candidate = LongPathHelper.Normalize(
                            Path.Combine(folder, string.Format("{0}_{1}_{2}.ncopylist", label, timestamp, suffix)));
                        if (!File.Exists(candidate))
                        {
                            fullPath = candidate;
                            break;
                        }
                        suffix++;
                    }
                }
                CrashRecoveryCheckpointPath = fullPath;

                BuildCrashRecoveryListSnapshot();
                SaveCrashRecoveryCheckpointUnsafe();
                crashRecoveryInitialized = true;
                crashRecoverySavedAtCancel = false;
            }
        }

        public void ConfigureMultiDestinationCopy(MultiDestinationCopyRequest request, int bufferSize)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Items == null || request.Items.Count == 0)
                throw new ArgumentException("The multi-destination request has no files.", nameof(request));

            if (request.DestinationRoots == null || request.DestinationRoots.Count == 0)
                throw new ArgumentException("The multi-destination request has no destinations.", nameof(request));

            MultiDestinationRequest = request;
            pauseAfterSkipRequested = false;
            multiDestinationCollisionMode = MultiDestinationCollisionMode.Ask;
            var copier = FileCopier as MultiDestinationFileCopier;
            if (copier == null)
                copier = new MultiDestinationFileCopier(bufferSize);
            else
                copier.BufferSize = bufferSize > 0 ? bufferSize : copier.BufferSize;

            FileCopier = copier;
            Operation = CopyMultiDestination;
        }

        #region Events

        public delegate Action<bool> FileExistEventHandle(NeathCopyHandle copyHandle, FileDataInfo currentFile);
        /// <summary>
        /// Occurs when a operation is performed and one or more files already exist.
        /// </summary>
        public event FileExistEventHandle FileExist;

        public delegate void DiskFullEventHandle(FileDataInfo currentFile);
        /// <summary>
        /// Occurs when a file is copiying and the destiny disk is full.
        /// </summary>
        public event DiskFullEventHandle DiskFull;
        protected void RaiseDiskFull(FileDataInfo currentFile)
        {
            if (DiskFull != null)
                DiskFull(currentFile);
        }


        public delegate FileNotFoundOption FileNotFoundEventHandle(NeathCopyHandle copyHandle, FileDataInfo currentFile);
        /// <summary>
        /// Occurs when the file to be copy.
        /// </summary>
        public event FileNotFoundEventHandle FileNotFound;
        protected FileNotFoundOption RaiseFileNotFound(NeathCopyHandle copyHandle, FileDataInfo CurrentFile)
        {
            if (FileNotFound != null)
                return FileNotFound(copyHandle, CurrentFile);

            return FileNotFoundOption.Cancel;
        }

        public delegate TransferErrorOption TransferErrorEventHandle(NeathCopyHandle copyHandle, FileDataInfo currentFile, string error);
        /// <summary>
        /// Occurs when an error occurs while copiying a file.
        /// </summary>
        public event TransferErrorEventHandle TransferError;
        protected TransferErrorOption RaiseTransferError(NeathCopyHandle copyHandle, FileDataInfo CurrentFile, string error)
        {
            if (TransferError != null)
                return TransferError(copyHandle, CurrentFile, error);

            return TransferErrorOption.SkipCurrentFile;
        }

        public delegate void CanceledEventHandle(NeathCopyHandle sender, string cancelCause);
        /// <summary>
        /// Occurs affter this worked has been canceled.
        /// </summary>
        public event CanceledEventHandle Canceled;
        protected void RaiseCanceled(string cancelCause)
        {
            if (Canceled != null)
                Canceled(this, cancelCause);
        }

        public delegate void FinishdEventHandle(List<Error> errors);
        /// <summary>
        /// Occurs when operation is finished.
        /// </summary>
        public event FinishdEventHandle Finished;
        protected void RaiseFinished(List<Error> errors)
        {
            if (Finished != null)
                Finished(errors);
        }

        #endregion

        #region Methods

        private void ConfigureExecutionContext()
        {
            if (FileCopier == null) return;

            var token = operationCts == null ? CancellationToken.None : operationCts.Token;
            FileCopier.ConfigureExecution(pauseGate, token);
        }
        private void WaitForResumeOrCancel()
        {
            if (operationCts == null) return;

            pauseGate.Wait(operationCts.Token);
            operationCts.Token.ThrowIfCancellationRequested();
        }
        private void StartOperationInternal()
        {
            if (Operation == null) return;

            lock (operationLock)
            {
                operationCts?.Dispose();
                operationCts = new CancellationTokenSource();
                crashRecoverySavedAtCancel = false;
                pauseGate.Set();
                ConfigureExecutionContext();

                operationTask = Task.Run(() =>
                {
                    try
                    {
                        Operation.Invoke();
                        if (State != CopyHandleState.Canceled)
                        {
                            RaiseFinished(Errors);
                            Errors.Clear();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancellation is handled by the caller.
                    }
                });
            }
        }
        private void CancelOperationForRestart()
        {
            if (operationCts == null) return;

            operationCts.Cancel();
            pauseGate.Set();
        }

        #region Affter File Copy Action
        void DoNothing(FileDataInfo currentFile)
        {
            FinalizeTempDestinationForFile(currentFile);
            try
            {
                MetadataRestorer.RestoreFileMetadata(currentFile.FullName, currentFile.DestinyPath);
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Delete the file thats was copied.
        /// </summary>
        /// <param name="currentFile"></param>
        void DeleteFile(FileDataInfo currentFile)
        {
            FinalizeTempDestinationForFile(currentFile);
            try
            {
                MetadataRestorer.RestoreFileMetadata(currentFile.FullName, currentFile.DestinyPath);
            }
            catch (Exception)
            {
            }

            #region Delete the file after copy finished

            try
            {
                //Delete the copied file
                File.Delete(LongPathHelper.Normalize(CurrentFile.FullName));
            }
            catch (IOException ex)
            {
                throw new IOException(string.Format("The File: {0} cold not been moved. {1}", CurrentFile.FullName, ex.Message));
            }

            #endregion
        }

        #endregion 

        #region File Collisions Actions
        public void AllwaysAsk(bool fastMove)
        {
            //Copy the current file.
            if (File.Exists(LongPathHelper.Normalize(CurrentFile.DestinyPath)))
            {
                if (FileCollisionAction == CancelCollision) return;

                FileCollisionAction = FileExist(this, CurrentFile);

                FileCollisionAction.Invoke(fastMove);
            }
            else
            {
                CurrentFile.CopyState = CopyState.Processing;
                DiscoverdList.SizeOfFilesToCopy -= CurrentFile.Size;

                if (fastMove)
                    File.Move(LongPathHelper.Normalize(CurrentFile.FullName), LongPathHelper.Normalize(CurrentFile.DestinyPath));
                else
                    FileCopier.CopyFile(CurrentFile);
              
                AffterFileCopyAction.Invoke(CurrentFile);
            }

        }
        public void OverwriteAll(bool fastMove)
        {
            //Copy the current file.
            CurrentFile.CopyState = CopyState.Processing;
            DiscoverdList.SizeOfFilesToCopy -= CurrentFile.Size;

            if (fastMove)
            {
                if (File.Exists(LongPathHelper.Normalize(CurrentFile.DestinyPath)))
                    File.Delete(LongPathHelper.Normalize(CurrentFile.DestinyPath));
                File.Move(LongPathHelper.Normalize(CurrentFile.FullName), LongPathHelper.Normalize(CurrentFile.DestinyPath));
            }
            else
            {
                FileCopier.CopyFile(CurrentFile);
                AffterFileCopyAction.Invoke(CurrentFile);
            }
        }
        public void SkipAll(bool fastMove)
        {
            if (!File.Exists(LongPathHelper.Normalize(CurrentFile.DestinyPath)))
            {
                CurrentFile.CopyState = CopyState.Processing;
                DiscoverdList.SizeOfFilesToCopy -= CurrentFile.Size;

                if (fastMove)
                    File.Move(LongPathHelper.Normalize(CurrentFile.FullName), LongPathHelper.Normalize(CurrentFile.DestinyPath));
                else
                    FileCopier.CopyFile(CurrentFile);

                AffterFileCopyAction.Invoke(CurrentFile);
            }
            else
                FileCopier.TotalBytesTransferred += CurrentFile.Size;
        }
        public void SkipCurrentFile(bool fastMove)
        {
            if (!File.Exists(LongPathHelper.Normalize(CurrentFile.DestinyPath)))
            {
                CurrentFile.CopyState = CopyState.Processing;
                DiscoverdList.SizeOfFilesToCopy -= CurrentFile.Size;

                if (fastMove)
                    File.Move(LongPathHelper.Normalize(CurrentFile.FullName), LongPathHelper.Normalize(CurrentFile.DestinyPath));
                else
                    FileCopier.CopyFile(CurrentFile);


                AffterFileCopyAction.Invoke(CurrentFile);
            }
            else
            {
                FileCopier.TotalBytesTransferred += CurrentFile.Size;

                FileCollisionAction = AllwaysAsk;
            }
        }
        public void OverwriteCurrentFile(bool fastMove)
        {
            OverwriteAll(fastMove);

            FileCollisionAction = AllwaysAsk;
        }
        public void OverwriteDifferent(bool fastMove)
        {
            OverwriteAllDifferent(fastMove);

            FileCollisionAction = AllwaysAsk;
        }
        public void OverwriteAllDifferent(bool fastMove)
        {
            if (File.Exists(LongPathHelper.Normalize(CurrentFile.DestinyPath)) && !FileDataInfo.Md5Check(CurrentFile.FullName, CurrentFile.DestinyPath))
                OverwriteCurrentFile(fastMove);
            else SkipCurrentFile(fastMove);

            FileCollisionAction = OverwriteAllDifferent;
        }
        public void CancelCollision(bool fastMove)
        {
            Cancel("User Cancel");
        }

        #endregion

        public enum CopyRoutineResult { Error, Canceled, Ok }
        protected CopyRoutineResult CopyRoutine(FilesList files, bool fastMove)
        {
            //System.IO.DirectoryInfo sysDirInfo = null;
            //FileAttributes sysAtt;
            //Alphaleonis.Win32.Filesystem.DirectoryInfo di = null;
            //FileAttributes att;

            for (; DiscoverdList.Index < DiscoverdList.Count; DiscoverdList.Index++)
            {
                try
                {
                    WaitForResumeOrCancel();
                    if (State == CopyHandleState.Canceled) return CopyRoutineResult.Canceled;

                    //Get the file to copy.
                    if (DiscoverdList.Count == 0 || DiscoverdList.Index >= DiscoverdList.Count) 
                        return CopyRoutineResult.Error;
                    CurrentFile = DiscoverdList.Files[DiscoverdList.Index];

                    if (CurrentFile.CopyState == CopyState.Copied || CurrentFile.CopyState == CopyState.Moved)
                    {
                        FileCopier.TotalBytesTransferred += CurrentFile.Size;
                        CopiedsFiles++;
                        UpdateCrashRecoveryForCurrentFile(CurrentFile.CopyState, false);
                        continue;
                    }

                    CurrentFile.CopyState = CopyState.Processing;
                    PrepareTempDestinationIfNeeded(fastMove);
                    UpdateCrashRecoveryForCurrentFile(CopyState.Processing, true);

                    //Create the Directories.
                    
                    try
                    {
                        //di = new Alphaleonis.Win32.Filesystem.DirectoryInfo(Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(CurrentFile.FullName));
                        //att = di.Attributes;
                        Directory.CreateDirectory(LongPathHelper.Normalize(CurrentFile.DestinyDirectoryPath));
                        //di=new Alphaleonis.Win32.Filesystem.DirectoryInfo(CurrentFile.DestinyDirectoryPath);
                        //di.Attributes = att;
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {

                        //This is a copy actions wich deals with file collisions.
                        FileCollisionAction.Invoke(fastMove);

                        //If the File was Processed succefully=============================
                        //Set the currentFile copy state as Copied.
                        if (CurrentFile.CopyState == CopyState.Skiped)
                        {
                            // Skip counts as processed but must not be marked as copied/moved.
                            CopiedsFiles++;
                            CleanupTempDestinationIfNeeded();
                        }
                        else
                        {
                            CurrentFile.CopyState = affeterOperationState;
                            FinalizeTempDestinationIfNeeded();

                            //Status.
                            CopiedsFiles++;
                        }

                        UpdateCrashRecoveryForCurrentFile(CurrentFile.CopyState, false);

                    }

                }
                catch (Exception ex)
                {
                    if (CurrentFile != null && CurrentFile.CopyState == CopyState.Skiped)
                    {
                        UpdateCrashRecoveryForCurrentFile(CurrentFile.CopyState, false);
                        continue;
                    }

                    opt = ErrorsCheck(ex);

                    if (opt == AffterErrorAction.Cancel)
                        return CopyRoutineResult.Canceled;
                    else if (opt == AffterErrorAction.Try)
                    {
                        //Try to process the same file
                        DiscoverdList.Index--;
                        FileCopier.TotalBytesTransferred -= FileCopier.FileBytesTransferred;
                        FileCopier.FileBytesTransferred = 0;
                    }
                    else if (!(ex is ThreadAbortException))
                    {
                        CurrentFile.CopyState = CopyState.Error;
                        CleanupTempDestinationIfNeeded();
                        UpdateCrashRecoveryForCurrentFile(CurrentFile.CopyState, false);
                    }
                }

            }

            return CopyRoutineResult.Ok;
        }

        private AffterErrorAction ErrorsCheck(Exception ex)
        {
            #region Errors Check

            if (ex is NotEnoughSpaceException)
            {
                Errors.Add(new DiskFullError { Message = string.Format("The file: {0} could not be copied, the disk is full or there are not enough space", CurrentFile.FullName) });
                RaiseDiskFull(CurrentFile);
                return AffterErrorAction.Cancel;
            }
            else if (ex is OperationCanceledException)
            {
                return AffterErrorAction.Cancel;
            }
            else if (ex is ThreadAbortException)
            {
                return AffterErrorAction.DoNothing;
            }
            else if (ex is FileNotFoundException)
            {
                return Error_DisconectDeviceAction(ex);
                //if (ex is FileNotFoundException && ((FileNotFoundException)ex).FileName != CurrentFile.FullName)
                //    return Error_DisconectDeviceAction(ex);
                //else
                //{
                //    switch (RaiseFileNotFound(this, CurrentFile))
                //    {
                //        case FileNotFoundOption.Cancel:
                //            return AffterErrorAction.DoNothing;
                //        case FileNotFoundOption.Skip:
                //            break;
                //    }
                //}
            }
            else if (ex is DirectoryNotFoundException || ex is ObjectDisposedException)
            {
                //Exceptions caused by cancelling operation.
            }
            else if (transferErrorOption != TransferErrorOption.SkipAll)
            {
                return Error_DisconectDeviceAction(ex);
            }

            #endregion

            return AffterErrorAction.DoNothing;
        }
        private AffterErrorAction Error_DisconectDeviceAction(Exception ex)
        {
            Errors.Add(new CopyError { Message = ex.Message });

            var message = Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "NeathCopyHandle", "Error_DisconectDeviceAction");

            using (var w = new StreamWriter(new FileStream(logsPath, FileMode.Append, FileAccess.Write)))
            {
                w.WriteLine("-------------------------------");
                w.WriteLine(System.DateTime.Now);
                w.WriteLine(message);
            }

            transferErrorOption = RaiseTransferError(this, CurrentFile, ex.Message);

            switch (transferErrorOption)
            {
                case TransferErrorOption.SkipCurrentFile:
                    // Since when copy this file occurs an error them
                    // follow to the next file by doing nothing
                    break;
                case TransferErrorOption.Try:
                    return AffterErrorAction.Try;
                case TransferErrorOption.Cancel:
                    return AffterErrorAction.Cancel;
            }

            return AffterErrorAction.DoNothing;
        }
        private void RestoreAllDirectoriesMetadata()
        {
            if (DiscoverdList == null || DiscoverdList.Directories == null) return;

            foreach (var dir in DiscoverdList.Directories)
            {
                if (dir == null) continue;

                var src = dir.FullName;
                var dst = dir.DestinyPath;

                try
                {
                    MetadataRestorer.RestoreDirectoryMetadata(src, dst);
                    LogDirectoryAttributes(src, dst);
                }
                catch (Exception)
                {
                }
            }
        }

        protected void CreateEmptysDirectories(FilesList list)
        {
            //Creating empty directories
            foreach (var d in list.EmptyDirectories)
            {
                Directory.CreateDirectory(LongPathHelper.Normalize(d.DestinyPath));
            }
        }

        private void LogDirectoryAttributes(string sourceDirPath, string destDirPath)
        {
            try
            {
                var src = LongPathHelper.Normalize(sourceDirPath);
                var dst = LongPathHelper.Normalize(destDirPath);

                if (!System.IO.Directory.Exists(src) || !System.IO.Directory.Exists(dst))
                    return;

                var a1 = File.GetAttributes(src);
                var a2 = File.GetAttributes(dst);

                if (a1 != a2)
                {
                    LogDebug(string.Format("ATTR MISMATCH: src={0}, dst={1}, srcAttr={2}, dstAttr={3}", src, dst, a1, a2));
                }

                if (string.Equals(Path.GetFileName(src), ".vs", StringComparison.OrdinalIgnoreCase))
                {
                    LogDebug(string.Format("VS ATTR: src={0}, dst={1}, srcAttr={2}, dstAttr={3}", src, dst, a1, a2));
                }
            }
            catch (Exception)
            {
            }
        }

        private void LogDebug(string message)
        {
            try
            {
                using (var w = new StreamWriter(new FileStream(logsPath, FileMode.Append, FileAccess.Write)))
                {
                    w.WriteLine("-------------------------------");
                    w.WriteLine(System.DateTime.Now);
                    w.WriteLine(message);
                }
            }
            catch (Exception)
            {
            }
        }

        public override void Copy()
        {
            MultiDestinationRequest = null;
            State = CopyHandleState.Runing;
            ConfigureExecutionContext();
            EnsureCrashRecoveryInitializedForCurrentOperation("copy", false);

            AffterFileCopyAction = DoNothing;
            affeterOperationState = CopyState.Copied;

            CopyRoutineResult copyResult = CopyRoutine(DiscoverdList,false);
            if (copyResult == CopyRoutineResult.Canceled)
            {
                State = CopyHandleState.Canceled;
                return;
            }
            if (copyResult == CopyRoutineResult.Error)
            {
                State = CopyHandleState.Finished;
                return;
            }

            CreateEmptysDirectories(DiscoverdList);
            RestoreAllDirectoriesMetadata();
            SaveCrashRecoveryCheckpointBestEffort();

            State = CopyHandleState.Finished;

        }
        public override void Move()
        {
            MultiDestinationRequest = null;
            State = CopyHandleState.Runing;
            ConfigureExecutionContext();
            EnsureCrashRecoveryInitializedForCurrentOperation("move", false);

            AffterFileCopyAction = DeleteFile;
            affeterOperationState = CopyState.Moved;

            CopyRoutineResult copyResult=CopyRoutine(DiscoverdList,false);
            if (copyResult == CopyRoutineResult.Canceled)
            {
                State = CopyHandleState.Canceled;
                return;
            }
            if (copyResult == CopyRoutineResult.Error)
            {
                State = CopyHandleState.Finished;
                return;
            }

            //Creating empty directories
            CreateEmptysDirectories(DiscoverdList);
            RestoreAllDirectoriesMetadata();

            #region Delete all Directory in data info list

            if (copyResult== CopyRoutineResult.Ok)//State != CopyHandleState.Canceled)
            {
                //Delete all Directory in data info list
                foreach (var dir in DiscoverdList.SourcesDirectories)
                {
                    try
                    {
                        //LongPath.Directory.Delete(dir);
                        Directory.Delete(LongPathHelper.Normalize(dir), true);
                    }
                    catch (IOException ex)
                    {
                        throw new IOException(string.Format("The Directory: {0} cold not been moved. {1}", dir, ex.Message));
                    }

                }
            }

            #endregion
            SaveCrashRecoveryCheckpointBestEffort();

            State = CopyHandleState.Finished;

        }
        public override void FastMove()
        {
            MultiDestinationRequest = null;
            State = CopyHandleState.Runing;
            ConfigureExecutionContext();
            EnsureCrashRecoveryInitializedForCurrentOperation("fastmove", false);

            AffterFileCopyAction = DoNothing;
            affeterOperationState = CopyState.Moved;

            CopyRoutineResult copyResult = CopyRoutine(DiscoverdList,true);
            if (copyResult == CopyRoutineResult.Canceled)
            {
                State = CopyHandleState.Canceled;
                return;
            }
            if (copyResult == CopyRoutineResult.Error)
            {
                State = CopyHandleState.Finished;
                return;
            }

            //Creating empty directories
            CreateEmptysDirectories(DiscoverdList);
            RestoreAllDirectoriesMetadata();
            SaveCrashRecoveryCheckpointBestEffort();

            State = CopyHandleState.Finished;
        }

        private void CopyMultiDestination()
        {
            State = CopyHandleState.Runing;
            ConfigureExecutionContext();
            EnsureCrashRecoveryInitializedForCurrentOperation("copy", true);

            var request = MultiDestinationRequest;
            if (request == null || request.Items == null || request.Items.Count == 0)
            {
                State = CopyHandleState.Finished;
                return;
            }

            var copier = FileCopier as MultiDestinationFileCopier;
            if (copier == null)
            {
                copier = new MultiDestinationFileCopier(1024 * 1024);
                FileCopier = copier;
                ConfigureExecutionContext();
            }

            for (; DiscoverdList.Index < request.Items.Count; DiscoverdList.Index++)
            {
                try
                {
                    WaitForResumeOrCancel();
                    if (State == CopyHandleState.Canceled)
                        return;

                    if (DiscoverdList.Index >= DiscoverdList.Count)
                        break;

                    CurrentFile = DiscoverdList.Files[DiscoverdList.Index];
                    CurrentFile.CopyState = CopyState.Processing;
                    UpdateCrashRecoveryForCurrentFile(CopyState.Processing, true);

                    var item = request.Items[DiscoverdList.Index];
                    var collisionDecision = ResolveMultiDestinationCollision(item, request);
                    if (collisionDecision == MultiDestinationCollisionDecision.Cancel)
                    {
                        Cancel("User Cancel");
                        return;
                    }
                    if (collisionDecision == MultiDestinationCollisionDecision.Skip)
                    {
                        CurrentFile.CopyState = CopyState.Skiped;
                        copier.FileBytesTransferred = 0;
                        copier.TotalBytesTransferred += item.Length;
                        CopiedsFiles++;
                        continue;
                    }

                    var skipped = copier
                        .CopyFileToDestinationsAsync(item, request.DestinationRoots, request.Threads)
                        .GetAwaiter()
                        .GetResult();

                    if (pauseAfterSkipRequested && skipped)
                    {
                        pauseAfterSkipRequested = false;
                        pauseGate.Reset();
                    }

                    CurrentFile.CopyState = skipped ? CopyState.Skiped : CopyState.Copied;
                    CopiedsFiles++;
                    UpdateCrashRecoveryForCurrentFile(CurrentFile.CopyState, false);
                }
                catch (OperationCanceledException)
                {
                    SaveCrashRecoveryCheckpointBestEffort();
                    State = CopyHandleState.Canceled;
                    return;
                }
                catch (Exception ex)
                {
                    Errors.Add(new CopyError { Message = ex.Message });
                    if (CurrentFile != null)
                    {
                        CurrentFile.CopyState = CopyState.Error;
                        UpdateCrashRecoveryForCurrentFile(CurrentFile.CopyState, false);
                    }
                    RaiseTransferError(this, CurrentFile, ex.Message);
                    Cancel("Multi-destination write failed");
                    return;
                }
            }

            SaveCrashRecoveryCheckpointBestEffort();
            State = CopyHandleState.Finished;
        }

        public void StartOperation()
        {
            if (Operation == null && State == CopyHandleState.NotStarted) return;

            StartOperationInternal();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">Used to check if must start in this files index or in index + 1</param>
        public void Skip(int index)
        {
            try
            {
                //If there is any file in copy process then stop copy,
                //free resources and delete the file with Cancel method.
                if (FileCopier.CurrentFile != null)
                {
                    FileCopier.CurrentFile.CopyState = CopyState.Skiped;
                    FileCopier.Skip();
                }
                // Ensure the active copy can observe skip; in paused state for multi-destination,
                // re-apply pause after skip so the transfer remains paused.
                if (State == CopyHandleState.Paused && MultiDestinationRequest != null)
                {
                    pauseAfterSkipRequested = true;
                    pauseGate.Set();
                }
                else
                {
                    pauseGate.Set();
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter w = new StreamWriter(new FileStream(logsPath, FileMode.Append, FileAccess.Write)))
                {
                    w.WriteLine("-------------------------------");
                    w.WriteLine(System.DateTime.Now);
                    w.WriteLine(Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "NeathCopyHandle", "Skip"));
                }

            }
        }

        private enum MultiDestinationCollisionDecision
        {
            Copy,
            Skip,
            Cancel
        }

        private MultiDestinationCollisionDecision ResolveMultiDestinationCollision(
            MultiDestinationCopyItem item,
            MultiDestinationCopyRequest request)
        {
            if (item == null || request == null || request.DestinationRoots == null || request.DestinationRoots.Count == 0)
                return MultiDestinationCollisionDecision.Copy;

            var hasCollision = request.DestinationRoots.Any(root =>
            {
                var path = LongPathHelper.Normalize(Path.Combine(root, item.RelativePath));
                return File.Exists(path);
            });

            if (!hasCollision)
                return MultiDestinationCollisionDecision.Copy;

            if (multiDestinationCollisionMode == MultiDestinationCollisionMode.OverwriteAll)
                return MultiDestinationCollisionDecision.Copy;

            if (multiDestinationCollisionMode == MultiDestinationCollisionMode.SkipAll)
                return MultiDestinationCollisionDecision.Skip;

            var action = FileExist != null ? FileExist(this, CurrentFile) : AllwaysAsk;
            if (action == null || action.Method == null)
                return MultiDestinationCollisionDecision.Copy;

            switch (action.Method.Name)
            {
                case nameof(OverwriteAll):
                case nameof(OverwriteAllDifferent):
                    multiDestinationCollisionMode = MultiDestinationCollisionMode.OverwriteAll;
                    return MultiDestinationCollisionDecision.Copy;
                case nameof(OverwriteCurrentFile):
                case nameof(OverwriteDifferent):
                    return MultiDestinationCollisionDecision.Copy;
                case nameof(SkipAll):
                    multiDestinationCollisionMode = MultiDestinationCollisionMode.SkipAll;
                    return MultiDestinationCollisionDecision.Skip;
                case nameof(SkipCurrentFile):
                    return MultiDestinationCollisionDecision.Skip;
                case nameof(CancelCollision):
                    return MultiDestinationCollisionDecision.Cancel;
                default:
                    return MultiDestinationCollisionDecision.Copy;
            }
        }
        private void LogSkip(int previousIndex, int newIndex, CopyHandleState previousState, CopyHandleState currentState)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(new FileStream(logsPath, FileMode.Append, FileAccess.Write)))
                {
                    w.WriteLine("-------------------------------");
                    w.WriteLine(System.DateTime.Now);
                    w.WriteLine(string.Format("Skip: index {0} -> {1}, State {2} -> {3}", previousIndex, newIndex, previousState, currentState));
                }
            }
            catch (Exception)
            {
            }
        }
        public void Pause()
        {
            if (State == CopyHandleState.Runing)
            {
                pauseGate.Reset();

                State = CopyHandleState.Paused;
            }
        }
        public void Resume()
        {
            if (State == CopyHandleState.Paused)
            {
                pauseGate.Set();

                State = CopyHandleState.Runing;
            }
        }
        public void Cancel(string cause)
        {
            try
            {
                var previousState = State;
                State = CopyHandleState.Canceled;

                //Terminate the copy process
                if (operationCts != null)
                {
                    operationCts.Cancel();
                    pauseGate.Set();
                }

                if (previousState == CopyHandleState.Runing && FileCopier.Writer != null)
                {
                    //If there is any file in copy process then stop copy,
                    //free resources and delete the file with Cancel method.
                    if (FileCopier.CurrentFile != null)
                        FileCopier.Cancel();
                }

                //Inform cancel action.
                RaiseCanceled(cause);

                if (!crashRecoverySavedAtCancel)
                {
                    crashRecoverySavedAtCancel = true;
                    SaveCrashRecoveryCheckpointBestEffort();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(string.Format("An Error has ocurred in Module: {0}, Class: {1}, Method: {2}, Error: {3}", "NeathCopyEngine", "NeathCopyHandle", "Cancel", ex.Message));
                using (var w = new StreamWriter(new FileStream(logsPath, FileMode.Append, FileAccess.Write)))
                {
                    w.WriteLine("-------------------------------");
                    w.WriteLine(System.DateTime.Now);
                    w.WriteLine(string.Format("Module: {0}, Class: {1}, Method: {2}, Error: {3}", "NeathCopyEngine", "NeathCopyHandle", "Cancel", ex.Message));
                }

            }
        }

        private void EnsureCrashRecoveryInitializedForCurrentOperation(string operation, bool multipleDestiny)
        {
            if (!CrashRecoveryEnabled)
                return;

            if (crashRecoveryInitialized)
                return;

            var destinyLabel = "UnknownDestiny";
            if (DiscoverdList != null && DiscoverdList.Destinys != null && DiscoverdList.Destinys.Count > 0)
                destinyLabel = multipleDestiny
                    ? string.Format("Multiple_{0}", DiscoverdList.Destinys.Count)
                    : DiscoverdList.Destinys[0];

            InitializeCrashRecovery(destinyLabel, operation, multipleDestiny);
        }

        private void BuildCrashRecoveryListSnapshot()
        {
            var files = new List<FileOnList>();
            if (DiscoverdList != null && DiscoverdList.Files != null)
            {
                files = DiscoverdList.Files.Select(f => new FileOnList
                {
                    From = f.FullName,
                    To = f.DestinyPath,
                    CopyState = f.CopyState
                }).ToList();
            }

            crashRecoveryList = new SerializableFilesList
            {
                Operation = string.IsNullOrWhiteSpace(crashRecoveryOperation)
                    ? (DiscoverdList == null ? "copy" : DiscoverdList.Operation)
                    : crashRecoveryOperation,
                MultipleDestiny = crashRecoveryMultipleDestiny,
                Files = files,
                CurrentIndex = DiscoverdList == null ? 0 : DiscoverdList.Index,
                JobId = string.IsNullOrWhiteSpace(crashRecoveryList == null ? null : crashRecoveryList.JobId)
                    ? Guid.NewGuid().ToString("N")
                    : crashRecoveryList.JobId,
                CreatedUtc = crashRecoveryList == null || crashRecoveryList.CreatedUtc == default(DateTime)
                    ? DateTime.UtcNow
                    : crashRecoveryList.CreatedUtc,
                UpdatedUtc = DateTime.UtcNow
            };
        }

        private void UpdateCrashRecoveryForCurrentFile(CopyState state, bool setIndexAsCurrent)
        {
            if (!CrashRecoveryEnabled || !crashRecoveryInitialized)
                return;

            lock (crashLock)
            {
                if (crashRecoveryList == null)
                    BuildCrashRecoveryListSnapshot();

                if (crashRecoveryList == null)
                    return;

                var idx = DiscoverdList == null ? -1 : DiscoverdList.Index;
                if (idx >= 0)
                {
                    crashRecoveryList.CurrentIndex = idx;
                    if (idx < crashRecoveryList.Files.Count)
                        crashRecoveryList.Files[idx].CopyState = state;
                }

                if (setIndexAsCurrent && idx >= 0)
                    crashRecoveryList.CurrentIndex = idx;

                crashRecoveryList.UpdatedUtc = DateTime.UtcNow;
                SaveCrashRecoveryCheckpointUnsafe();
            }
        }

        private void SaveCrashRecoveryCheckpointBestEffort()
        {
            try
            {
                if (!CrashRecoveryEnabled || !crashRecoveryInitialized)
                    return;

                lock (crashLock)
                {
                    if (crashRecoveryList == null)
                        BuildCrashRecoveryListSnapshot();
                    else if (DiscoverdList != null)
                        crashRecoveryList.CurrentIndex = DiscoverdList.Index;

                    if (crashRecoveryList != null)
                    {
                        crashRecoveryList.UpdatedUtc = DateTime.UtcNow;
                        SaveCrashRecoveryCheckpointUnsafe();
                    }
                }
            }
            catch
            {
            }
        }

        private void SaveCrashRecoveryCheckpointUnsafe()
        {
            if (crashRecoveryList == null || string.IsNullOrWhiteSpace(CrashRecoveryCheckpointPath))
                return;

            AtomicSerializer.SerializeCompressedAtomic(
                crashRecoveryList,
                typeof(SerializableFilesList),
                CrashRecoveryCheckpointPath);
        }

        private void PrepareTempDestinationIfNeeded(bool fastMove)
        {
            if (!CrashRecoveryEnabled || fastMove || CurrentFile == null)
                return;

            var tempPath = CurrentFile.DestinyPath + ".neathcopytmp";
            CurrentFile.TempDestinyPath = tempPath;
            try
            {
                var normalized = LongPathHelper.Normalize(tempPath);
                if (File.Exists(normalized))
                    File.Delete(normalized);
            }
            catch
            {
            }
        }

        private void FinalizeTempDestinationIfNeeded()
        {
            FinalizeTempDestinationForFile(CurrentFile);
        }

        private void FinalizeTempDestinationForFile(FileDataInfo file)
        {
            if (!CrashRecoveryEnabled || file == null || string.IsNullOrWhiteSpace(file.TempDestinyPath))
                return;

            var tempPath = LongPathHelper.Normalize(file.TempDestinyPath);
            var finalPath = LongPathHelper.Normalize(file.DestinyPath);
            try
            {
                if (File.Exists(finalPath))
                    File.Delete(finalPath);

                if (File.Exists(tempPath))
                    File.Move(tempPath, finalPath);
            }
            finally
            {
                file.TempDestinyPath = null;
            }
        }

        private void CleanupTempDestinationIfNeeded()
        {
            if (CurrentFile == null || string.IsNullOrWhiteSpace(CurrentFile.TempDestinyPath))
                return;

            try
            {
                var tempPath = LongPathHelper.Normalize(CurrentFile.TempDestinyPath);
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch
            {
            }
            finally
            {
                CurrentFile.TempDestinyPath = null;
            }
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "CrashRecovery";

            var invalid = Path.GetInvalidFileNameChars();
            var clean = new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
            clean = clean.Replace(':', '_').Replace('\\', '_').Replace('/', '_');
            while (clean.Contains("__"))
                clean = clean.Replace("__", "_");

            clean = clean.Trim('_', ' ', '.');
            return string.IsNullOrWhiteSpace(clean) ? "CrashRecovery" : clean;
        }

        private static string GetDefaultFilesListFolder()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrWhiteSpace(documents))
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FilesList");

            return Path.Combine(documents, "NeathCopy", "FilesList");
        }

        #endregion

    }
}
