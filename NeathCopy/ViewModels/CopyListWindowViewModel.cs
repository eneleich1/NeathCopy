using NeathCopyEngine.DataTools;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;

namespace NeathCopy.ViewModels
{
    public class CopyListWindowViewModel : ViewModelBase
    {
        private readonly VisualCopy visualCopy;
        private FilesList currentFilesList;
        private ObservableCollection<FileDataInfo> files;
        private string filesCount;
        private IEnumerable<FileDataInfo> selectedFiles;

        public ObservableCollection<FileDataInfo> Files
        {
            get => files;
            private set => SetProperty(ref files, value);
        }

        public string FilesCount
        {
            get => filesCount;
            private set => SetProperty(ref filesCount, value);
        }

        public IEnumerable<FileDataInfo> SelectedFiles
        {
            get => selectedFiles;
            set => SetProperty(ref selectedFiles, value);
        }

        public ICommand MoveFirstCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand MoveLastCommand { get; }
        public ICommand RemoveCommand { get; }

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadOneDestinyCommand { get; }
        public ICommand SaveOneDestinyCommand { get; }
        public ICommand LoadCompressedCommand { get; }
        public ICommand SaveCompressedCommand { get; }

        public event Action ListSaved;
        public event Action RequestRefresh;
        public event Action RequestHide;

        public CopyListWindowViewModel(VisualCopy visualCopy)
        {
            this.visualCopy = visualCopy;
            currentFilesList = new FilesList();
            Files = currentFilesList.Files;
            FilesCount = "0";

            MoveFirstCommand = new RelayCommand(MoveFirst);
            MoveUpCommand = new RelayCommand(MoveUp);
            MoveDownCommand = new RelayCommand(MoveDown);
            MoveLastCommand = new RelayCommand(MoveLast);
            RemoveCommand = new RelayCommand(Remove);

            LoadCommand = new RelayCommand(Load);
            SaveCommand = new RelayCommand(Save);
            LoadOneDestinyCommand = new RelayCommand(LoadOneDestiny);
            SaveOneDestinyCommand = new RelayCommand(SaveOneDestiny);
            LoadCompressedCommand = new RelayCommand(LoadCompressed);
            SaveCompressedCommand = new RelayCommand(SaveCompressed);
        }

        public FilesList CurrentFilesList => currentFilesList;

        public void SetCurrentList(FilesList filesList)
        {
            if (currentFilesList != null)
                currentFilesList.PropertyChanged -= CurrentFilesList_PropertyChanged;

            currentFilesList = filesList ?? new FilesList();
            Files = currentFilesList.Files;
            UpdateFilesCount();

            currentFilesList.PropertyChanged += CurrentFilesList_PropertyChanged;
        }

        public void UpdateFilesCount()
        {
            FilesCount = currentFilesList.Count.ToString();
        }

        private void CurrentFilesList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Count")
                UpdateFilesCount();
        }

        public void HandleDrop(List<string> paths)
        {
            if (paths == null || paths.Count == 0)
                return;

            visualCopy.HandleDrop(paths);
        }

        private IEnumerable<FileDataInfo> GetSelectedFiles()
        {
            return SelectedFiles ?? Enumerable.Empty<FileDataInfo>();
        }

        private void MoveFirst()
        {
            var selecteds = GetSelectedFiles();
            currentFilesList.MoveToBegining(selecteds);
            RequestRefresh?.Invoke();
        }

        private void MoveUp()
        {
            var selecteds = GetSelectedFiles();
            currentFilesList.MoveUp(selecteds);
            RequestRefresh?.Invoke();
        }

        private void MoveDown()
        {
            var selecteds = GetSelectedFiles();
            currentFilesList.MoveDown(selecteds);
            RequestRefresh?.Invoke();
        }

        private void MoveLast()
        {
            var selecteds = GetSelectedFiles();
            currentFilesList.MoveToEnd(selecteds);
            RequestRefresh?.Invoke();
        }

        private void Remove()
        {
            var selecteds = GetSelectedFiles();

            var action = new Action(() =>
            {
                currentFilesList.Remove(selecteds);
                UpdateFilesCount();
            });

            if (visualCopy.State == VisualCopy.VisualCopyState.Runing)
                action.Invoke();
            else
                action.Invoke();
        }

        private void Load()
        {
            if (currentFilesList == null)
                return;

            var ofd = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "ncl Files (*.ncl)|*.ncl"
            };

            var dlgResult = ofd.ShowDialog();
            if (dlgResult != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                if (visualCopy.State == VisualCopy.VisualCopyState.Runing)
                    visualCopy.HandleLoadList(FilesList.Load(ofd.FileName));
                else
                    visualCopy.HandleLoadList(FilesList.Load(ofd.FileName));

                RequestHide?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An Error has ocurred loading a list: {0}", ex.Message));
            }
        }

        private void Save()
        {
            if (currentFilesList == null)
                return;

            var sfd = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "ncl Files (*.ncl)|*.ncl"
            };

            var dlgResult = sfd.ShowDialog();
            if (dlgResult != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                currentFilesList.SaveList(sfd.FileName);
                ListSaved?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadOneDestiny()
        {
            if (currentFilesList == null)
                return;

            var ofd = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "odl Files (*.odl)|*.odl"
            };

            var dlgResult = ofd.ShowDialog();
            if (dlgResult != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                visualCopy.HandleLoadList(FilesList.LoadListOneDestiny(ofd.FileName));
                RequestHide?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An Error has ocurred loading a list: {0}", ex.Message));
            }
        }

        private void SaveOneDestiny()
        {
            if (currentFilesList == null)
                return;

            var sfd = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "odl Files (*.odl)|*.odl"
            };

            var dlgResult = sfd.ShowDialog();
            if (dlgResult != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                currentFilesList.SaveListOneDestiny(sfd.FileName);
                ListSaved?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadCompressed()
        {
            if (currentFilesList == null)
                return;

            var ofd = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Compressed Lists (*.cnl;*.ncopylist)|*.cnl;*.ncopylist|cnl Files (*.cnl)|*.cnl|Recovery Lists (*.ncopylist)|*.ncopylist"
            };

            var dlgResult = ofd.ShowDialog();
            if (dlgResult != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                visualCopy.HandleLoadList(FilesList.LoadCompressed(ofd.FileName));
                RequestHide?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An Error has ocurred loading a list: {0}", ex.Message));
            }
        }

        private void SaveCompressed()
        {
            if (currentFilesList == null)
                return;

            var sfd = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "cnl Files (*.cnl)|*.cnl"
            };

            var dlgResult = sfd.ShowDialog();
            if (dlgResult != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                currentFilesList.SaveCompressedList(sfd.FileName);
                ListSaved?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
