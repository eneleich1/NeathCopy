using NeathCopy.Module2_Configuration;
using NeathCopy.Tools;
using NeathCopyEngine;
using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.DataTools;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
using static NeathCopy.VisualCopy;

namespace NeathCopy.ViewModels
{
    public class VisualCopyVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //Other Fields
        VisualCopy visualCopy;
        public VisualCopy VisualCopy
        {
            get => visualCopy;
            set
            {
                visualCopy = value;
                var args = new PropertyChangedEventArgs(nameof(VisualCopy));
                PropertyChanged?.Invoke(this, args);

                SetPauseResumeBrush();
            }
        }
        public Button pauseResume_bt { get; private set; }
        public TextBlock CopyListWndFilesCount_tb { get; set; }

        public VisualCopyVM()
        {
            TargetDevice = "Device";
            SpeedInfo = "0 Bytes/s";
            CurrentFileSizeTransferred = "0 Bytes of 0 Bytes";
            OverallSizeTransferred = "0 Bytes of 0 Bytes";
            FilesCount = "0 of 0";
            Operation = "Copy";
            CopyOption = "Allways Ask";

            Themes.ThemesManager.Manager.VisualCopySkingChanged += Manager_VisualCopySkingChanged;

            InitializeTimers();

        }

        private void Manager_VisualCopySkingChanged(string skin)
        {
            SetPauseResumeBrush();
        }

        #region Properties

        FileDataInfo currentFile;
        public FileDataInfo CurrentFile
        {
            get => currentFile;
            set
            {
                currentFile = value;
                var args = new PropertyChangedEventArgs(nameof(CurrentFile));
                PropertyChanged?.Invoke(this, args);

                if (currentFile != null)
                    From = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(currentFile.FullName);
            }
        }

        string from;
        public string From
        {
            get => from;
            set
            {
                from = value;
                var args = new PropertyChangedEventArgs(nameof(From));
                PropertyChanged?.Invoke(this, args);
            }
        }

        string to;
        public string To
        {
            get => to;
            set
            {
                to = value;
                var args = new PropertyChangedEventArgs(nameof(To));
                PropertyChanged?.Invoke(this, args);
            }
        }

        string targetDevice;
        public string TargetDevice
        {
            get => targetDevice;
            set
            {
                targetDevice = value;
                var args = new PropertyChangedEventArgs(nameof(TargetDevice));
                PropertyChanged?.Invoke(this, args);
            }
        }

        string speedInfo;
        public string SpeedInfo
        {
            get => speedInfo;
            set
            {
                speedInfo = value;
                var args = new PropertyChangedEventArgs(nameof(SpeedInfo));
                PropertyChanged?.Invoke(this, args);
            }
        }

        double speed;
        public double Speed
        {
            get => speed;
            set
            {
                speed = value;
                var args = new PropertyChangedEventArgs(nameof(Speed));
                PropertyChanged?.Invoke(this, args);
                SpeedInfo = string.Format("{0}/s", new MySize((long)speed));
            }
        }

        double maxSpeed;
        public double MaxSpeed
        {
            get => maxSpeed;
            set
            {
                maxSpeed = value;
                var args = new PropertyChangedEventArgs(nameof(MaxSpeed));
                PropertyChanged?.Invoke(this, args);
            }
        }

        Brush speedBrush;
        public Brush SpeedBrush
        {
            get => speedBrush;
            set
            {
                speedBrush = value;
                var args = new PropertyChangedEventArgs(nameof(SpeedBrush));
                PropertyChanged?.Invoke(this, args);
            }
        }

        string currentFileSizeTransferred;
        public string CurrentFileSizeTransferred
        {
            get => currentFileSizeTransferred;
            set
            {
                currentFileSizeTransferred = value;
                var args = new PropertyChangedEventArgs(nameof(CurrentFileSizeTransferred));
                PropertyChanged?.Invoke(this, args);
            }
        }

        string overallSizeTransferred;
        public string OverallSizeTransferred
        {
            get => overallSizeTransferred;
            set
            {
                overallSizeTransferred = value;
                var args = new PropertyChangedEventArgs(nameof(OverallSizeTransferred));
                PropertyChanged?.Invoke(this, args);
            }
        }

        string filesCount;
        public string FilesCount
        {
            get => filesCount;
            set
            {
                filesCount = value;
                var args = new PropertyChangedEventArgs(nameof(FilesCount));
                PropertyChanged?.Invoke(this, args);
            }
        }

        float singlePorcent;
        public float SinglePorcent
        {
            get => singlePorcent;
            set
            {
                singlePorcent = value;
                var args = new PropertyChangedEventArgs(nameof(SinglePorcent));
                PropertyChanged?.Invoke(this, args);
            }
        }

        float overallPorcent;
        public float OverallPorcent
        {
            get => overallPorcent;
            set
            {
                overallPorcent = value;
                var args = new PropertyChangedEventArgs(nameof(OverallPorcent));
                PropertyChanged?.Invoke(this, args);
            }
        }

        Time remainingTime;
        public Time RemainingTime
        {
            get => remainingTime;
            set
            {
                remainingTime = value;
                var args = new PropertyChangedEventArgs(nameof(RemainingTime));
                PropertyChanged?.Invoke(this, args);
            }
        }

        Time elapsedTime;
        public Time ElapsedTime
        {
            get => elapsedTime;
            set
            {
                elapsedTime = value;
                var args = new PropertyChangedEventArgs(nameof(ElapsedTime));
                PropertyChanged?.Invoke(this, args);
            }
        }

        string copyOption;
        public string CopyOption
        {
            get => copyOption;
            set
            {
                copyOption = value;
                var args = new PropertyChangedEventArgs(nameof(CopyOption));
                PropertyChanged?.Invoke(this, args);
            }
        }

        string operation;
        public string Operation
        {
            get => operation;
            set
            {
                operation = value;
                var args = new PropertyChangedEventArgs(nameof(Operation));
                PropertyChanged?.Invoke(this, args);
            }
        }

        float driverSizePorcent;
        public float DriverSizePorcent
        {
            get => driverSizePorcent;
            set
            {
                driverSizePorcent = value;
                var args = new PropertyChangedEventArgs(nameof(DriverSizePorcent));
                PropertyChanged?.Invoke(this, args);
            }
        }

        Brush driverSizeBrush;
        public Brush DriverSizeBrush
        {
            get => driverSizeBrush;
            set
            {
                driverSizeBrush = value;
                var args = new PropertyChangedEventArgs(nameof(DriverSizeBrush));
                PropertyChanged?.Invoke(this, args);
            }
        }

        string inqueueNumber;
        public string InqueueNumber
        {
            get => inqueueNumber;
            set
            {
                inqueueNumber = value;
                var args = new PropertyChangedEventArgs(nameof(InqueueNumber));
                PropertyChanged?.Invoke(this, args);
            }
        }

        VisualCopyState state;
        public VisualCopyState State
        {
            get => state;
            set
            {
                state = value;
                var args = new PropertyChangedEventArgs(nameof(State));
                PropertyChanged?.Invoke(this, args);

                SetPauseResumeBrush();
            }
        }

        private void SetPauseResumeBrush()
        {
            //Change the brush
            if (visualCopy == null) return;
            if (state == VisualCopyState.Paused || state == VisualCopyState.None || state == VisualCopyState.Finished)
                PauseButtonBrush = (Brush)visualCopy.FindResource("resumeStartBrush");
            else PauseButtonBrush = (Brush)visualCopy.FindResource("pauseBrush");
        }

        Brush pauseButtonBrush;
        public Brush PauseButtonBrush
        {
            get => pauseButtonBrush;
            set
            {
                pauseButtonBrush = value;
                var args = new PropertyChangedEventArgs(nameof(PauseButtonBrush));
                PropertyChanged?.Invoke(this, args);
            }
        }

        #endregion

        #region Methods to allow change the properties values

        #region Updates

        /// <summary>
        /// Update all fields and it's respective control wich display
        /// any process information.
        /// </summary>

        public void UpdateCurrentFileNameAndFrom()
        {
            //Name of CurrentFile
            if (VisualCopy.NeathCopy.CurrentFile != null)
            {
                //Current File
                CurrentFile = VisualCopy.NeathCopy.CurrentFile;
            }
        }
        public void UpdateToAndDevice()
        {
            if (VisualCopy == null) return;

            try
            {
                //To
                if (CurrentFile != null)
                {
                    To = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(CurrentFile.DestinyPath);

                    //Drive Info
                    VisualCopy.driveInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(To));
                    if (VisualCopy.driveInfo != null)
                        TargetDevice = string.Format("{0}({1}) {2}"
                            , VisualCopy.driveInfo.VolumeLabel.ShortVersion(4)
                            , VisualCopy.driveInfo.Name == null ? "" : VisualCopy.driveInfo.Name
                            , new MySize(VisualCopy.driveInfo.TotalSize));
                }

                //Operation, CopyOption
                if (VisualCopy.NeathCopy != null && VisualCopy.NeathCopy.Operation != null && VisualCopy.NeathCopy.FileCollisionAction != null)
                {
                    Operation = VisualCopy.NeathCopy.Operation.Method.Name;
                    CopyOption = VisualCopy.NeathCopy.FileCollisionAction.Method.Name;
                }

            }
            catch (Exception) { }
        }
        public void UpdateSpeed()
        {
            try
            {
                if (VisualCopy == null || VisualCopy.NeathCopy == null) return;

                /*
               * currenntCopySize -> elapsedTime
               *     x            -> 1000 1 sec
               */
                if (elapsedTime.AllMiliseconds == 0) return;

                //Way 1
                //totalBytesTransferred -> elapsed
                //           x          -> 1000
                // x = totalBytesTransferred * 1000/elapsed;
                //double speed = (NeathCopy.TotalBytesTransferred * 1000) / elapsedTime;

                //Way 2
                // ds -> UpdateSpeedInterval
                // x -> 1000 ms
                ds = VisualCopy.NeathCopy.TotalBytesTransferred - lastAmountSize;
                lastAmountSize = VisualCopy.NeathCopy.TotalBytesTransferred;

                if (Configuration.Main.UpdateTimeInterval == 0) return;
                Speed = (ds * 1000d) / Configuration.Main.UpdateTimeInterval;

                //Speed ProgressBar
                if (MaxSpeed == 0) return;
                if (MaxSpeed < Speed)
                    MaxSpeed = Speed;
                double porcent = MaxSpeed == 0 ? 100 : Speed * 100 / MaxSpeed;

                if (porcent < 40)
                    SpeedBrush = Brushes.Red;
                else if (porcent < 70)
                    SpeedBrush = Brushes.Orange;
                else SpeedBrush = Brushes.Green;
            }
            catch (Exception) { }
        }
        public void UpdateCurrentFileSizeTransferred()
        {
            try
            {
                //Total size trandferred of CurrentFile
                if (VisualCopy != null && VisualCopy.NeathCopy.CurrentFile != null)
                {
                    CurrentFileSizeTransferred = string.Format("{0} of {1}"
                        , new MySize(VisualCopy.NeathCopy.CurrentFileBytesTransferred)
                        , new MySize(VisualCopy.NeathCopy.CurrentFile.Size));
                }
            }
            catch (Exception) { }
        }
        public void UpdateTotalSizeTransferred()
        {
            try
            {
                if (VisualCopy == null) return;

                //Total size transferred
                OverallSizeTransferred = string.Format("{0} of {1}"
                    , new MySize(VisualCopy.NeathCopy.TotalBytesTransferred)
                    , VisualCopy.NeathCopy.DiscoverdList.Size);
            }
            catch (Exception) { }
        }
        public void UpdateTotalsFilesCopieds()
        {
            try
            {
                if (VisualCopy == null || VisualCopy.NeathCopy.DiscoverdList == null) return;

                //Total of Files Copieds
                FilesCount = string.Format("{0} of {1}"
                    , Math.Min(VisualCopy.NeathCopy.CopiedsFiles + 1, VisualCopy.NeathCopy.DiscoverdList.Count)
                    , VisualCopy.NeathCopy.DiscoverdList.Count);
            }
            catch (Exception) { }

        }
        public void UpdateProgressBarPorcents()
        {
            try
            {
                if (VisualCopy == null || VisualCopy.NeathCopy == null) return;

                //Single Porcent ProgressBar
                if (VisualCopy.NeathCopy.CurrentFile != null && VisualCopy.NeathCopy.CurrentFile.Size > 0)
                {
                    SinglePorcent = (VisualCopy.NeathCopy.CurrentFileBytesTransferred * 100f) / VisualCopy.NeathCopy.CurrentFile.Size;
                    //porcent = Math.Round(porcent, 2);

                    if (SinglePorcent > 100) SinglePorcent = 100;
                }

                //Overall Porcent ProgressBar
                if (VisualCopy.NeathCopy.DiscoverdList.Size > 0)
                {
                    OverallPorcent = (VisualCopy.NeathCopy.TotalBytesTransferred * 100f) / VisualCopy.NeathCopy.DiscoverdList.Size.Bytes;
                }
            }
            catch (Exception) { }
        }
        public void UpdateRemainingTime()
        {
            try
            {
                leftToCopy = VisualCopy.NeathCopy.DiscoverdList.Size.Bytes - VisualCopy.NeathCopy.TotalBytesTransferred;

                /*
                 * currenntCopySize -> elapsedTime
                 *     leftToCopy   ->  x
                 */

                if (VisualCopy.NeathCopy.TotalBytesTransferred > 0)
                    RemainingTime = new Time((elapsedTime.AllMiliseconds * leftToCopy) / VisualCopy.NeathCopy.TotalBytesTransferred);
            }
            catch (Exception) { }
        }
        public void UpdateElapsedTime()
        {
            try
            {
                if (VisualCopy == null || VisualCopy.NeathCopy == null) return;

                elapsedTime.AllMiliseconds += Configuration.Main.UpdateTimeInterval;

                ElapsedTime = new Time(elapsedTime.AllMiliseconds);
            }
            catch (Exception) { }
        }
        public void UpdateDriverSizeProgressBar()
        {
            try
            {
                if (VisualCopy == null || VisualCopy.driveInfo == null || VisualCopy.driveInfo.TotalSize == 0) return;

                try
                {
                    //Update DriveInfo
                    VisualCopy.driveInfo = new System.IO.DriveInfo(VisualCopy.driveInfo.Name);

                    //Driver Size ProgressBar
                    DriverSizePorcent = 100 - (VisualCopy.driveInfo.TotalFreeSpace * 100f) / VisualCopy.driveInfo.TotalSize;
                    DriverSizeBrush = driverSizePorcent > 75 ? Brushes.Red : Brushes.Green;
                }
                catch (Exception) { }
            }
            catch (Exception) { }

        }
        public void UpdateInqueveNo()
        {
            if (VisualCopy == null) return;

            //Inqueve #
            InqueueNumber = string.Format("InQueve #: {0}", (VisualCopy.InqueveState != QueueState.None) ? VisualCopy.InqueveId.ToString() : "None");

        }
        public void UpdateAllVisualsControls()
        {
            UpdateInqueveNo();
            UpdateToAndDevice();

            UpdateCurrentFileNameAndFrom();
            UpdateCurrentFileSizeTransferred();
            UpdateProgressBarPorcents();
            UpdateDriverSizeProgressBar();
            UpdateTotalSizeTransferred();
            UpdateTotalsFilesCopieds();

            if (VisualCopy.State == VisualCopyState.Runing)
            {
                UpdateSpeed();
                UpdateElapsedTime();
                UpdateRemainingTime();
            }
            else if (VisualCopy.State == VisualCopyState.Discovering)
            {
                UpdateElapsedTime();
                UpdateRemainingTime();
            }
        }

        #endregion

        #region Timers

        long lastAmountSize = 0;
        double ds;// speed, maxSpeed;
        long leftToCopy;

        #endregion

        #region Displaying Information Methods

        public Timer updateTime_timer;
        protected void InitializeTimers()
        {
            updateTime_timer = new Timer(Configuration.Main.UpdateTimeInterval);
            updateTime_timer.Elapsed += ElapsedTime_timer_Elapsed;
        }

        private void ElapsedTime_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateAllVisualsControls();
        }


        #endregion

        #endregion
    }
}
