using NeathCopy.Module1_ShellExt;
using NeathCopy.Module2_Configuration;
using NeathCopy.Services.AppControl;
using NeathCopy.Themes;
using NeathCopy.UsedWindows;
using NeathCopy.Services;
using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NeathCopy.ViewModels;
using System.Threading;
using System.Windows.Threading;

namespace NeathCopy
{
    /// <summary>
    /// Interaction logic for VisualsCopysHandler.xaml
    /// </summary>
    public partial class VisualsCopysHandler : Window
    {
        public static HwndSource hwndCopy;
        private readonly VisualsCopysHandlerViewModel viewModel;
        private bool commonStateInitialized;
        private bool sourceStateInitialized;
        internal IAppController Controller { get; }

        public static VisualsCopysHandler MainHandler { get; internal set; }
        public static ContainerWindow MainContainer { get; set; }
        public static List<ContainerWindow> ContainersList { get; private set; }
        public static IEnumerable<VisualCopy> VisualsCopys
        {
            get
            {
                var containers = ContainersList;
                if (containers == null)
                    yield break;

                foreach (var container in containers)
                {
                    if (container?.VisualsCopys == null)
                        continue;

                    foreach (var vc in container.VisualsCopys)
                    {
                        yield return vc;
                    }
                }
            }
        }

        static VisualsCopysHandler()
        {
            Configuration.Main = Configuration.LoadFromRegister();
            IntegrationManager.UpdateAutoStart(Configuration.Main);
            ThemesManager.Manager.InitResources();
            ThemesManager.Manager.SetThemes(Configuration.Main);
        }
        public VisualsCopysHandler()
            : this(StartupClass.Controller)
        {
        }

        internal VisualsCopysHandler(IAppController controller)
        {
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            InitializeComponent();
            viewModel = new VisualsCopysHandlerViewModel();
            DataContext = viewModel;

            if (Controller is AppController appController)
                appController.AttachMainWindow(this);

            EnsureCommonStateInitialized();
        }

        private void EnsureCommonStateInitialized()
        {
            if (commonStateInitialized)
                return;

            commonStateInitialized = true;
            if (Controller is AppController appController)
                ContainersList = appController.ContainersList;
            else if (ContainersList == null)
                ContainersList = new List<ContainerWindow>();

            MainHandler = this;
        }

        private void InitializeSourceState()
        {
            if (sourceStateInitialized)
                return;

            sourceStateInitialized = true;
            hwndCopy = PresentationSource.FromVisual(this) as HwndSource;
            hwndCopy?.AddHook(WndProc);
        }

        private void MyInitialize()
        {
            try
            {
                EnsureCommonStateInitialized();
                InitializeSourceState();

                //Ensure a main container exists for AllInOne mode before creating the first VisualCopy
                if (Configuration.Main.AddNewVisualCopy != null &&
                    string.Equals(Configuration.Main.AddNewVisualCopy.Method.Name, "AllInOne_AddNewVC", StringComparison.Ordinal))
                {
                    if (MainContainer == null || !MainContainer.IsLoaded)
                    {
                        MainContainer = new ContainerWindow(Controller);
                        Controller.RegisterContainer(MainContainer);
                        if (Controller is AppController controller)
                            controller.SetMainContainer(MainContainer);
                    }
                }

                //Add new VisualCopy
                var vc = Configuration.Main.AddNewVisualCopy();

                //If Destiny not exist
                if (StartupClass.requestInfo.Content != RquestContent.None)
                    viewModel.CheckAndFixRequest();

                //Start operation
                if (StartupClass.requestInfo.Content == RquestContent.All)
                    Configuration.Main.SetRunningState(vc, StartupClass.requestInfo);

                IntegrationManager.EnsureMinimalRegistryKeysIfMissing(Configuration.Main);
                ApplyIntegrationState();
                if (Controller is AppController controllerInstance)
                    controllerInstance.WarnIfElevated();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Class: VisualCopyHandler\n Method: MyInitialize\nMessage: {0}", ex.Message));
            }
        }

        #region WndProc

        #region Messages

        /// <summary>
        /// Inform a window (CopyHandler) thats must add dataInfo to it's list.
        /// </summary>
        public const int WM_ADD_DATAINFO = 5000;
        public const int WM_CREATE = 0x0001;
        public const int WM_ALERT = 5001;

        #endregion

        #region Exter Functions

        [DllImportAttribute("User32.dll")]
        public static extern IntPtr SendMessage(int hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);

        [DllImportAttribute("User32.dll")]
        public static extern bool ShowWindow(int hWnd, int nCmdShow);

        [DllImportAttribute("User32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImportAttribute("User32.dll")]
        public static extern int GetClassName(IntPtr hWnd, char[] lpClassName, int nMaxCount);

        [DllImportAttribute("User32.dll")]
        public static extern long SetWindowLong(IntPtr hWnd,int nIndex,long dwNewLong);

        [DllImportAttribute("User32.dll")]
        public static extern long GetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            EnsureCommonStateInitialized();
            InitializeSourceState();
            MyInitialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_ALERT:
                    Process.GetCurrentProcess().Kill();
                    break;
                case WM_CREATE:
                    if (StartupClass.IsTrayLaunch && IntegrationManager.IsResident(Configuration.Main))
                        Visibility = System.Windows.Visibility.Hidden;
                    break;
                case WM_ADD_DATAINFO:
                    Controller.RequestShowMainWindow("WM_ADD_DATAINFO");
                    viewModel.HandleAddDataInfo(VisualsCopys);
                    break;

            }

            return IntPtr.Zero;
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var appController = Controller as AppController;
            if (IntegrationManager.IsResident(Configuration.Main) && !(appController?.IsExitAllowed() ?? false))
            {
                // Resident mode: closing the main window should NOT leave transfers alive in hidden containers.
                // Cancel all transfers first, then hide to tray.
                try { Controller.CancelAllTransfers(); } catch { }

                e.Cancel = true;
                Controller.RequestHideToTray("Main window closing");
                return;
            }

            Controller.RequestExit("Main window closing");
        }

        public void ApplyIntegrationState()
        {
            Controller.ApplyIntegrationState();
        }

        internal void HandleAddDataInfo(IEnumerable<VisualCopy> visualsCopys)
        {
            viewModel.HandleAddDataInfo(visualsCopys);
        }

        public void HideToTray()
        {
            Controller.RequestHideToTray("HideToTray");
        }

        public void ExitToLegacy()
        {
            if (Controller is AppController controller)
                controller.ExitToLegacy();
            else
                Controller.RequestExit("Exit to legacy");
        }

        public void RequestExitToLegacyAfterConfigClose()
        {
            if (Controller is AppController controller)
                controller.RequestExitToLegacyAfterConfigClose();
        }

        public void ExecutePendingExitToLegacy()
        {
            if (Controller is AppController controller)
                controller.ExecutePendingExitToLegacy();
        }
    }

    public class StartupClass
    {
        internal static readonly AppController Controller = new AppController();
        private static Mutex trayLaunchMutex;
        public static int Id;
        public static CmdShellExtAgent cmdShellAgent;
        public static RequestInfo requestInfo=new RequestInfo();
        public static bool IsTrayLaunch;
        public StartupClass() { }
        [STAThread]
        public static void Main(string[] arguments)
        {
            //try
            //{
                #region Get ShellExt Arguments

                //for (int i = 0; i < arguments.Length; i++)
                //{
                //    MessageBox.Show(arguments[i]);
                //}

                //if (arguments != null && arguments.Length > 0)
                //{
                //    if (arguments[0] == "GET_ARGUMENTS")
                //    {
                //        var path = RegisterAccess.Acces.GetFilesListPath();
                //        //var writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write), Encoding.Unicode);

                //        var ope = arguments[1];

                //        //for (int i = 2; i < arguments.Length - 1; i++)
                //        //    writer.WriteLine(arguments[i]);

                //        //writer.Close();
                //        //writer.Dispose();

                //        var destiny = arguments[arguments.Length - 1];

                //        arguments = new string[] { ope, '*' + path, destiny };

                //    }

                //    //MessageBox.Show(arguments.Length.ToString());
                //}

                #endregion

                Configuration.Main = Configuration.LoadFromRegister();
                IntegrationManager.UpdateAutoStart(Configuration.Main);
                IsTrayLaunch = arguments != null && arguments.Any(a => string.Equals(a, "--tray", StringComparison.OrdinalIgnoreCase));
                if (IsTrayLaunch && !TryAcquireTrayLaunchMutex())
                    return;

                IntegrationManager.EnsureMinimalRegistryKeysIfMissing(Configuration.Main);

                App myApp = new App();

                cmdShellAgent = new CmdShellExtAgent(arguments);

                if (cmdShellAgent.ArgumentsState == ArgumentsState.Valids)
                {

                    //Fixing arguments
                    requestInfo = cmdShellAgent.ProcessArguments();

                    requestInfo.Content = RquestContent.All;

                    //Set arguments to the Registry, so any other CopyHandle may know.
                    RegisterAccess.Acces.SetLastArguments(requestInfo.Arguments);

                    //Try add DataInfo'list to any copyHandler
                    var res = VisualsCopysHandler.FindWindow(null, "VisualsCopysHandler");
                    if (res != (IntPtr)0)
                    {
                        VisualsCopysHandler.SendMessage((int)res, VisualsCopysHandler.WM_ADD_DATAINFO, (UIntPtr)0, (IntPtr)0);
                        Process.GetCurrentProcess().Kill();
                    }
                }

                //MessageBox.Show("Hello NeathCopy");

                var nc = new VisualsCopysHandler(Controller);
                myApp.MainWindow = nc;
                Controller.Start();
                if (!IsTrayLaunch)
                    nc.Show();
                myApp.Run();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "StartupClass", "Main"));
            //}
        }

        private static bool TryAcquireTrayLaunchMutex()
        {
            try
            {
                bool createdNew;
                trayLaunchMutex = new Mutex(true, @"Local\NeathCopy.TrayLaunch", out createdNew);
                return createdNew;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }

}

