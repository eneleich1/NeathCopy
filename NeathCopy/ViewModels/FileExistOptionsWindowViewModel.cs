using System;
using System.Windows.Documents;
using System.Windows.Input;
using NeathCopyEngine.CopyHandlers;

namespace NeathCopy.ViewModels
{
    public class FileExistOptionsWindowViewModel : ViewModelBase
    {
        private FlowDocument infoDocument;
        private int selectedOptionIndex;
        private FileCopyOptions option;

        public FlowDocument InfoDocument
        {
            get => infoDocument;
            set => SetProperty(ref infoDocument, value);
        }

        public int SelectedOptionIndex
        {
            get => selectedOptionIndex;
            set => SetProperty(ref selectedOptionIndex, value);
        }

        public FileCopyOptions Option
        {
            get => option;
            private set => SetProperty(ref option, value);
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action RequestClose;

        public FileExistOptionsWindowViewModel()
        {
            SelectedOptionIndex = 0;

            OkCommand = new RelayCommand(() =>
            {
                switch (SelectedOptionIndex)
                {
                    case 0:
                        Option = FileCopyOptions.OverwriteIfFileExist;
                        break;
                    case 1:
                        Option = FileCopyOptions.AllwaysOverride;
                        break;
                    case 2:
                        Option = FileCopyOptions.OverrideDifferent;
                        break;
                    case 3:
                        Option = FileCopyOptions.AllwaysOverrideDifferent;
                        break;
                    case 4:
                        Option = FileCopyOptions.SkipIfFileExist;
                        break;
                    case 5:
                        Option = FileCopyOptions.AllwaysSkip;
                        break;
                }

                RequestClose?.Invoke();
            });

            CancelCommand = new RelayCommand(() =>
            {
                Option = FileCopyOptions.Cancel;
                RequestClose?.Invoke();
            });
        }
    }
}
