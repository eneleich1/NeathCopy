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
    /// Interaction logic for FileNotFoundWindows.xaml
    /// </summary>
    public partial class FileNotFoundWindows : Window
    {
        public FileNotFoundOption Option;
        public FileNotFoundWindows()
        {
            InitializeComponent();
        }

        private void try_button_Click(object sender, RoutedEventArgs e)
        {
            Option = FileNotFoundOption.Skip;
            Hide();
        }

        private void skip_button_Click(object sender, RoutedEventArgs e)
        {
            Option = FileNotFoundOption.Skip;
            Hide();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            Option = FileNotFoundOption.Cancel;
            Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
