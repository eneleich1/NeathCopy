using System;
using System.Windows;
using System.Windows.Documents;
using NeathCopy.ViewModels;
using NeathCopyEngine.CopyHandlers;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for FileExistOptionsWindow.xaml
    /// </summary>
    public partial class FileExistOptionsWindow : Window
    {
        private readonly FileExistOptionsWindowViewModel viewModel;

        public FileCopyOptions Option => viewModel.Option;

        public FileExistOptionsWindow()
        {
            InitializeComponent();

            viewModel = new FileExistOptionsWindowViewModel();
            viewModel.RequestClose += () => HideOnExternalThread();
            DataContext = viewModel;
        }

        public void SetMessage(string message)
        {
            var flowDoc = new FlowDocument(new Paragraph(new Run(message)));
            viewModel.InfoDocument = flowDoc;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            HideOnExternalThread();
        }

        void HideOnExternalThread()
        {
            Dispatcher.Invoke(() => { Hide(); });
        }
    }
}
