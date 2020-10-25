using NeathCopyEngine.CopyHandlers;
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
    /// Interaction logic for FileExistOptionsWindow.xaml
    /// </summary>
    public partial class FileExistOptionsWindow : Window
    {
        public FileCopyOptions Option { get; set; }
        public FileExistOptionsWindow()
        {
            InitializeComponent();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Option = FileCopyOptions.Cancel;

                HideOnExternalThread();
            }
            catch(Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "FileExistWindow", "cancel_button_Click"));
            }
        }

        private void ok_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (option_cb.SelectedIndex)
                {
                    case 0:
                        Option = FileCopyOptions.OverwriteIfFileExist;
                        break;
                    case 1:
                        Option = FileCopyOptions.AllwaysOverride;
                        break;
                    case 2:
                        Option = FileCopyOptions.OverrideDifferent;
                        break;
                    case 3:
                        Option = FileCopyOptions.AllwaysOverrideDifferent;
                        break;
                    case 4:
                        Option = FileCopyOptions.SkipIfFileExist;
                        break;
                    case 5:
                        Option = FileCopyOptions.AllwaysSkip;
                        break;
                }

                HideOnExternalThread();

            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "FileExistWindow", "cancel_button_Click"));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            HideOnExternalThread();
        }

        void HideOnExternalThread()
        {
            Dispatcher.Invoke(() => { Hide(); });
        }
    }
}
