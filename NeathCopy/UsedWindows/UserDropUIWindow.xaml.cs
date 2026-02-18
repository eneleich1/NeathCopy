using Alphaleonis.Win32.Filesystem;
using System.Windows;
using NeathCopy.ViewModels;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for UserDropUIWindow.xaml
    /// </summary>
    public partial class UserDropUIWindow : Window
    {
        private readonly System.Windows.Forms.FolderBrowserDialog fb;
        private readonly UserDropUIWindowViewModel viewModel;

        public bool DlgResult => viewModel.DlgResult;
        public string Operation => viewModel.Operation;
        public string Destiny => viewModel.Destiny;

        public UserDropUIWindow()
        {
            InitializeComponent();

            fb = new System.Windows.Forms.FolderBrowserDialog();

            viewModel = new UserDropUIWindowViewModel();
            viewModel.RequestClose += () => Hide();
            viewModel.RequestBrowse += BrowseForFolder;
            DataContext = viewModel;
        }

        public void SetTitle(string title)
        {
            Title = title;
        }

        public void SetDestiny(string destiny)
        {
            viewModel.Destiny = destiny;
        }

        public void SetOperation(string operation)
        {
            viewModel.SelectedOperationIndex = operation == "move" ? 1 : 0;
        }

        private void BrowseForFolder()
        {
            if (fb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                viewModel.Destiny = fb.SelectedPath;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
