using Alphaleonis.Win32.Filesystem;
using NeathCopy.Module1_ShellExt;
using NeathCopy.Module2_Configuration;
using NeathCopy.Themes;
using NeathCopy.UsedWindows;
using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace NeathCopy
{
    /// <summary>
    /// Interaction logic for VisualsCopysHandler.xaml
    /// </summary>
    public partial class VisualsCopysHandler : Window
    {
        public static HwndSource hwndCopy;

        public static VisualsCopysHandler MainHandler { get; private set; }
        public static ContainerWindow MainContainer { get; set; }
        public static List<ContainerWindow> ContainersList { get; private set; }
        public static IEnumerable<VisualCopy> VisualsCopys
        {
            get
            {
                foreach (var container in ContainersList)
                {
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
            ThemesManager.Manager.InitResources();
            ThemesManager.Manager.SetThemes(Configuration.Main);
        }
        public VisualsCopysHandler()
        {
            InitializeComponent();
        }

        private void CheckAndFixRequest()
        {
            var browseDestiny = new UserDropUIWindow();

            if (StartupClass.requestInfo.Operation != "copy" 
                && StartupClass.requestInfo.Operation != "move" && StartupClass.requestInfo.Operation != "fastmove")
            {
                browseDestiny.Title = string.Format("Invalid Operation: {0}, select Copy or Move.",StartupClass.requestInfo.Operation);
                browseDestiny.DestinyTextBox.Text = StartupClass.requestInfo.Destiny;
                browseDestiny.ShowDialog();
                if (browseDestiny.DlgResult)
                {
                    StartupClass.requestInfo.Destiny = browseDestiny.Destiny;
                    StartupClass.requestInfo.Operation = browseDestiny.Operation;
                    StartupClass.requestInfo.Content = RquestContent.Sources | RquestContent.Operation;
                }
                else Process.GetCurrentProcess().Kill();
            }

            //Destiny
            else if (!Alphaleonis.Win32.Filesystem.Directory.Exists(StartupClass.requestInfo.Destiny))
            {
                browseDestiny.Title = string.Format("Destiny: {0} do not exist. Browse other", StartupClass.requestInfo.Destiny); ;
                browseDestiny.OptionComboBox.SelectedIndex = StartupClass.requestInfo.Operation == "copy" ? 0 : 1;
                browseDestiny.DestinyTextBox.Text = StartupClass.requestInfo.Destiny;

                browseDestiny.ShowDialog();

                if (browseDestiny.DlgResult)
                {
                    StartupClass.requestInfo.Destiny = browseDestiny.Destiny;
                    StartupClass.requestInfo.Content = RquestContent.All;
                }
                else Process.GetCurrentProcess().Kill();
            }
            else StartupClass.requestInfo.Content = RquestContent.All;

            browseDestiny.Close();
        }
        private void MyInitialize()
        {
            try
            {
                //Initialize some fields
                MainContainer = new ContainerWindow();
                ContainersList = new List<ContainerWindow>();
                MainHandler = this;
                hwndCopy = PresentationSource.FromVisual(this) as HwndSource;
                hwndCopy.AddHook(WndProc);

                //Add new VisualCopy
                var vc = Configuration.Main.AddNewVisualCopy();

                //If Destiny not exist
                if (StartupClass.requestInfo.Content != RquestContent.None)
                    CheckAndFixRequest();

                //Start operation
                if (StartupClass.requestInfo.Content == RquestContent.All)
                    Configuration.Main.SetRunningState(vc, StartupClass.requestInfo);

                //System.Windows.Forms.MessageBox.Show("after start operation");

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
                    Visibility = System.Windows.Visibility.Hidden;
                    break;
                case WM_ADD_DATAINFO:
                    Configuration.Main.Process_ADD_DATA(VisualsCopys);
                    break;

            }

            return IntPtr.Zero;
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RegisterAccess.Acces.UnregisterCopyHandler();
            Process.GetCurrentProcess().Kill();
        }
    }

    public class StartupClass
    {
        public static int Id;
        public static CmdShellExtAgent cmdShellAgent;
        public static RequestInfo requestInfo=new RequestInfo();
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

                var nc = new VisualsCopysHandler();
                myApp.Run(nc);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopy", "StartupClass", "Main"));
            //}
        }
    }

}
