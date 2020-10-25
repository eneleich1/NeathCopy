using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeathCopy.Tools
{
    public class MyCommands
    {
        #region VisualCopy Buttons Commands

        public static RoutedCommand PauseCommand = new RoutedCommand();
        public static RoutedCommand SkipCommand = new RoutedCommand();
        public static RoutedCommand CancelCommand = new RoutedCommand();
        public static RoutedCommand MoreCommand = new RoutedCommand();

        #endregion

        #region List Windows Commands

        public static RoutedCommand MoveFirstCommand = new RoutedCommand();
        public static RoutedCommand MoveUpCommand = new RoutedCommand();
        public static RoutedCommand MoveDownCommand = new RoutedCommand();
        public static RoutedCommand MoveLastCommand = new RoutedCommand();
        public static RoutedCommand RemoveCommand = new RoutedCommand();
        public static RoutedCommand SaveCommand = new RoutedCommand();
        public static RoutedCommand LoadCommand = new RoutedCommand();

        #endregion

    }
}
