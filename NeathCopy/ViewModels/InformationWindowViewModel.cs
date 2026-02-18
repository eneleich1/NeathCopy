using System;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Input;

namespace NeathCopy.ViewModels
{
    public class InformationWindowViewModel : ViewModelBase
    {
        private FlowDocument messageDocument;
        private InformationWindowUserAction userSelectedAction;

        public FlowDocument MessageDocument
        {
            get => messageDocument;
            set => SetProperty(ref messageDocument, value);
        }

        public ObservableCollection<FormattedDriveInfo> Drives { get; }

        public InformationWindowUserAction UserSelectedAction
        {
            get => userSelectedAction;
            private set => SetProperty(ref userSelectedAction, value);
        }

        public ICommand TryCommand { get; }
        public ICommand IgnoreCommand { get; }
        public ICommand FitCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action RequestClose;

        public InformationWindowViewModel()
        {
            Drives = new ObservableCollection<FormattedDriveInfo>();

            TryCommand = new RelayCommand(() =>
            {
                UserSelectedAction = InformationWindowUserAction.Try;
                RequestClose?.Invoke();
            });
            IgnoreCommand = new RelayCommand(() =>
            {
                UserSelectedAction = InformationWindowUserAction.Ignore;
                RequestClose?.Invoke();
            });
            FitCommand = new RelayCommand(() =>
            {
                UserSelectedAction = InformationWindowUserAction.Fit;
                RequestClose?.Invoke();
            });
            CancelCommand = new RelayCommand(() =>
            {
                UserSelectedAction = InformationWindowUserAction.Cancel;
                RequestClose?.Invoke();
            });
        }
    }

    public enum InformationWindowUserAction
    {
        Try,
        Ignore,
        Cancel,
        None,
        Fit
    }
}
