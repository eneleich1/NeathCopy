using System;
using System.Windows;
using NeathCopy.ViewModels;
using NeathCopy.UsedWindows;

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
            Dispatcher.BeginInvoke(new Action(() =>
            {
                NeathCopy.VisualsCopysHandler.MainHandler?.ExecutePendingExitToLegacy();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void VideoConversionMenu_Click(object sender, RoutedEventArgs e)
        {
            var window = new VideoConversionWindow
            {
                Owner = this
            };
            window.Show();
        }

        private void ScriptHooksMenu_Click(object sender, RoutedEventArgs e)
        {
            var window = new ScriptHooksWindow
            {
                Owner = this
            };
            window.ShowDialog();
        }

        private void PackageCopierMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var window = new PackageCopierWindow
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    window.Show();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Package Copier Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
