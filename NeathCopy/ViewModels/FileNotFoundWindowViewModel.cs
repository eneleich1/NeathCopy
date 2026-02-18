using System;
using System.Windows.Documents;
using System.Windows.Input;
using NeathCopyEngine.CopyHandlers;

namespace NeathCopy.ViewModels
{
    public class FileNotFoundWindowViewModel : ViewModelBase
    {
        private FlowDocument infoDocument;
        private FileNotFoundOption option;

        public FlowDocument InfoDocument
        {
            get => infoDocument;
            set => SetProperty(ref infoDocument, value);
        }

        public FileNotFoundOption Option
        {
            get => option;
            private set => SetProperty(ref option, value);
        }

        public ICommand TryCommand { get; }
        public ICommand SkipCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action RequestClose;

        public FileNotFoundWindowViewModel()
        {
            TryCommand = new RelayCommand(() =>
            {
                Option = FileNotFoundOption.Skip;
                RequestClose?.Invoke();
            });
            SkipCommand = new RelayCommand(() =>
            {
                Option = FileNotFoundOption.Skip;
                RequestClose?.Invoke();
            });
            CancelCommand = new RelayCommand(() =>
            {
                Option = FileNotFoundOption.Cancel;
                RequestClose?.Invoke();
            });
        }
    }
}
