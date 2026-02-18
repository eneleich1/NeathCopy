using NeathCopy.ViewModels;
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

        /// <summary>
        /// Get an Enumerable of VisualCopy contained in this instance.
        /// </summary>
        public IEnumerable<VisualCopy> VisualsCopys => viewModel.GetVisualsCopys();

        public ContainerWindow()
        {
            InitializeComponent();

            viewModel = new ContainerWindowViewModel(Dispatcher, CloseIfEmpty);
            DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindow = this;
        }

        /// <summary>
        /// Add a new VisualCopy to this instance. Do not start it.
        /// </summary>
        public VisualCopy AddNew()
        {
            return viewModel.AddNew();
        }

        /// <summary>
        /// Remove the specific visualCopy from this container.
        /// </summary>
        public void Remove(VisualCopy vc)
        {
            viewModel.Remove(vc, CloseIfEmpty);
        }

        private void CloseIfEmpty()
        {
            if (!viewModel.VisualsCopys.Any())
                Close();
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
