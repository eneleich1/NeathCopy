using System;
using System.Collections.Generic;
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
    /// Interaction logic for DiskIsFullWindows.xaml
    /// </summary>
    public partial class DiskIsFullWindows : Window
    {
        public DiskFullOption Option;
        public DiskIsFullWindows()
        {
            InitializeComponent();
        }

        public void ShowMessage(string text)
        {
            message_tb.Dispatcher.Invoke(new Action(() => { message_tb.Text = text; }));
            message_tb.Dispatcher.Invoke(new Action(() => { ShowDialog(); ; }));
        }

        private void delete_bt_Click(object sender, RoutedEventArgs e)
        {
            Option = DiskFullOption.DeleteFile;
            Hide();
        }

        private void keep_bt_Click(object sender, RoutedEventArgs e)
        {
            Option = DiskFullOption.KeepFile;
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }

    public enum DiskFullOption
    {
        DeleteFile, KeepFile
    }
}
