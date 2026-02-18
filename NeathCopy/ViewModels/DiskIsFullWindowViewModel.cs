using System;
using System.Windows.Input;
using NeathCopy.UsedWindows;

namespace NeathCopy.ViewModels
{
    public class DiskIsFullWindowViewModel : ViewModelBase
    {
        private string message;
        private DiskFullOption option;

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public DiskFullOption Option
        {
            get => option;
            private set => SetProperty(ref option, value);
        }

        public ICommand DeleteCommand { get; }
        public ICommand KeepCommand { get; }

        public event Action RequestClose;

        public DiskIsFullWindowViewModel()
        {
            DeleteCommand = new RelayCommand(() =>
            {
                Option = DiskFullOption.DeleteFile;
                RequestClose?.Invoke();
            });

            KeepCommand = new RelayCommand(() =>
            {
                Option = DiskFullOption.KeepFile;
                RequestClose?.Invoke();
            });
        }
    }
}
