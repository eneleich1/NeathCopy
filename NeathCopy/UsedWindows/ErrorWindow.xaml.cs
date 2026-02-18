using System;
using System.Windows;
using System.Windows.Documents;
using NeathCopy.ViewModels;
using NeathCopyEngine.CopyHandlers;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        private readonly ErrorWindowViewModel viewModel;

        public TransferErrorOption Option => viewModel.Option;

        public ErrorWindow()
        {
            InitializeComponent();

            viewModel = new ErrorWindowViewModel();
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
