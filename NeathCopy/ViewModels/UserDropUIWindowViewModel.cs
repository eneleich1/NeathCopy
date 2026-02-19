using System;
using System.Windows.Input;
using System.Windows;
using System.IO;
using NeathCopyEngine.Helpers;

namespace NeathCopy.ViewModels
{
    public class UserDropUIWindowViewModel : ViewModelBase
    {
        private int selectedOperationIndex;
        private string destiny;
        private bool dlgResult;
        private string title;

        public int SelectedOperationIndex
        {
            get => selectedOperationIndex;
            set => SetProperty(ref selectedOperationIndex, value);
        }

        public string Destiny
        {
            get => destiny;
            set => SetProperty(ref destiny, value);
        }

        public bool DlgResult
        {
            get => dlgResult;
            private set => SetProperty(ref dlgResult, value);
        }

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public string Operation => SelectedOperationIndex == 1 ? "move" : "copy";

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseCommand { get; }

        public event Action RequestClose;
        public event Action RequestBrowse;

        public UserDropUIWindowViewModel()
        {
            SelectedOperationIndex = 0;

            OkCommand = new RelayCommand(() =>
            {
                if (string.IsNullOrWhiteSpace(Destiny))
                    return;

                if (!Directory.Exists(LongPathHelper.Normalize(Destiny)))
                {
                    MessageBox.Show("The specific Destiny is not valid");
                    return;
                }

                DlgResult = true;
                RequestClose?.Invoke();
            });

            CancelCommand = new RelayCommand(() =>
            {
                DlgResult = false;
                RequestClose?.Invoke();
            });

            BrowseCommand = new RelayCommand(() => RequestBrowse?.Invoke());
        }
    }
}
