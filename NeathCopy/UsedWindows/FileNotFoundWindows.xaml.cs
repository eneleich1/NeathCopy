using System;
using System.Windows;
using System.Windows.Documents;
using NeathCopy.ViewModels;
using NeathCopyEngine.CopyHandlers;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for FileNotFoundWindows.xaml
    /// </summary>
    public partial class FileNotFoundWindows : Window
    {
        private readonly FileNotFoundWindowViewModel viewModel;

        public FileNotFoundOption Option => viewModel.Option;

        public FileNotFoundWindows()
        {
            InitializeComponent();

            viewModel = new FileNotFoundWindowViewModel();
            viewModel.RequestClose += () => Hide();
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
            Hide();
        }
    }
}
