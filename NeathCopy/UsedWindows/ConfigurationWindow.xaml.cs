using System.Windows;
using NeathCopy.ViewModels;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private readonly ConfigurationWindowViewModel viewModel;

        public ConfigurationWindow()
        {
            InitializeComponent();

            viewModel = new ConfigurationWindowViewModel();
            DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.LoadFromConfiguration();
        }

        private void configWnd_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
