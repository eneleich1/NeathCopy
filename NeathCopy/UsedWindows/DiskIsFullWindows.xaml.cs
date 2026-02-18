using System;
using System.Windows;
using NeathCopy.ViewModels;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for DiskIsFullWindows.xaml
    /// </summary>
    public partial class DiskIsFullWindows : Window
    {
        private readonly DiskIsFullWindowViewModel viewModel;

        public DiskFullOption Option => viewModel.Option;

        public DiskIsFullWindows()
        {
            InitializeComponent();

            viewModel = new DiskIsFullWindowViewModel();
            viewModel.RequestClose += () => Hide();
            DataContext = viewModel;
        }

        public void ShowMessage(string text)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                viewModel.Message = text;
                ShowDialog();
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }

    public enum DiskFullOption
    {
        DeleteFile,
        KeepFile
    }
}
