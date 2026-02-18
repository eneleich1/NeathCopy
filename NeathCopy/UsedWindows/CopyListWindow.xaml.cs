using NeathCopy.ViewModels;
using NeathCopyEngine.DataTools;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for CopyListWindow.xaml
    /// </summary>
    public partial class CopyListWindow : Window
    {
        private readonly CopyListWindowViewModel viewModel;

        public CopyListWindow(VisualCopy vc)
        {
            InitializeComponent();

            viewModel = new CopyListWindowViewModel(vc);
            viewModel.ListSaved += RaiseListSaved;
            viewModel.RequestRefresh += () => MainListView.Items.Refresh();
            viewModel.RequestHide += () => Hide();
            DataContext = viewModel;
        }

        public CopyListWindow()
        {
            InitializeComponent();
        }

        public FilesList CurrentFilesList => viewModel?.CurrentFilesList;

        #region Events

        public delegate void ListLoadedEventHandle(FilesList list);
        /// <summary>
        /// Occurs when a files'list have been loaded.
        /// </summary>
        public event ListLoadedEventHandle ListLoaded;
        public void RaiseListLoaded(FilesList list)
        {
            if (ListLoaded != null)
                ListLoaded(list);
        }

        public delegate void ListSavedEventHandle();
        /// <summary>
        /// Occurs when a files'list have saved sussefully.
        /// </summary>
        public event ListSavedEventHandle ListSaved;
        protected void RaiseListSaved()
        {
            if (ListSaved != null)
                ListSaved();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        #endregion

        public void SetCurrentList(FilesList filesList)
        {
            viewModel?.SetCurrentList(filesList);
        }

        public void DisplayList()
        {
            viewModel?.UpdateFilesCount();
            Show();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (viewModel == null)
                return;

            var paths = new List<string>((string[])e.Data.GetData("FileDrop"));
            viewModel.HandleDrop(paths);
            Hide();
        }

        private void MainListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel == null)
                return;

            viewModel.SelectedFiles = MainListView.SelectedItems.Cast<FileDataInfo>().ToList();
        }
    }
}
