using NeathCopy.UsedWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeathCopy.Themes
{
    public class Theme:ResourceDictionary
    {
        public static ConfigurationWindow ConfigWindow { get; private set; }

        static Theme()
        {
            ConfigWindow = new ConfigurationWindow();
        }
        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            if (ContainerWindow.mainWindow != null)
                ContainerWindow.mainWindow.Close();
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event. This event handler is used here to facilitate
        /// dragging of the Window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            try
            {
                // Check if the control have been double clicked.
                if (e.ClickCount == 2)
                {
                    // If double clicked then maximize the window.
                    MaximizeWindow(sender, e);
                }
                else
                {
                    // If not double clicked then just drag the window around.
                    window.DragMove();
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Fires when the user clicks the maximize button on the window's custom title bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            // Check the current state of the window. If the window is currently maximized, return the
            // window to it's normal state when the maximize button is clicked, otherwise maximize the window.
            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
            }
            else
            {
                window.Focus();
                window.WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Fires when the user clicks the Close button on the window's custom title bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CloseWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            if (window.Tag == null || window.Tag.ToString() != "hide")
                window.Close();
            else window.Hide();
        }

        protected void HideWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Hide();
        }

        /// <summary>
        /// Fires when the user clicks the minimize button on the window's custom title bar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Called when the user drags the title bar when maximized.
        /// </summary>
        protected void OnBorderMouseMove(object sender, MouseEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            if (window != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed && window.WindowState == WindowState.Maximized)
                {
                    Size maxSize = new Size(window.ActualWidth, window.ActualHeight);
                    Size resSize = window.RestoreBounds.Size;

                    double curX = e.GetPosition(window).X;
                    double curY = e.GetPosition(window).Y;

                    double newX = curX / maxSize.Width * resSize.Width;
                    double newY = curY;

                    window.WindowState = WindowState.Normal;

                    window.Left = curX - newX;
                    window.Top = curY - newY;
                    window.DragMove();
                }
            }
        }
        protected void settings_button_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow.ShowDialog();

            //new DiskIsFullWindows().ShowDialog();
            //new ErrorWindow().ShowDialog();
            //new FileExistOptionsWindow().ShowDialog();
            //new FileNotFoundWindows().ShowDialog();
            //new InformationWindow("hello").ShowDialog();
            //new MessageWindow().ShowDialog();
            //new UserDropUIWindow().ShowDialog();
        }
    }

}
