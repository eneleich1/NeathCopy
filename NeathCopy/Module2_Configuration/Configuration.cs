using NeathCopy.Themes;
using NeathCopy.Tools;
using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace NeathCopy.Module2_Configuration
{
    public class Configuration
    {
        #region Fields

        public string ConfigName { get; set; }

        SoundPlayer soundPlayer = new SoundPlayer();
        public static Configuration Main;

        /// <summary>
        /// There is only VisualCopy queve.
        /// </summary>
        public static List<VisualCopy> queve = new List<VisualCopy>();

        public static string AppDirectory = System.Windows.Forms.Application.StartupPath;

        #endregion

        #region Configurations Settings

        bool playSound_After_ADD_DATA,playSound_After_Finish,playSound_After_Cancel;
        /// <summary>
        /// Specific if can play sound after process ADD_DATA_MESSAGE or not.
        /// </summary>
        public bool PlaySound_After_ADD_DATA
        {
            get { return playSound_After_ADD_DATA; }
            set
            {
                playSound_After_ADD_DATA = value;
                RegisterAccess.Acces.SetConfigurationValue("PlaySound_After_ADD_DATA", playSound_After_ADD_DATA);
                RaiseSettingChanged();
            }
        }
        /// <summary>
        /// Specific if can play sound after operation finish or not.
        /// </summary>
        public bool PlaySound_After_Finish
        {
            get { return playSound_After_Finish; }
            set
            {
                playSound_After_Finish = value;
                RegisterAccess.Acces.SetConfigurationValue("PlaySound_After_Finish", playSound_After_Finish);
                RaiseSettingChanged();
            }
        }
        /// <summary>
        /// Specific if can play sound when operation was cancelled or not.
        /// </summary>
        public bool PlaySound_After_Cancel
        {
            get { return playSound_After_Cancel; }
            set
            {
                playSound_After_Cancel = value;
                RegisterAccess.Acces.SetConfigurationValue("PlaySound_After_Cancel", playSound_After_Cancel);
                RaiseSettingChanged();
            }
        }

        public string AddData_Sound = "Sounds\\add data.wav";
        public string FinishOperation_Sound = "Sounds\\task finished.wav";
        public string Cancell_Sound = "Sounds\\cancell.wav";

        //Themes
        string brush,font,language,theme,visualCopySkin;
        public string Brush
        {
            get { return brush; }
            set
            {
                brush = value;
                RegisterAccess.Acces.SetConfigurationValue("Brushes",brush);
                RaiseBrushsChanged();
                RaiseSettingChanged();
            }
        }
        public string Font
        {
            get { return font; }
            set
            {
                font = value;
                RegisterAccess.Acces.SetConfigurationValue("Font", font);
                RaiseSettingChanged();
            }
        }
        public string Language
        {
            get { return language; }
            set
            {
                language = value;
                RegisterAccess.Acces.SetConfigurationValue("Language", language);
                RaiseSettingChanged();
            }
        }
        public string Theme
        {
            get { return theme; }
            set
            {
                theme = value;
                RegisterAccess.Acces.SetConfigurationValue("Theme", theme);
                RaiseThemeChanged();
                RaiseSettingChanged();
            }
        }
        public string VisualCopySkin
        {
            get { return visualCopySkin; }
            set
            {
                visualCopySkin = value;
                RegisterAccess.Acces.SetConfigurationValue("VisualCopySkins", visualCopySkin);
                RaiseSkinChanged();
                RaiseSettingChanged();
            }
        }

        FileCopier currentFileCopier;
        public FileCopier CurrentFileCopier
        {
            get { return currentFileCopier; }
            set
            {
                currentFileCopier = value;
                RegisterAccess.Acces.SetConfigurationValue("FileCopier", currentFileCopier.Name);
                RaiseSettingChanged();
            }
        }

        int bufferSize;
        public int BufferSize
        {
            get { return bufferSize; }
            set
            {
                bufferSize = value;
                RegisterAccess.Acces.SetConfigurationValue("BufferSize", bufferSize.ToString());
                RaiseSettingChanged();
            }
        }

        //Update Times
        int updateTimeInterval;
        public int UpdateTimeInterval
        {
            get { return updateTimeInterval; }
            set
            {
                updateTimeInterval = value;
                RegisterAccess.Acces.SetConfigurationValue("UpdateTimeInterval", updateTimeInterval.ToString());
                RaiseSettingChanged();
            }
        }

        public static Dictionary<string, FileCopier> FileCopiers;
            //new FasterBufferFileCopier(1024*1024)
            //,new NotCopyFileCopier(1024)


        #endregion

        #region Valid Availables Settings

        public static List<string> Thems;
        public static List<string> VisualCopySkins;
        public static List<string> Brushes;
        public static List<string> Fonts;
        public static List<string> Languages;

        static Configuration()
        {
            Thems = new List<string> { "Windows8Theme.xaml", "WindowsStandar.xaml" };
            VisualCopySkins = new List<string> { "Windows8.xaml", "Advance2017Fixed.xaml", "Advance2017.xaml"};
            Brushes = new List<string> { "Windows8_Brushes.xaml", "Dark.xaml", "Light.xaml", "Blue.xaml"};
            Fonts = new List<string> { "Fonts1.xaml" };
            Languages = new List<string> { "English.xaml", "Spanish.xaml", "Frances.xaml", "Chino Tradicional.xaml" };

            FileCopiers = new Dictionary<string, FileCopier>();
            FileCopiers.Add("FasterBufferFileCopier", new FasterBufferFileCopier(1024 * 1024));
            FileCopiers.Add("NotCopyFileCopier", new NotCopyFileCopier(1024)); 
            FileCopiers.Add("ProducerConsumerFileCopier", new ProducerConsumerFileCopier(1024));
            FileCopiers.Add("DynamicBufferFileCopier", new DynamicBufferFileCopier(1024));
        }

        #endregion

        public Dictionary<string, Action> AffterErrorAction_WAYS;
        public Dictionary<string, Func<VisualCopy>> AddNewVisualCopy_WAYS;
        public Dictionary<string, Action<IEnumerable<VisualCopy>>> Process_ADD_DATA_WAYS;
        public Dictionary<string, Action<VisualCopy, RequestInfo>> SetRunningState_WAYS;
        public Configuration()
        {
            ConfigName = "Default";

            //Initialize Dictionaries to allow load configurations.
            AddNewVisualCopy_WAYS = new Dictionary<string, Func<VisualCopy>>();
            AddNewVisualCopy_WAYS.Add("AllInOne_AddNewVC", AllInOne_AddNewVC);
            AddNewVisualCopy_WAYS.Add("SeparateWindows_AddNewVC", SeparateWindows_AddNewVC);

            AffterErrorAction_WAYS = new Dictionary<string, Action>();
            AffterErrorAction_WAYS.Add("Keep_AffterErrorAction", Keep_AffterErrorAction);
            AffterErrorAction_WAYS.Add("Close_AffterErrorAction", Close_AffterErrorAction);

            Process_ADD_DATA_WAYS = new Dictionary<string, Action<IEnumerable<VisualCopy>>>();
            Process_ADD_DATA_WAYS.Add("AddToLast_Process_ADD_DATA", AddToLast_Process_ADD_DATA);
            Process_ADD_DATA_WAYS.Add("AddToFirsth_Process_ADD_DATA", AddToFirsth_Process_ADD_DATA);
            Process_ADD_DATA_WAYS.Add("SameDestiny_Process_ADD_DATA", SameDestiny_Process_ADD_DATA);
            Process_ADD_DATA_WAYS.Add("SameVolumen_Process_ADD_DATA", SameVolumen_Process_ADD_DATA);

            SetRunningState_WAYS = new Dictionary<string, Action<VisualCopy, RequestInfo>>();
            SetRunningState_WAYS.Add("StartOperation_SetRunningState", StartOperation_SetRunningState);
            SetRunningState_WAYS.Add("Inqueve_SetRunningState", Inqueve_SetRunningState);
        }

        #region Add New VisualCopy Behavior

        public Func<VisualCopy> addNewVisualCopy;
        public Func<VisualCopy> AddNewVisualCopy
        {
            get { return addNewVisualCopy; }
            set
            {
                addNewVisualCopy = value;
                RegisterAccess.Acces.SetConfigurationValue("AddNewVisualCopy", addNewVisualCopy.Method.Name);
            }
        }

        public VisualCopy AllInOne_AddNewVC()
        {
            if (!VisualsCopysHandler.ContainersList.Contains(VisualsCopysHandler.MainContainer))
            {
                VisualsCopysHandler.ContainersList.Add(VisualsCopysHandler.MainContainer);
                VisualsCopysHandler.MainContainer.Closing += container_Closing;
            }

            var vc = VisualsCopysHandler.MainContainer.AddNew();

            if (VisualsCopysHandler.MainContainer.Visibility != Visibility.Visible)
            {
                VisualsCopysHandler.MainContainer.Show();
            }

            return vc;
        }
        public VisualCopy SeparateWindows_AddNewVC()
        {
            VisualsCopysHandler.MainContainer = new ContainerWindow();
            VisualsCopysHandler.MainContainer.Closing += container_Closing;

            VisualsCopysHandler.ContainersList.Add(VisualsCopysHandler.MainContainer);

            var vc = VisualsCopysHandler.MainContainer.AddNew();
            VisualsCopysHandler.MainContainer.Show();

            return vc;
        }
        void container_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            VisualsCopysHandler.ContainersList.Remove((ContainerWindow)sender);

            if (VisualsCopysHandler.ContainersList.Count == 0)
                VisualsCopysHandler.MainHandler.Close();
        }

        #endregion

        #region Affter Error Action

        public Action affterErrorAction;
        public Action AffterErrorAction
        {
            get { return affterErrorAction; }
            set
            {
                affterErrorAction = value;
                RegisterAccess.Acces.SetConfigurationValue("AffterErrorAction", affterErrorAction.Method.Name);
            }
        }

        public void Keep_AffterErrorAction()
        {

        }
        public void Close_AffterErrorAction()
        {
           
        }

        #endregion


        #region Process ADD_DATA_MESSAGE Behavior

        Action<IEnumerable<VisualCopy>> addData;
        public Action<IEnumerable<VisualCopy>> Process_ADD_DATA
        {
            get { return addData; }
            set
            {
                addData = value;
                RegisterAccess.Acces.SetConfigurationValue("Process_ADD_DATA", addData.Method.Name);
            }
        }

        public void AddToLast_Process_ADD_DATA(IEnumerable<VisualCopy> VisualsCopys)
        {
            var requestInfo = RegisterAccess.Acces.GetLastCopyRequestInfo();

            var first = VisualsCopys.First();
            if (first.RequestInf.Content == RquestContent.None)
            {
                if (first.State == VisualCopy.VisualCopyState.Finished) return;
                SetRunningState(first, requestInfo);
            }
            else
            {
                var active = VisualsCopys.Last();
                if (active.State == VisualCopy.VisualCopyState.Finished) return;

                //If operations mach, them AddToCopy.
                if (active.RequestInf.Operation == requestInfo.Operation)
                {
                    //Add items to only active CopyHandle in BackGround
                    Task.Factory.StartNew(() =>
                    {
                        active.AddData(requestInfo,false);
                    });
                }
                else
                {
                    SetRunningState(VisualsCopysHandler.MainContainer.AddNew(), requestInfo);
                }
            }

            PLaySoundAfterOperation(PlaySound_After_ADD_DATA, AddData_Sound);

        }
        public void AddToFirsth_Process_ADD_DATA(IEnumerable<VisualCopy> VisualsCopys)
        {
            var requestInfo = RegisterAccess.Acces.GetLastCopyRequestInfo();

            var first = VisualsCopys.First();
            if (first.RequestInf.Content == RquestContent.None)
            {
                if (first.State == VisualCopy.VisualCopyState.Finished) return;
                SetRunningState(first, requestInfo);
            }
            else
            {
                var active = VisualsCopys.First();
                if (active.State == VisualCopy.VisualCopyState.Finished) return;

                //If operations mach, them AddToCopy.
                if (active.RequestInf.Operation == requestInfo.Operation)
                {
                    //Add items to only active CopyHandle in BackGround
                    Task.Factory.StartNew(() =>
                    {
                        active.AddData(requestInfo,false);
                    });
                }
                else
                {
                    SetRunningState(VisualsCopysHandler.MainContainer.AddNew(), requestInfo);
                }
            }

            PLaySoundAfterOperation(PlaySound_After_ADD_DATA, AddData_Sound);

        }
        public void SameDestiny_Process_ADD_DATA(IEnumerable<VisualCopy> VisualsCopys)
        {
            var info = RegisterAccess.Acces.GetLastCopyRequestInfo();

            var first = VisualsCopys.First();
            if (first.RequestInf.Content == RquestContent.None)
            {
                if (first.State == VisualCopy.VisualCopyState.Finished) return;
                SetRunningState(first, info);
            }
            else
            {
                var visuals = VisualsCopys.Where(v => v.RequestInf.Destiny == info.Destiny 
                && v.RequestInf.Operation == info.Operation && v.State!= VisualCopy.VisualCopyState.Finished);

                if (visuals != null && visuals.Count() > 0)
                {
                    //Add items to only active CopyHandle in Background
                    Task.Factory.StartNew(() =>
                    {
                        visuals.First().AddData(info,false);
                    });
                }
                else
                {
                    SetRunningState(AddNewVisualCopy(), info);
                }
            }

            PLaySoundAfterOperation(PlaySound_After_ADD_DATA, AddData_Sound);
        }
        public void SameVolumen_Process_ADD_DATA(IEnumerable<VisualCopy> VisualsCopys)
        {
            var info = RegisterAccess.Acces.GetLastCopyRequestInfo();

            var first = VisualsCopys.First();
            if (first.RequestInf.Content == RquestContent.None)
            {
                if (first.State == VisualCopy.VisualCopyState.Finished) return;
                SetRunningState(first, info);
            }
            else
            {
                var visuals = VisualsCopys.Where(v => Path.GetPathRoot(v.RequestInf.Destiny) == Path.GetPathRoot(info.Destiny) 
                && v.RequestInf.Operation == info.Operation && v.State!= VisualCopy.VisualCopyState.Finished);

                if (visuals != null && visuals.Count() > 0)
                {
                    //Add items to only active CopyHandle in Background
                    visuals.First().AddData(info, false);
                }
                else
                {
                    SetRunningState(AddNewVisualCopy(), info);
                }
            }

            PLaySoundAfterOperation(PlaySound_After_ADD_DATA, AddData_Sound);
        }

        #endregion

        #region Start Operation or Inqueve Behavior (SetRunningState)

        Action<VisualCopy, RequestInfo> setRunningState;
        public Action<VisualCopy, RequestInfo> SetRunningState
        {
            get { return setRunningState; }
            set
            {
                setRunningState = value;
                RegisterAccess.Acces.SetConfigurationValue("SetRunningState", setRunningState.Method.Name);
            }
        }
        public void StartOperation_SetRunningState(VisualCopy visualCopy, RequestInfo requestInfo)
        {
            visualCopy.AceptArgumentsFinished += visualCopy_AceptArgumentsFinished;

            if (requestInfo.Content == RquestContent.All)
                visualCopy.AceptRequest(requestInfo, false,false);
        }
        public void Inqueve_SetRunningState(VisualCopy visualCopy, RequestInfo requestInfo)
        {
            visualCopy.InqueveState = QueueState.Waiting;
            queve.Add(visualCopy);
            visualCopy.InqueveId = queve.Count;

            visualCopy.AceptArgumentsFinished += visualCopy_AceptArgumentsFinished1;

            if (requestInfo.Content == RquestContent.All)
                visualCopy.AceptRequest(requestInfo, false,false);
        }

        #endregion 

        #region Methods

        /// <summary>
        /// Play the specific sound depending of play param.
        /// Used after operation finish or in cancel action.
        /// </summary>
        /// <param name="play"></param>
        /// <param name="sound"></param>
        public void PLaySoundAfterOperation(bool play, string sound)
        {
            if (play)
            {
                //Animate the window or play sound
                soundPlayer.SoundLocation = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, sound);
                soundPlayer.Play();

                System.Threading.Thread.Sleep(850);
            }
        }
        /// <summary>
        /// Remove the specific VisualCopy from a VisualsCopy queve if it 
        /// below to this queve. Them start the next one inqueved based on startNext param.
        /// </summary>
        /// <param name="visualCopy"></param>
        /// <param name="startNext"></param>
        public void RemoveFromQueve(VisualCopy visualCopy, bool startNext)
        {
            //If below to VisualsCopy queve.
            if (visualCopy.InqueveState != QueueState.None)
            {
                //Remove from queve
                queve.Remove(visualCopy);
                visualCopy.InqueveState = QueueState.None;
                //[DI]visualCopy.displayInfo.UpdateInqueveNo();

                //Update others inqueved VisualsCopys index
                foreach (var vc in VisualsCopysHandler.VisualsCopys.Where(c => c.InqueveId > visualCopy.InqueveId))
                {
                    vc.InqueveId--;
                    //[DI]vc.displayInfo.UpdateInqueveNo();
                }

                //Start the next one
                if (startNext)
                    Configuration.Main.StartNextInQueveVC();

            }
        }
        /// <summary>
        /// Start the next one in VisualCopy queve.
        /// Only start if next VisualCopy is in Waiting.
        /// 
        /// </summary>
        public void StartNextInQueveVC()
        {
            if (queve.Count > 0)
            {
                //Check thats next vc does not been started, is waiting for a turn.
                if (queve[0].InqueveState == QueueState.Waiting)
                {
                    queve[0].InqueveState = QueueState.StartedRuning;
                    //queve[0].CurrentPauseButtonAction = VisualCopy.PauseButtonAction.Pause;
                    queve[0].PerformOperation();
                }
            }
        }

        public static Configuration LoadDefault()
        {
            var config = new Configuration();

            config.AddNewVisualCopy = config.AddNewVisualCopy_WAYS["SeparateWindows_AddNewVC"];
            config.Process_ADD_DATA = config.Process_ADD_DATA_WAYS["SameVolumen_Process_ADD_DATA"];
            config.SetRunningState = config.SetRunningState_WAYS["StartOperation_SetRunningState"];
            config.AffterErrorAction = config.AffterErrorAction_WAYS["Keep_AffterErrorAction"];

            config.CurrentFileCopier = FileCopiers["FasterBufferFileCopier"];
            config.BufferSize = 65536;

            config.PlaySound_After_ADD_DATA = false;
            config.PlaySound_After_Finish = true;
            config.PlaySound_After_Cancel = true;

            config.Brush = "Blue.xaml";
            config.Font = "Fonts1.xaml";
            config.Language = "English.xaml";
            config.Theme = "Windows8Theme.xaml";
            config.VisualCopySkin = "Windows8.xaml";
            config.UpdateTimeInterval = 200;

            return config;
        }
        public static Configuration LoadFromRegister()
        {
            if (RegisterAccess.Acces.ExistConfiguration())
            {
                var config = new Configuration();

                config.PlaySound_After_ADD_DATA = bool.Parse(RegisterAccess.Acces.GetConfigurationValue("PlaySound_After_ADD_DATA"));
                config.PlaySound_After_Finish = bool.Parse(RegisterAccess.Acces.GetConfigurationValue("PlaySound_After_Finish"));
                config.PlaySound_After_Cancel = bool.Parse(RegisterAccess.Acces.GetConfigurationValue("PlaySound_After_Cancel"));

                config.BufferSize = int.Parse(RegisterAccess.Acces.GetConfigurationValue("BufferSize"));
                config.CurrentFileCopier = FileCopiers[RegisterAccess.Acces.GetConfigurationValue("FileCopier")];

                var action = RegisterAccess.Acces.GetConfigurationValue("AddNewVisualCopy");
                config.AddNewVisualCopy = config.AddNewVisualCopy_WAYS[action];

                action = RegisterAccess.Acces.GetConfigurationValue("AffterErrorAction");
                config.AffterErrorAction = config.AffterErrorAction_WAYS[action];


                action = RegisterAccess.Acces.GetConfigurationValue("Process_ADD_DATA");
                config.Process_ADD_DATA = config.Process_ADD_DATA_WAYS[action];

                action = RegisterAccess.Acces.GetConfigurationValue("SetRunningState");
                config.SetRunningState = config.SetRunningState_WAYS[action];

                config.Brush = RegisterAccess.Acces.GetConfigurationValue("Brushes");
                config.Font = RegisterAccess.Acces.GetConfigurationValue("Font");
                config.Language = RegisterAccess.Acces.GetConfigurationValue("Language");
                config.Theme = RegisterAccess.Acces.GetConfigurationValue("Theme");
                config.VisualCopySkin = RegisterAccess.Acces.GetConfigurationValue("VisualCopySkins");

                config.UpdateTimeInterval = int.Parse(RegisterAccess.Acces.GetConfigurationValue("UpdateTimeInterval"));

                return config;

            }
            else
                return LoadDefault();
        }
        public void SaveToRegister()
        {

        }

        #endregion

        #region Event Handlers

        void visualCopy_AceptArgumentsFinished(VisualCopy sender)
        {
            //sender.CurrentPauseButtonAction = VisualCopy.PauseButtonAction.Pause;
            sender.PerformOperation();
        }
        void visualCopy_AceptArgumentsFinished1(VisualCopy sender)
        {
            //If the visualcopy is the first in queve => start
            if (queve[0].InqueveId == sender.InqueveId)
            {
                sender.InqueveState = QueueState.StartedRuning;
                //sender.CurrentPauseButtonAction = VisualCopy.PauseButtonAction.Pause;

                sender.PerformOperation();
            }
            else
            {
                //sender.CurrentPauseButtonAction = VisualCopy.PauseButtonAction.StartNow;
            }
        }

        #endregion

        #region Events

        public delegate void SkinChangedEventHandler();
        /// <summary>
        /// Occurs When user change Visual Copy Skin
        /// </summary>
        public event SkinChangedEventHandler SkinChanged;
        protected void RaiseSkinChanged()
        {
            if (SkinChanged != null)
                SkinChanged();
        }

        public delegate void ThemeChangedEventHandler();
        /// <summary>
        /// Occurs When User change Windows Theme
        /// </summary>
        public event SkinChangedEventHandler ThemeChanged;
        protected void RaiseThemeChanged()
        {
            if (ThemeChanged != null)
                ThemeChanged();
        }

        public delegate void BruhsChangedEventHandler();
        /// <summary>
        /// Occurs When User change Brushs
        /// </summary>
        public event BruhsChangedEventHandler BrushsChanged;
        protected void RaiseBrushsChanged()
        {
            if (BrushsChanged != null)
                BrushsChanged();
        }

        public delegate void SettingChangedEventHandler();
        /// <summary>
        /// Occurs When User change Brushs
        /// </summary>
        public event SettingChangedEventHandler SettingChanged;
        protected void RaiseSettingChanged()
        {
            if (SettingChanged != null)
                SettingChanged();
        }

        #endregion

    }
}
