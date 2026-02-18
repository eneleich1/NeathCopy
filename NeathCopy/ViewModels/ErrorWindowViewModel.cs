using System;
using System.Windows.Documents;
using System.Windows.Input;
using NeathCopyEngine.CopyHandlers;

namespace NeathCopy.ViewModels
{
    public class ErrorWindowViewModel : ViewModelBase
    {
        private FlowDocument infoDocument;
        private TransferErrorOption option;

        public FlowDocument InfoDocument
        {
            get => infoDocument;
            set => SetProperty(ref infoDocument, value);
        }

        public TransferErrorOption Option
        {
            get => option;
            private set => SetProperty(ref option, value);
        }

        public ICommand TryCommand { get; }
        public ICommand SkipAllCommand { get; }
        public ICommand SkipCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action RequestClose;

        public ErrorWindowViewModel()
        {
            TryCommand = new RelayCommand(() =>
            {
                Option = TransferErrorOption.Try;
                RequestClose?.Invoke();
            });
            SkipAllCommand = new RelayCommand(() =>
            {
                Option = TransferErrorOption.SkipAll;
                RequestClose?.Invoke();
            });
            SkipCommand = new RelayCommand(() =>
            {
                Option = TransferErrorOption.SkipCurrentFile;
                RequestClose?.Invoke();
            });
            CancelCommand = new RelayCommand(() =>
            {
                Option = TransferErrorOption.Cancel;
                RequestClose?.Invoke();
            });
        }
    }
}
