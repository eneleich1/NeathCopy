using System.Windows;
using NeathCopy.ViewModels;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        private readonly MessageWindowViewModel viewModel;

        public MessageWindow()
        {
            InitializeComponent();

            viewModel = new MessageWindowViewModel();
            viewModel.RequestClose += () => Hide();
            DataContext = viewModel;
        }

        public void SetMessage(string message)
        {
            viewModel.Message = message;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
