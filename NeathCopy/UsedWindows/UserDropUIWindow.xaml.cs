using Alphaleonis.Win32.Filesystem;
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
    /// Interaction logic for UserDropUIWindow.xaml
    /// </summary>
    public partial class UserDropUIWindow : Window
    {
        System.Windows.Forms.FolderBrowserDialog fb = new System.Windows.Forms.FolderBrowserDialog();

        public ComboBox OptionComboBox { get; private set; }
        public TextBox DestinyTextBox { get; private set; }
        public bool DlgResult { get; set; }

        public string Operation { get; protected set; }
        public string Destiny { get; protected set; }

        public UserDropUIWindow()
        {
            InitializeComponent();

            OptionComboBox = option_cb;
            DestinyTextBox = tb1;
        }

        private void ok_bt_Click(object sender, RoutedEventArgs e)
        {
            switch (option_cb.SelectedIndex)
            {
                case 0:
                    Operation = "copy";
                    break;
                case 1:
                    Operation = "move";
                    break;
            }

            if (Directory.Exists(tb1.Text))
            {
                Destiny = tb1.Text;
                DlgResult = true;
                Hide();
            }
            else
            {
                MessageBox.Show("The specific Destiny is not valid");
            }

        }
        private void cancel_bt_Click(object sender, RoutedEventArgs e)
        {
            DlgResult = false;
            Hide();
        }
        private void browse_bt_Click(object sender, RoutedEventArgs e)
        {
            if (fb.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Destiny = fb.SelectedPath;
                tb1.Text = fb.SelectedPath;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

    }
}
