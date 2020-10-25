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
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public TransferErrorOption Option;
        public ErrorWindow()
        {
            InitializeComponent();
        }

        private void skipAll_button_Click(object sender, RoutedEventArgs e)
        {
            Option = TransferErrorOption.SkipAll;
            Hide();
        }
        private void skip_button_Click(object sender, RoutedEventArgs e)
        {
            Option = TransferErrorOption.SkipCurrentFile;
            Hide();
        }
        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            Option = TransferErrorOption.Cancel;
            Hide();
        }

        private void try_bt_Click(object sender, RoutedEventArgs e)
        {
            Option = TransferErrorOption.Try;
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

      
    }
}
