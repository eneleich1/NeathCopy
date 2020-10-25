using NeathCopyEngine.DataTools;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for CopyListWindow.xaml
    /// </summary>
    public partial class CopyListWindow : Window
    {
        System.Windows.Forms.OpenFileDialog ofd;
        System.Windows.Forms.SaveFileDialog sfd;
        public VisualCopy VisualCopy { get; set; }
        public FilesList CurrentFilesList { get; set; }
        public CopyListWindow(VisualCopy vc)
        {
            InitializeComponent();

            VisualCopy = vc;

            CurrentFilesList = new FilesList();

            ofd = new System.Windows.Forms.OpenFileDialog();
            sfd = new System.Windows.Forms.SaveFileDialog();
        }

        public CopyListWindow() { }

        IEnumerable<FileDataInfo> SelectedFiles
        {
            get
            {
                List<FileDataInfo> res = new List<FileDataInfo>();

                foreach (var item in MainListView.SelectedItems)
                    res.Add((FileDataInfo)item);

                return res;
            }
        }

        private void load_menuItem_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }
        private void save_menuItem_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
        private void loadOneDestiny_menuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadOneDestiny();
        }
        private void saveOneDestiny_menuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveOneDestiny();
        }
        private void loadCompressed_menuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadCompressed();   
        }
        private void saveCompressed_menuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveCompressed();
        }

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
            CurrentFilesList = filesList;

            var filesVS = ((CollectionViewSource)FindResource("filesVS"));
            filesVS.Source = CurrentFilesList.Files;
        }
        public void DisplayList()
        {
            filesCount_tb.Text = CurrentFilesList.Count.ToString();
            Show();
        }
        public void MoveFirst(object sender, RoutedEventArgs e)
        {
            //var selecteds = SelectedFiles;

            //MainListView.ItemsSource = null;

            //CurrentFilesList.MoveToBegining(selecteds);

            //MainListView.ItemsSource = CurrentFilesList.Files;

            var selecteds = SelectedFiles;
            CurrentFilesList.MoveToBegining(selecteds);
            MainListView.Items.Refresh();

        }
        public void Remove(object sender, RoutedEventArgs e)
        {
            var selecteds = SelectedFiles;
            MainListView.ItemsSource = null;

            var action = new Action(() =>
              {
                  CurrentFilesList.Remove(selecteds);
                  MainListView.ItemsSource = CurrentFilesList.Files;
                  filesCount_tb.Text = CurrentFilesList.Count.ToString();
              });

            if (VisualCopy.State == VisualCopy.VisualCopyState.Runing)
            {
                //VisualCopy.Pause();

                action.Invoke();

                //VisualCopy.Resume();
            }
            else action.Invoke();

        }
        public void MoveUp(object sender, RoutedEventArgs e)
        {
            //var selecteds = SelectedFiles;

            //MainListView.ItemsSource = null;

            //CurrentFilesList.MoveUp(selecteds);
            
            //MainListView.ItemsSource = CurrentFilesList.Files;

            var selecteds = SelectedFiles;
            CurrentFilesList.MoveUp(selecteds);
            MainListView.Items.Refresh();
        }
        public void MoveDown(object sender, RoutedEventArgs e)
        {
            //var selecteds = SelectedFiles;
            //MainListView.ItemsSource = null;
            //CurrentFilesList.MoveDown(selecteds);
            //MainListView.ItemsSource = CurrentFilesList.Files;

            var selecteds = SelectedFiles;
            CurrentFilesList.MoveDown(selecteds);
            MainListView.Items.Refresh();
        }
        public void MoveLast(object sender, RoutedEventArgs e)
        {
            //var selecteds = SelectedFiles;
            //MainListView.ItemsSource = null;
            //CurrentFilesList.MoveToEnd(selecteds);
            //MainListView.ItemsSource = CurrentFilesList.Files;

            var selecteds = SelectedFiles;
            CurrentFilesList.MoveToEnd(selecteds);
            MainListView.Items.Refresh();
        }


        public void Load()
        {
            try
            {
                if (CurrentFilesList != null)
                {
                    ofd.Filter = "ncl Files (*.ncl)|*.ncl";
                    var dlgResult = ofd.ShowDialog();

                    if (dlgResult == System.Windows.Forms.DialogResult.OK)
                    {
                        if (VisualCopy.State == VisualCopy.VisualCopyState.Runing)
                        {
                            VisualCopy.Pause();

                            VisualCopy.HandleLoadList(FilesList.Load(ofd.FileName));

                            VisualCopy.Resume();
                        }
                        else
                            VisualCopy.HandleLoadList(FilesList.Load(ofd.FileName));

                        Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An Error has ocurred loading a list: {0}", ex.Message));
            }
        }
        public void Save()
        {
            try
            {
                if (CurrentFilesList != null)
                {
                    sfd.Filter = "ncl Files (*.ncl)|*.ncl";
                    var dlgResult = sfd.ShowDialog();

                    if (dlgResult == System.Windows.Forms.DialogResult.OK)
                    {
                        if (VisualCopy.State == VisualCopy.VisualCopyState.Runing)
                        {
                            //VisualCopy.Pause();

                            CurrentFilesList.SaveList(sfd.FileName);

                            //VisualCopy.Resume();
                        }
                        else CurrentFilesList.SaveList(sfd.FileName);

                        RaiseListSaved();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void LoadOneDestiny()
        {
            try
            {
                if (CurrentFilesList != null)
                {
                    ofd.Filter = "odl Files (*.odl)|*.odl";//Extension = One Destiny List
                    var dlgResult = ofd.ShowDialog();

                    if (dlgResult == System.Windows.Forms.DialogResult.OK)
                    {
                        if (VisualCopy.State == VisualCopy.VisualCopyState.Runing)
                        {
                            //VisualCopy.Pause();

                            VisualCopy.HandleLoadList(FilesList.LoadListOneDestiny(ofd.FileName));

                            //VisualCopy.Resume();
                        }
                        else
                            VisualCopy.HandleLoadList(FilesList.LoadListOneDestiny(ofd.FileName));

                        Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An Error has ocurred loading a list: {0}", ex.Message));
            }
        }
        public void SaveOneDestiny()
        {
            try
            {
                if (CurrentFilesList != null)
                {
                    sfd.Filter = "odl Files (*.odl)|*.odl";
                    var dlgResult = sfd.ShowDialog();

                    if (dlgResult == System.Windows.Forms.DialogResult.OK)
                    {
                        if (VisualCopy.State == VisualCopy.VisualCopyState.Runing)
                        {
                            //VisualCopy.Pause();

                            CurrentFilesList.SaveListOneDestiny(sfd.FileName);

                            //VisualCopy.Resume();
                        }
                        else CurrentFilesList.SaveListOneDestiny(sfd.FileName);

                        RaiseListSaved();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void LoadCompressed()
        {
            try
            {
                if (CurrentFilesList != null)
                {
                    ofd.Filter = "cnl Files (*.cnl)|*.cnl";
                    var dlgResult = ofd.ShowDialog();

                    if (dlgResult == System.Windows.Forms.DialogResult.OK)
                    {
                        if (VisualCopy.State == VisualCopy.VisualCopyState.Runing)
                        {
                            //VisualCopy.Pause();

                            VisualCopy.HandleLoadList(FilesList.LoadCompressed(ofd.FileName));

                            //VisualCopy.Resume();
                        }
                        else
                            VisualCopy.HandleLoadList(FilesList.LoadCompressed(ofd.FileName));

                        Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An Error has ocurred loading a list: {0}", ex.Message));
            }
        }
        public void SaveCompressed()
        {
            try
            {
                if (CurrentFilesList != null)
                {
                    sfd.Filter = "cnl Files (*.cnl)|*.cnl";
                    var dlgResult = sfd.ShowDialog();

                    if (dlgResult == System.Windows.Forms.DialogResult.OK)
                    {
                        if (VisualCopy.State == VisualCopy.VisualCopyState.Runing)
                        {
                            //VisualCopy.Pause();

                            CurrentFilesList.SaveCompressedList(sfd.FileName);

                            //VisualCopy.Resume();
                        }
                        else CurrentFilesList.SaveCompressedList(sfd.FileName);

                        RaiseListSaved();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void Window_Drop(object sender, DragEventArgs e)
        {
            //[DI]VisualCopy.displayInfo.CopyListWndFilesCount_tb = filesCount_tb;
            VisualCopy.HandleDrop(new List<string>((string[])e.Data.GetData("FileDrop")));

            Hide();
            
        }

        private void MoveFirst(object sender, ExecutedRoutedEventArgs e)
        {

        }
    }

}
