using NeathCopy.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for VideoConversionWindow.xaml
    /// </summary>
    public partial class VideoConversionWindow : Window
    {
        private readonly VideoConversionViewModel viewModel;

        public VideoConversionWindow()
        {
            InitializeComponent();

            viewModel = new VideoConversionViewModel();
            DataContext = viewModel;
        }

        private void FilesListView_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void FilesListView_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            viewModel.AddFiles(files);
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = new List<VideoConversionItemViewModel>();
            foreach (var item in filesListView.SelectedItems)
            {
                if (item is VideoConversionItemViewModel vm)
                    selected.Add(vm);
            }
            viewModel.RemoveItems(selected);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                textBox.ScrollToEnd();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}
