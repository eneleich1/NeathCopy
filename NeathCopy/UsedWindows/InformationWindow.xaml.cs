using System;
using System.Windows;
using System.Windows.Documents;
using NeathCopy.ViewModels;
using NeathCopyEngine.DataTools;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for InformationWindow.xaml
    /// </summary>
    public partial class InformationWindow : Window
    {
        private readonly InformationWindowViewModel viewModel;

        public string SoundPath { get; set; }
        public InformationWindowUserAction UserSelectedAction => viewModel.UserSelectedAction;

        public InformationWindow(string soundPath)
        {
            SoundPath = soundPath;

            InitializeComponent();

            viewModel = new InformationWindowViewModel();
            viewModel.RequestClose += () => Hide();
            DataContext = viewModel;
        }

        public void ShowMessage(IDriveInfo driveInfo, long freeSpace, long requireSpace)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var message = string.Format("There is not enough free space in {0} volumen. Try delete something firsth.", driveInfo.VolumeLabel);
                viewModel.MessageDocument = new FlowDocument(new Paragraph(new Run(message)));

                viewModel.Drives.Clear();
                viewModel.Drives.Add(new FormattedDriveInfo(driveInfo, freeSpace, requireSpace));

                ShowDialog();
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
