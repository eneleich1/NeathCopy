using NeathCopy.Module2_Configuration;
using NeathCopy.Services;
using NeathCopy.Themes;
using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace NeathCopy.Services.AppControl
{
    internal sealed class AppController : IAppController
    {
        private VisualsCopysHandler mainWindow;
        private ContainerWindow mainContainer;
        private readonly List<ContainerWindow> containers = new List<ContainerWindow>();
        private readonly CopyPipeServer pipeServer = new CopyPipeServer();
        private global::System.Windows.Forms.NotifyIcon trayIcon;
        private DispatcherTimer trayTimer;
        private bool allowExit;
        private bool trayBalloonShownThisRun;
        private bool pendingExitToLegacy;
        private bool isShuttingDown;
        private static bool elevationWarningShown;
        private readonly TransferOrchestrator transferOrchestrator;

        public AppController()
        {
            transferOrchestrator = new TransferOrchestrator(() => containers);
        }

        internal List<ContainerWindow> ContainersList => containers;

        public void Start()
        {
            ApplyIntegrationState();
        }

        public void ApplyIntegrationState()
        {
            var resident = IntegrationManager.IsResident(Configuration.Main);
            IntegrationManager.EnsureMinimalRegistryKeysIfMissing(Configuration.Main);
            IntegrationManager.UpdateAutoStart(Configuration.Main);

            if (resident)
            {
                StartPipeServer();
                EnsureTrayIcon();
                StartTrayTimer();

                if (StartupClass.IsTrayLaunch)
                {
                    HideToTray();
                    mainContainer?.Hide();
                }
            }
            else
            {
                StopPipeServer();
                StopTrayTimer();
                DisposeTrayIcon();

                if (StartupClass.IsTrayLaunch && !pendingExitToLegacy)
                    FullShutdown("Tray launch in legacy mode");
            }
        }

        public void RequestShowMainWindow(string reason = null)
        {
            EnsureMainWindow();
            var shouldEnsureContainers = true;
            if (IsSeparateWindowsMode() && (string.Equals(reason, "Pipe request", StringComparison.Ordinal)
                || string.Equals(reason, "WM_ADD_DATAINFO", StringComparison.Ordinal)))
            {
                shouldEnsureContainers = false;
            }

            ShowMainWindow(shouldEnsureContainers);
        }

        public void RequestHideToTray(string reason = null)
        {
            HideToTray();
        }

        public void RequestExit(string reason = null)
        {
            FullShutdown(string.IsNullOrWhiteSpace(reason) ? "Request exit" : reason);
        }

        public bool HasAnyTransfers()
        {
            return transferOrchestrator.HasAnyTransfers();
        }

        public bool HasAnyActiveTransfers()
        {
            return transferOrchestrator.HasAnyActiveTransfers();
        }

        public void CancelAllTransfers()
        {
            transferOrchestrator.CancelAllTransfers();
        }

        public void PauseAllTransfers()
        {
            transferOrchestrator.PauseAllTransfers();
        }

        public void ResumeAllTransfers()
        {
            transferOrchestrator.ResumeAllTransfers();
        }

        public void RegisterContainer(ContainerWindow container)
        {
            if (container == null)
                return;

            if (!containers.Contains(container))
                containers.Add(container);

            if (mainContainer == null)
                SetMainContainer(container);
        }

        public void UnregisterContainer(ContainerWindow container)
        {
            if (container == null)
                return;

            containers.Remove(container);

            if (mainContainer == container)
                mainContainer = null;

            if (!IntegrationManager.IsResident(Configuration.Main) && containers.Count == 0)
                RequestExit("Last container closed in legacy mode");
        }

        internal void AttachMainWindow(VisualsCopysHandler window)
        {
            if (window == null)
                return;

            mainWindow = window;
            VisualsCopysHandler.MainHandler = window;
        }

        internal void SetMainContainer(ContainerWindow container)
        {
            mainContainer = container;
            VisualsCopysHandler.MainContainer = container;
        }

        internal void RequestExitToLegacyAfterConfigClose()
        {
            pendingExitToLegacy = true;
        }

        internal void ExecutePendingExitToLegacy()
        {
            if (!pendingExitToLegacy)
                return;

            pendingExitToLegacy = false;
            if (!IntegrationManager.IsResident(Configuration.Main))
                FullShutdown("Switched to legacy, closing after settings");
        }

        internal void ExitToLegacy()
        {
            FullShutdown("Exit to legacy");
        }

        internal void AllowExitNow()
        {
            allowExit = true;
        }

        internal bool IsExitAllowed()
        {
            return allowExit;
        }

        internal void WarnIfElevated()
        {
            if (elevationWarningShown || !IsProcessElevated())
                return;

            elevationWarningShown = true;
            var warning = GetResourceString("t135", "NeathCopy is running as Administrator, drag & drop from Explorer will not work. Please restart NeathCopy normally.");
            var question = GetResourceString("t136", "Restart NeathCopy normally now?");
            var message = string.Format("{0}{1}{1}{2}", warning, Environment.NewLine, question);

            var result = MessageBox.Show(message, "NeathCopy", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
                RestartAsNonElevated();
        }

        private void RestartAsNonElevated()
        {
            try
            {
                var exe = System.Windows.Forms.Application.ExecutablePath;
                Process.Start(new ProcessStartInfo("explorer.exe", string.Format("\"{0}\"", exe)) { UseShellExecute = true });
            }
            catch (Exception)
            {
            }

            allowExit = true;
            Application.Current?.Shutdown();
        }

        private void StartPipeServer()
        {
            if (pipeServer.IsRunning)
                return;

            pipeServer.Start(HandlePipeRequest);
        }

        private void StopPipeServer()
        {
            pipeServer.Stop();
        }

        private void HandlePipeRequest(CopyPipeRequest request)
        {
            if (request == null)
                return;

            var dispatcher = (mainWindow ?? Application.Current?.MainWindow)?.Dispatcher ?? Application.Current?.Dispatcher;
            if (dispatcher == null)
                return;

            dispatcher.Invoke(() =>
            {
                try
                {
                    EnsureMainWindow();
                    RequestShowMainWindow("Pipe request");

                    var operation = (request.Operation ?? "").Trim().ToLowerInvariant();
                    var sources = request.Sources == null ? new List<string>() : request.Sources.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    var destiny = request.Destination ?? "";

                    LogPipeRequest(string.Format("HandlePipeRequest start operation={0} sources={1} destiny={2}", operation, sources.Count, destiny), null);

                    if (sources.Count == 0 || string.IsNullOrWhiteSpace(destiny))
                        return;

                    try
                    {
                        var listPath = RegisterAccess.Acces.GetFilesListPath();
                        if (!string.IsNullOrWhiteSpace(listPath))
                        {
                            var listContent = "|" + string.Join("|", sources) + "|";
                            File.WriteAllText(listPath, listContent, Encoding.Unicode);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogPipeRequest("HandlePipeRequest failed writing FilesList", ex);
                    }

                    var info = new RequestInfo(operation, sources, destiny) { Content = RquestContent.All };
                    RegisterAccess.Acces.SetPendingCopyRequestInfo(info);
                    info.Arguments[0] = operation;
                    info.Arguments[1] = "*" + RegisterAccess.Acces.GetFilesListPath();
                    info.Arguments[2] = destiny;
                    RegisterAccess.Acces.SetLastArguments(info.Arguments);

                    LogPipeRequest(string.Format("HandlePipeRequest visualsCopys={0} addDataBehaviour={1}",
                        VisualsCopysHandler.VisualsCopys.Count(),
                        Configuration.Main.addDataBehaviour == null ? "(null)" : Configuration.Main.addDataBehaviour.GetType().Name), null);

                    mainWindow.HandleAddDataInfo(VisualsCopysHandler.VisualsCopys);

                    LogPipeRequest("HandlePipeRequest success", null);
                }
                catch (Exception ex)
                {
                    LogPipeRequest("HandlePipeRequest failed", ex);
                }
            });
        }

        private static readonly object pipeLogLock = new object();

        private void LogPipeRequest(string message, Exception ex)
        {
            try
            {
                var logsDir = RegisterAccess.Acces.GetLogsDir();
                if (string.IsNullOrWhiteSpace(logsDir))
                    logsDir = AppDomain.CurrentDomain.BaseDirectory;

                var path = System.IO.Path.Combine(logsDir, "Pipe.log");
                var line = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, message);
                if (ex != null)
                    line = line + Environment.NewLine + ex.ToString();

                lock (pipeLogLock)
                {
                    File.AppendAllText(path, line + Environment.NewLine);
                }
            }
            catch (Exception logEx)
            {
                System.Diagnostics.Trace.WriteLine("Pipe log failure: " + logEx);
            }
        }

        private bool EnsureTrayIcon()
        {
            if (trayIcon != null)
            {
                UpdateTrayMenu();
                return true;
            }

            try
            {
                trayIcon = new global::System.Windows.Forms.NotifyIcon();
                trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(global::System.Windows.Forms.Application.ExecutablePath);
                trayIcon.Text = "NeathCopy";

                UpdateTrayMenu();
                trayIcon.Visible = true;
                trayIcon.DoubleClick += (s, e) => RequestShowMainWindow("Tray double click");
            }
            catch (Exception)
            {
                DisposeTrayIcon();
                return false;
            }

            return true;
        }

        private void DisposeTrayIcon()
        {
            if (trayIcon == null)
                return;

            trayIcon.Visible = false;
            trayIcon.Dispose();
            trayIcon = null;
            trayBalloonShownThisRun = false;
        }

        private void StartTrayTimer()
        {
            if (trayTimer != null || mainWindow == null)
                return;

            trayTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, (s, e) => UpdateTrayTooltip(), mainWindow.Dispatcher);
            trayTimer.Start();
        }

        private void StopTrayTimer()
        {
            if (trayTimer == null)
                return;

            trayTimer.Stop();
            trayTimer = null;
        }

        private void UpdateTrayTooltip()
        {
            if (trayIcon == null)
                return;

            var active = VisualsCopysHandler.VisualsCopys.FirstOrDefault(vc =>
                vc.State == VisualCopy.VisualCopyState.Runing || vc.State == VisualCopy.VisualCopyState.Discovering);

            if (active == null)
            {
                trayIcon.Text = "NeathCopy";
                return;
            }

            var percent = Math.Max(0f, Math.Min(100f, active.displayInfo.OverallPorcent));
            var sizeInfo = active.displayInfo.OverallSizeTransferred ?? "";
            var operation = active.RequestInf?.Operation ?? "copy";
            var verb = operation == "move" || operation == "fastmove" ? "Moving" : "Copying";

            var text = string.Format("NeathCopy  {0}: {1:0}% ({2})", verb, percent, sizeInfo);
            trayIcon.Text = text.Length > 63 ? text.Substring(0, 63) : text;
        }

        private void HideToTray()
        {
            var trayReady = EnsureTrayIcon();
            if (mainWindow != null)
            {
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.Hide();
            }
            mainContainer?.Hide();
            if (trayReady)
                ShowTrayBalloonIfResident();
        }

        private void ShowMainWindow(bool ensureContainers)
        {
            if (mainWindow == null)
                return;

            if (!mainWindow.Dispatcher.CheckAccess())
            {
                mainWindow.Dispatcher.Invoke(() => ShowMainWindow(ensureContainers));
                return;
            }

            if (mainWindow.Visibility != Visibility.Visible)
                mainWindow.Show();

            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            mainWindow.Focus();

            if (ensureContainers)
                EnsureContainerHasAtLeastOneVisualCopy();
        }

        private void EnsureContainerWindow()
        {
            if (mainWindow == null)
                return;

            if (!mainWindow.Dispatcher.CheckAccess())
            {
                mainWindow.Dispatcher.Invoke(EnsureContainerWindow);
                return;
            }

            if (mainContainer == null || !mainContainer.IsLoaded)
            {
                mainContainer = new ContainerWindow(this);
                SetMainContainer(mainContainer);
                if (!containers.Contains(mainContainer))
                    containers.Add(mainContainer);
            }

            if (VisualsCopysHandler.MainContainer != mainContainer)
                VisualsCopysHandler.MainContainer = mainContainer;

            if (mainContainer.WindowState == WindowState.Minimized)
                mainContainer.WindowState = WindowState.Normal;

            if (mainContainer.VisualsCopys.Any())
            {
                if (mainContainer.Visibility != Visibility.Visible)
                    mainContainer.Show();

                mainContainer.Activate();
            }
        }

        private void EnsureContainerHasAtLeastOneVisualCopy()
        {
            if (mainWindow == null)
                return;

            if (!mainWindow.Dispatcher.CheckAccess())
            {
                mainWindow.Dispatcher.Invoke(EnsureContainerHasAtLeastOneVisualCopy);
                return;
            }

            var activeContainer = containers
                .FirstOrDefault(c => c != null && c.IsLoaded && c.VisualsCopys.Any());

            if (activeContainer != null)
                mainContainer = activeContainer;

            var isSeparateWindows = IsSeparateWindowsMode();

            if (isSeparateWindows)
            {
                if (activeContainer == null || !activeContainer.VisualsCopys.Any())
                    Configuration.Main.AddNewVisualCopy();
                return;
            }

            EnsureContainerWindow();

            if (!mainContainer.VisualsCopys.Any())
            {
                Configuration.Main.AddNewVisualCopy();
            }

            EnsureContainerWindow();
        }

        private bool IsSeparateWindowsMode()
        {
            var methodName = Configuration.Main.AddNewVisualCopy == null
                ? null
                : Configuration.Main.AddNewVisualCopy.Method.Name;
            return string.Equals(methodName, "SeparateWindows_AddNewVC", StringComparison.Ordinal);
        }

        private void UpdateTrayMenu()
        {
            if (trayIcon == null)
                return;

            var menu = new global::System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add(GetResourceString("t131", "Open"), null, (s, e) => RequestShowMainWindow("Tray Open"));
            menu.Items.Add(GetResourceString("t132", "Settings"), null, (s, e) => Theme.ConfigWindow.ShowDialog());

            var disableDefault = new global::System.Windows.Forms.ToolStripMenuItem(GetResourceString("t133", "Disable default copy handler"));
            disableDefault.Enabled = Configuration.Main.IsDefaultCopyHandler;
            disableDefault.Click += (s, e) => DisableDefaultCopyHandler();
            menu.Items.Add(disableDefault);

            menu.Items.Add(new global::System.Windows.Forms.ToolStripSeparator());
            menu.Items.Add(GetResourceString("t134", "Exit"), null, (s, e) => RequestExit("Tray exit"));

            trayIcon.ContextMenuStrip = menu;
        }

        private void DisableDefaultCopyHandler()
        {
            try
            {
                string error;
                var ok = IntegrationManager.TrySetDefaultCopyHandler(false, Application.Current?.MainWindow, out error);

                Configuration.Main.IsDefaultCopyHandler = false;
                IntegrationManager.EnsureMinimalRegistryKeysIfMissing(Configuration.Main);
                IntegrationManager.UpdateAutoStart(Configuration.Main);

                if (!ok && !string.IsNullOrWhiteSpace(error))
                    MessageBox.Show(error, "NeathCopy", MessageBoxButton.OK, MessageBoxImage.Warning);

                StopPipeServer();
                ApplyIntegrationState();
                UpdateTrayMenu();

                if (!IntegrationManager.IsResident(Configuration.Main))
                    FullShutdown("Disable default handler from tray");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "NeathCopy", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowTrayBalloonIfResident()
        {
            if (trayIcon == null)
                return;

            if (!IsResidentMode())
                return;

            if (trayBalloonShownThisRun)
                return;

            trayBalloonShownThisRun = true;
            var text = GetResourceString("t127", "NeathCopy is still running in the system tray.");
            trayIcon.BalloonTipTitle = "NeathCopy";
            trayIcon.BalloonTipText = text;
            trayIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            try
            {
                trayIcon.ShowBalloonTip(3000);
            }
            catch (Exception)
            {
            }
        }

        private bool IsResidentMode()
        {
            return string.Equals(Configuration.Main.IntegrationMode, IntegrationManager.TrayIpcMode, StringComparison.OrdinalIgnoreCase)
                && Configuration.Main.IsDefaultCopyHandler;
        }


        private void EnsureMainWindow()
        {
            if (mainWindow == null || !mainWindow.IsLoaded)
            {
                mainWindow = new VisualsCopysHandler(this);
                mainWindow.Show();
            }
        }

        private void FullShutdown(string reason)
        {
            if (isShuttingDown)
                return;

            isShuttingDown = true;
            var resident = IntegrationManager.IsResident(Configuration.Main);

            try
            {
                StopPipeServer();
                StopTrayTimer();
                DisposeTrayIcon();
            }
            catch (Exception)
            {
            }

            try
            {
                foreach (var container in containers.ToList())
                {
                    if (container == null)
                        continue;

                    if (resident)
                    {
                        if (container.IsLoaded)
                            container.Hide();
                    }
                    else
                    {
                        if (container.IsLoaded)
                            container.Close();
                    }
                }

                if (mainContainer != null && mainContainer.IsLoaded && !containers.Contains(mainContainer))
                {
                    if (resident)
                        mainContainer.Hide();
                    else
                        mainContainer.Close();
                }
            }
            catch (Exception)
            {
            }

            allowExit = true;
            Application.Current?.Shutdown();
        }

        private static bool IsProcessElevated()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetResourceString(string key, string fallback)
        {
            try
            {
                var value = Application.Current?.TryFindResource(key) as string;
                return string.IsNullOrWhiteSpace(value) ? fallback : value;
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}
