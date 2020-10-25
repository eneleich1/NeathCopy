using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for InformationWindow.xaml
    /// </summary>
    public partial class InformationWindow : Window
    {
        public string SoundPath { get; set; }
        public UserAction UserSelectedAction = UserAction.None;
        public enum UserAction { Try, Ignore, Cancel, None, Fit }

        public InformationWindow(string soundPath)
        {
            SoundPath = soundPath;

            InitializeComponent();
        }

        private void try_button_Click(object sender, RoutedEventArgs e)
        {
            UserSelectedAction = UserAction.Try;
            Hide();
        }
        private void ignore_button_Click(object sender, RoutedEventArgs e)
        {
            UserSelectedAction = UserAction.Ignore;
            Hide();
        }
        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            UserSelectedAction = UserAction.Cancel;
            Hide();
        }
        private void fit_button_Click(object sender, RoutedEventArgs e)
        {
            UserSelectedAction = UserAction.Fit;
            Hide();
        }
        public void ShowMessage(DriveInfo driveInfo, long freeSpace, long requireSpace)
        {
            //message_tb.Text = message;
            message_rb.Dispatcher.Invoke(new Action(() =>
            {
                var message = string.Format("There is not enough free space in {0} volumen. Try delete something firsth.", driveInfo.VolumeLabel);

                // Create a simple FlowDocument to serve as content.
                FlowDocument flowDoc = new FlowDocument(new Paragraph(new Run(message)));

                // This call sets the contents of the RichTextBox to the specified FlowDocument.
                message_rb.Document = flowDoc;

            }));

            diskSpace_listview.Dispatcher.Invoke(new Action(() =>
            {
                diskSpace_listview.Items.Clear();
                diskSpace_listview.Items.Add(new FormattedDriveInfo(driveInfo, freeSpace, requireSpace));
            }));

            //ShowDialog();
            Dispatcher.Invoke(new Action(() => { ShowDialog(); }), null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }

    public class FormattedDriveInfo
    {
        public string Volumen { get; set; }
        public MySize Capacity { get; set; }
        public MySize UsedSpace { get; set; }
        public MySize FreeSpace { get; set; }
        public MySize RequireSpace { get; set; }
        public MySize NeedMore { get; set; }
        public DriveType VolumenType { get; set; }

        public FormattedDriveInfo(DriveInfo info, long freeSpace, long requireSpace)
        {
            Volumen = string.Format("[{2}]: {0} ({1})", info.VolumeLabel, info.Name.Substring(0, info.Name.Length - 1), info.DriveFormat);
            VolumenType = info.DriveType;
            Capacity = new MySize(info.TotalSize);
            UsedSpace = new MySize(info.TotalSize - freeSpace);
            FreeSpace = new MySize(freeSpace);
            RequireSpace = new MySize(requireSpace);
            NeedMore = new MySize(requireSpace - freeSpace);
        }
    }
}
