using NeathCopy.Module2_Configuration;
using NeathCopy.Themes;
using NeathCopyEngine.CopyHandlers;
using System;
using System.Collections.Generic;

namespace NeathCopy.ViewModels
{
    public class ConfigurationWindowViewModel : ViewModelBase
    {
        private bool loading;

        public IEnumerable<string> Themes => Configuration.Thems;
        public IEnumerable<string> Skins => Configuration.VisualCopySkins;
        public IEnumerable<string> Brushes => Configuration.Brushes;
        public IEnumerable<string> Fonts => Configuration.Fonts;
        public IEnumerable<string> Languages => Configuration.Languages;
        public IEnumerable<FileCopier> FileCopiers => Configuration.FileCopiers.Values;

        private string selectedTheme;
        public string SelectedTheme
        {
            get => selectedTheme;
            set
            {
                if (SetProperty(ref selectedTheme, value) && !loading)
                {
                    Configuration.Main.Theme = value;
                    ThemesManager.Manager.SetTheme(value);
                }
            }
        }

        private string selectedSkin;
        public string SelectedSkin
        {
            get => selectedSkin;
            set
            {
                if (SetProperty(ref selectedSkin, value) && !loading)
                {
                    Configuration.Main.VisualCopySkin = value;
                    ThemesManager.Manager.SetVisualCopySkins(value);
                }
            }
        }

        private string selectedBrush;
        public string SelectedBrush
        {
            get => selectedBrush;
            set
            {
                if (SetProperty(ref selectedBrush, value) && !loading)
                {
                    Configuration.Main.Brush = value;
                    ThemesManager.Manager.SetBrushes(value);
                }
            }
        }

        private string selectedFont;
        public string SelectedFont
        {
            get => selectedFont;
            set
            {
                if (SetProperty(ref selectedFont, value) && !loading)
                {
                    Configuration.Main.Font = value;
                    ThemesManager.Manager.SetFonts(value);
                }
            }
        }

        private string selectedLanguage;
        public string SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                if (SetProperty(ref selectedLanguage, value) && !loading)
                {
                    Configuration.Main.Language = value;
                    ThemesManager.Manager.SetLanguages(value);
                }
            }
        }

        private FileCopier selectedFileCopier;
        public FileCopier SelectedFileCopier
        {
            get => selectedFileCopier;
            set
            {
                if (SetProperty(ref selectedFileCopier, value) && !loading && value != null)
                    Configuration.Main.CurrentFileCopier = value;
            }
        }

        private string bufferSizeText;
        public string BufferSizeText
        {
            get => bufferSizeText;
            set
            {
                if (!SetProperty(ref bufferSizeText, value))
                    return;

                if (loading)
                    return;

                int tmp = Configuration.Main.BufferSize;
                if (int.TryParse(value, out var parsed))
                    Configuration.Main.BufferSize = parsed;
                else
                    Configuration.Main.BufferSize = tmp;
            }
        }

        private string updateTimeText;
        public string UpdateTimeText
        {
            get => updateTimeText;
            set
            {
                if (!SetProperty(ref updateTimeText, value))
                    return;

                if (loading)
                    return;

                int tmp = Configuration.Main.UpdateTimeInterval;
                if (int.TryParse(value, out var parsed))
                    Configuration.Main.UpdateTimeInterval = parsed;
                else
                    Configuration.Main.UpdateTimeInterval = tmp;
            }
        }

        private bool playSoundAfterAddData;
        public bool PlaySoundAfterAddData
        {
            get => playSoundAfterAddData;
            set
            {
                if (SetProperty(ref playSoundAfterAddData, value) && !loading)
                    Configuration.Main.PlaySound_After_ADD_DATA = value;
            }
        }

        private bool playSoundAfterFinish;
        public bool PlaySoundAfterFinish
        {
            get => playSoundAfterFinish;
            set
            {
                if (SetProperty(ref playSoundAfterFinish, value) && !loading)
                    Configuration.Main.PlaySound_After_Finish = value;
            }
        }

        private bool playSoundAfterCancel;
        public bool PlaySoundAfterCancel
        {
            get => playSoundAfterCancel;
            set
            {
                if (SetProperty(ref playSoundAfterCancel, value) && !loading)
                    Configuration.Main.PlaySound_After_Cancel = value;
            }
        }

        private bool addToFirsth;
        public bool AddToFirsth
        {
            get => addToFirsth;
            set
            {
                if (SetProperty(ref addToFirsth, value) && value && !loading)
                    Configuration.Main.addDataBehaviour = Configuration.addDataFac.GetBehaviour("AddToFirsth");
            }
        }

        private bool addToLast;
        public bool AddToLast
        {
            get => addToLast;
            set
            {
                if (SetProperty(ref addToLast, value) && value && !loading)
                    Configuration.Main.addDataBehaviour = Configuration.addDataFac.GetBehaviour("AddToLast");
            }
        }

        private bool addToSameDestiny;
        public bool AddToSameDestiny
        {
            get => addToSameDestiny;
            set
            {
                if (SetProperty(ref addToSameDestiny, value) && value && !loading)
                    Configuration.Main.addDataBehaviour = Configuration.addDataFac.GetBehaviour("AddToSameDestiny");
            }
        }

        private bool addToSameVolumen;
        public bool AddToSameVolumen
        {
            get => addToSameVolumen;
            set
            {
                if (SetProperty(ref addToSameVolumen, value) && value && !loading)
                    Configuration.Main.addDataBehaviour = Configuration.addDataFac.GetBehaviour("AddToSameVolumen");
            }
        }

        private bool startAutomatically;
        public bool StartAutomatically
        {
            get => startAutomatically;
            set
            {
                if (SetProperty(ref startAutomatically, value) && value && !loading)
                    Configuration.Main.SetRunningState = Configuration.Main.StartOperation_SetRunningState;
            }
        }

        private bool waitInQueue;
        public bool WaitInQueue
        {
            get => waitInQueue;
            set
            {
                if (SetProperty(ref waitInQueue, value) && value && !loading)
                    Configuration.Main.SetRunningState = Configuration.Main.Inqueve_SetRunningState;
            }
        }

        private bool allInOne;
        public bool AllInOne
        {
            get => allInOne;
            set
            {
                if (SetProperty(ref allInOne, value) && value && !loading)
                    Configuration.Main.AddNewVisualCopy = Configuration.Main.AllInOne_AddNewVC;
            }
        }

        private bool separateWindows;
        public bool SeparateWindows
        {
            get => separateWindows;
            set
            {
                if (SetProperty(ref separateWindows, value) && value && !loading)
                    Configuration.Main.AddNewVisualCopy = Configuration.Main.SeparateWindows_AddNewVC;
            }
        }

        private bool closeAfterError;
        public bool CloseAfterError
        {
            get => closeAfterError;
            set
            {
                if (SetProperty(ref closeAfterError, value) && value && !loading)
                    Configuration.Main.AffterErrorAction = Configuration.Main.Close_AffterErrorAction;
            }
        }

        private bool keepAfterError;
        public bool KeepAfterError
        {
            get => keepAfterError;
            set
            {
                if (SetProperty(ref keepAfterError, value) && value && !loading)
                    Configuration.Main.AffterErrorAction = Configuration.Main.Keep_AffterErrorAction;
            }
        }

        public void LoadFromConfiguration()
        {
            loading = true;

            SelectedTheme = Configuration.Main.Theme;
            SelectedSkin = Configuration.Main.VisualCopySkin;
            SelectedBrush = Configuration.Main.Brush;
            SelectedFont = Configuration.Main.Font;
            SelectedLanguage = Configuration.Main.Language;

            SelectedFileCopier = Configuration.Main.CurrentFileCopier;

            AddToSameDestiny = Configuration.Main.addDataBehaviour.Name == "AddToSameDestiny";
            AddToSameVolumen = Configuration.Main.addDataBehaviour.Name == "AddToSameVolumen";
            AddToFirsth = Configuration.Main.addDataBehaviour.Name == "AddToFirsth";
            AddToLast = Configuration.Main.addDataBehaviour.Name == "AddToLast";

            StartAutomatically = Configuration.Main.SetRunningState.Method.Name == "StartOperation_SetRunningState";
            WaitInQueue = Configuration.Main.SetRunningState.Method.Name == "Inqueve_SetRunningState";

            AllInOne = Configuration.Main.AddNewVisualCopy.Method.Name == "AllInOne_AddNewVC";
            SeparateWindows = Configuration.Main.AddNewVisualCopy.Method.Name == "SeparateWindows_AddNewVC";

            CloseAfterError = Configuration.Main.AffterErrorAction.Method.Name == "Close_AffterErrorAction";
            KeepAfterError = Configuration.Main.AffterErrorAction.Method.Name == "Keep_AffterErrorAction";

            BufferSizeText = Configuration.Main.BufferSize.ToString();
            UpdateTimeText = Configuration.Main.UpdateTimeInterval.ToString();

            PlaySoundAfterAddData = Configuration.Main.PlaySound_After_ADD_DATA;
            PlaySoundAfterFinish = Configuration.Main.PlaySound_After_Finish;
            PlaySoundAfterCancel = Configuration.Main.PlaySound_After_Cancel;

            loading = false;
        }
    }
}
