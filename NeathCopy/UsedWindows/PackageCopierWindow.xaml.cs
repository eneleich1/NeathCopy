using Microsoft.Win32;
using NeathCopy.Module2_Configuration;
using NeathCopy.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for PackageCopierWindow.xaml
    /// </summary>
    public partial class PackageCopierWindow : Window
    {
        private readonly PackageCopierWindowViewModel viewModel;

        public PackageCopierWindow()
        {
            InitializeComponent();
            viewModel = new PackageCopierWindowViewModel();
            viewModel.StartRequested += ViewModel_StartRequested;
            DataContext = viewModel;
        }

        private void ViewModel_StartRequested(object sender, PackageCopierStartEventArgs e)
        {
            if (e == null || e.RequestInfo == null)
                return;

            try
            {
                var visualCopy = Configuration.Main.AddNewVisualCopy();
                Configuration.Main.SetRunningState(visualCopy, e.RequestInfo);
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Package Copier", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSourceFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
                viewModel.AddSources(dialog.FileNames);
        }

        private void AddSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    viewModel.AddSources(new[] { dialog.SelectedPath });
            }
        }

        private void AddDestination_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    viewModel.AddDestinations(new[] { dialog.SelectedPath });
            }
        }

        private void RemoveSources_Click(object sender, RoutedEventArgs e)
        {
            var selected = new List<SourceEntry>();
            foreach (var item in sourcesListBox.SelectedItems)
            {
                if (item is SourceEntry source)
                    selected.Add(source);
            }

            viewModel.RemoveSources(selected);
        }

        private void ClearSources_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ClearSources();
        }

        private void RemoveDestinations_Click(object sender, RoutedEventArgs e)
        {
            var selected = new List<DestinationEntry>();
            foreach (var item in destinationsListBox.SelectedItems)
            {
                if (item is DestinationEntry destination)
                    selected.Add(destination);
            }

            viewModel.RemoveDestinations(selected);
        }

        private void ClearDestinations_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ClearDestinations();
        }

        private void SourcesListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void SourcesListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var entries = (string[])e.Data.GetData(DataFormats.FileDrop);
            viewModel.AddSources(entries);
        }

        private void DestinationsListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void DestinationsListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var entries = (string[])e.Data.GetData(DataFormats.FileDrop);
            var folders = entries.Where(Directory.Exists);
            viewModel.AddDestinations(folders);
        }

        private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                textBox.ScrollToEnd();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            viewModel.StartRequested -= ViewModel_StartRequested;
            viewModel.CancelCopy();
        }
    }
}
