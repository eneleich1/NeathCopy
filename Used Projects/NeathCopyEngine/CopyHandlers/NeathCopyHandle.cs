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

        #endregion

        public NeathCopyHandle() : base()
        {
            FileCopier = new FasterBufferFileCopier(1024 * 1024);

            operationStaus = OperationStatus.NotStarted;
            transferErrorOption = TransferErrorOption.SkipCurrentFile;
            FileCollisionAction = AllwaysAsk;
            logsPath = Path.Combine(RegisterAccess.Acces.GetLogsDir(), "Errors Log.txt");

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
                    CurrentFile.CopyState = CopyState.Processing;

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
                        }
                        else
                        {
                            CurrentFile.CopyState = affeterOperationState;

                            //Status.
                            CopiedsFiles++;
                        }

                    }

                }
                catch (Exception ex)
                {
                    if (CurrentFile != null && CurrentFile.CopyState == CopyState.Skiped)
                        continue;

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
                        CurrentFile.CopyState = CopyState.Error;
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
            State = CopyHandleState.Runing;
            ConfigureExecutionContext();

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

            State = CopyHandleState.Finished;

        }
        public override void Move()
        {
            State = CopyHandleState.Runing;
            ConfigureExecutionContext();

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

            State = CopyHandleState.Finished;

        }
        public override void FastMove()
        {
            State = CopyHandleState.Runing;
            ConfigureExecutionContext();

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
                // Ensure we are not blocked on pause so the current copy can exit.
                pauseGate.Set();
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

        #endregion

    }
}
