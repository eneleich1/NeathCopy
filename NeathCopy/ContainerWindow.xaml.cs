using NeathCopy.Module2_Configuration;
using NeathCopy.Themes;
using NeathCopy.UsedWindows;
using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.Helpers;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;

namespace NeathCopy
{
    /// <summary>
    /// Interaction logic for ContainerWindow.xaml
    /// </summary>
    public partial class ContainerWindow : Window
    {
        public static ContainerWindow mainWindow;
        /// <summary>
        /// Get an Enumerable of VisualCopy contained in this instance.
        /// </summary>
        public IEnumerable<VisualCopy> VisualsCopys
        {
            get
            {
                foreach (VisualCopy vc in listbox1.Items)
                {
                    yield return vc;
                }
            }
        }
     

        //Use to prevent multiple access
        private static Mutex mut = new Mutex();

        public ContainerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindow = this;
        }

        /// <summary>
        /// Add a new VisualCopy to this instance. Do not start it.
        /// </summary>
        /// <returns></returns>
        public VisualCopy AddNew()
        {
            try
            {
                var vc = new VisualCopy();

                vc.Id = VisualsCopysHandler.VisualsCopys.Count() + 1;

                //VisualCopy eventsv
                vc.BreakInqueve += vc_BreakInqueve;
                vc.AfterCancel += vc_AfterCancel;
                vc.Finish += vc_Finish;

                ManipulateListBox(vc, ListManipulation.Add);

                return vc;
            }
            catch(Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindow", "AddNew"));
                return null;
            }
        }

        public enum ListManipulation { Add,Remove}
        private void ManipulateListBox(VisualCopy vc, ListManipulation listManipulation)
        {
            try
            {
                // Wait until it is safe to enter.
                mut.WaitOne();

                if (listManipulation == ListManipulation.Add)
                {
                    listbox1.Dispatcher.Invoke(() => { listbox1.Items.Add(vc); });
                }
                else if (listManipulation == ListManipulation.Remove)
                {
                    listbox1.Dispatcher.Invoke(() => { listbox1.Items.Remove(vc); });
                }

                // Release the Mutex.
                mut.ReleaseMutex();

            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindow", "ManipulateListBox"));
            }
        }

        /// <summary>
        /// Remove the specific visualCopy from ths listbox container.
        /// </summary>
        /// <param name="sender"></param>
        public void Remove(VisualCopy vc)
        {
            try
            {
                ManipulateListBox(vc, ListManipulation.Remove);

                if (listbox1.Items.Count == 0)
                    listbox1.Dispatcher.Invoke(new Action(Close), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindow", "Remove"));
            }
        }

        /// <summary>
        /// Cancell all VisualsCopy in this instance.
        /// </summary>
        /// <param name="visualCopy"></param>
        public void CancelAll()
        {
            try
            {
                foreach (var vc in VisualsCopys.ToList())
                {
                    vc.Cancel("User Cancell All Copys");
                    Remove(vc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindow", "CancelAll"));
            }
        }
        /// <summary>
        /// Paused all VisualsCopy in this instance.
        /// </summary>
        /// <param name="visualCopy"></param>
        public void PauseAll()
        {
            try
            {
                foreach (var vc in VisualsCopys)
                    vc.Pause();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindow", "PauseAll"));
            }
        }
        /// <summary>
        /// Resume all VisualsCopy in this instance.
        /// </summary>
        /// <param name="visualCopy"></param>
        public void ResumeAll()
        {
            try
            {
                foreach (var vc in VisualsCopys)
                    vc.Resume();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindow", "ResumeAll"));
            }
        }

        #region Event Handlers

        void vc_BreakInqueve(VisualCopy sender)
        {
            try
            {
                //Dont ask for if(InQueve) becose this ask was made in VisualCopy class
                //before RaiseBreakInQuevve was called.
                Configuration.Main.RemoveFromQueve(sender, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindow", "vc_BreakInqueve"));
            }
        }
        void vc_Finish(VisualCopy sender)
        {
            try
            {
                //Try to play sound
                Configuration.Main.PLaySoundAfterOperation(Configuration.Main.PlaySound_After_Finish
                    , Configuration.Main.FinishOperation_Sound);

                //Remove from listBox
                Remove(sender);

                //Remove from VisualCopy queve if below.
                Configuration.Main.RemoveFromQueve(sender, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindow", "vc_Finish"));
            }

        }
        void vc_AfterCancel(VisualCopy sender)
        {
            try
            {
                //Try to play sound
                Configuration.Main.PLaySoundAfterOperation(Configuration.Main.PlaySound_After_Cancel
                    , Configuration.Main.Cancell_Sound);

                //Remove from listBox
                Remove(sender);

                //Remove from VisualCopy queve if below.
                Configuration.Main.RemoveFromQueve(sender, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "ContainerWindow", "vc_AfterCancel"));
            }

        }

        private void pausedAll_menuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                PauseAll();
            });
        }
        private void resumeAll_menuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                ResumeAll();
            });
        }
        private void cancelAll_menuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                CancelAll();
            });
        }

        #endregion

    }
}
