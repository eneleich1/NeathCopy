using System;
using System.Windows.Input;

namespace NeathCopy.ViewModels
{
    public class MessageWindowViewModel : ViewModelBase
    {
        private string message;

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public ICommand OkCommand { get; }

        public event Action RequestClose;

        public MessageWindowViewModel()
        {
            OkCommand = new RelayCommand(() => RequestClose?.Invoke());
        }
    }
}
