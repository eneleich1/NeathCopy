using NeathCopy.Module2_Configuration;
using NeathCopy.Services;
using NeathCopy.Services.AppControl;
using NeathCopy.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeathCopy
{
    /// <summary>
    /// Interaction logic for ContainerWindow.xaml
    /// </summary>
    public partial class ContainerWindow : Window
    {
        public static ContainerWindow mainWindow;
        private readonly ContainerWindowViewModel viewModel;
        private readonly IAppController controller;

        /// <summary>
        /// Get an Enumerable of VisualCopy contained in this instance.
        /// </summary>
        public IEnumerable<VisualCopy> VisualsCopys => viewModel.GetVisualsCopys();

        public ContainerWindow()
            : this(StartupClass.Controller)
        {
        }

        internal ContainerWindow(IAppController controller)
        {
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
            InitializeComponent();

            viewModel = new ContainerWindowViewModel(Dispatcher, CloseIfEmpty, HideToTrayIfResident, () => false);
            DataContext = viewModel;
            Closing += ContainerWindow_Closing;
            Closed += ContainerWindow_Closed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindow = this;
            controller.RegisterContainer(this);
        }

        private void ContainerWindow_Closed(object sender, EventArgs e)
        {
            controller.UnregisterContainer(this);
        }

        /// <summary>
        /// Add a new VisualCopy to this instance. Do not start it.
        /// </summary>
        public VisualCopy AddNew()
        {
            return viewModel.AddNew();
        }

        internal void BringToFront()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(BringToFront);
                return;
            }

            if (Visibility != Visibility.Visible)
                Show();

            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        }

        /// <summary>
        /// Remove the specific visualCopy from this container.
        /// </summary>
        public void Remove(VisualCopy vc)
        {
            viewModel.Remove(vc, CloseIfEmpty);
        }

        private void ContainerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // In resident mode, the close button should NOT keep copy handlers alive.
            // It must terminate all VisualCopy instances and then hide the container.
            if (IsResidentNow())
            {
                e.Cancel = true;

                try
                {
                    // Cancel + remove every VisualCopy so the container becomes empty.
                    CancelAll();
                }
                catch
                {
                    // Intentionally ignore here; we still want to hide.
                }

                Hide();
                return;
            }

            // In legacy mode, allow window to close normally.
            // Optional: if you also want to force-cancel active copies on legacy close,
            // uncomment the following:
            // CancelAll();
        }

        private void CloseIfEmpty()
        {
            if (!viewModel.VisualsCopys.Any())
            {
                if (IsResidentNow())
                    Hide();
                else
                    Close();
            }
        }

        private void HideToTrayIfResident()
        {
            if (!IsResidentNow())
                return;

            controller.RequestHideToTray("Container requested hide");
        }

        private bool IsResidentNow()
        {
            return IntegrationManager.IsResident(Configuration.Main);
        }

        /// <summary>
        /// Cancel all VisualsCopy in this instance.
        /// </summary>
        public void CancelAll()
        {
            viewModel.CancelAll(CloseIfEmpty);
        }

        /// <summary>
        /// Pause all VisualsCopy in this instance.
        /// </summary>
        public void PauseAll()
        {
            viewModel.PauseAll();
        }

        /// <summary>
        /// Resume all VisualsCopy in this instance.
        /// </summary>
        public void ResumeAll()
        {
            viewModel.ResumeAll();
        }
    }
}
