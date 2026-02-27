using NeathCopy.Module2_Configuration;
using NeathCopyEngine.Helpers;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace NeathCopy.Services
{
    public static class IntegrationManager
    {
        public const string TrayIpcMode = "TrayIPC";
        public const string LegacyMode = "LegacyFileQueue";
        private const string AdminHelperExeName = "NeathCopy.AdminHelper.exe";

        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunValueName = "NeathCopy";

        public static bool IsResident(Configuration config)
        {
            if (config == null)
                return false;

            return config.IsDefaultCopyHandler &&
                   string.Equals(config.IntegrationMode, TrayIpcMode, StringComparison.OrdinalIgnoreCase);
        }

        public static void EnsureMinimalRegistryKeysIfMissing(Configuration config)
        {
            var exe = System.Windows.Forms.Application.ExecutablePath;
            var dir = Path.GetDirectoryName(exe) ?? string.Empty;
            var defaultFilesListPath = Path.Combine(@"C:\Users\Public\AppData\NeathCopy", "FilesList.txt");

            bool hasKey = RegisterAccess.Acces.HasCompanyKey();
            var currentCopyHandler = RegisterAccess.Acces.GetCopyHandlerPath();
            if (!hasKey || string.IsNullOrWhiteSpace(currentCopyHandler) || !File.Exists(currentCopyHandler))
            {
                RegisterAccess.Acces.SetCopyHandlerPath(exe);
                RegisterAccess.Acces.SetInstallDir(dir);
            }

            if (config != null)
            {
                if (!RegisterAccess.Acces.ValueExists("IsDefaultCopyHandler"))
                    RegisterAccess.Acces.SetDefaultCopyHandlerFlag(config.IsDefaultCopyHandler);
            }

            try
            {
                var hasFilesListValue = RegisterAccess.Acces.ValueExists("FilesList");
                var filesListPath = hasFilesListValue
                    ? RegisterAccess.Acces.GetFilesListPath()
                    : defaultFilesListPath;

                var filesListDir = Path.GetDirectoryName(filesListPath);
                if (!string.IsNullOrWhiteSpace(filesListDir))
                    Directory.CreateDirectory(filesListDir);

                if (!File.Exists(filesListPath))
                {
                    using (File.Create(filesListPath)) { }
                }

                if (!hasFilesListValue)
                    RegisterAccess.Acces.SetFilesListPath(filesListPath);
            }
            catch (Exception)
            {
            }
        }

        public static void ApplyRegistryKeysForDefaultHandler(Configuration config, bool enabling)
        {
            if (config == null)
                return;

            if (enabling)
            {
                var exe = System.Windows.Forms.Application.ExecutablePath;
                var dir = Path.GetDirectoryName(exe) ?? string.Empty;

                RegisterAccess.Acces.SetCopyHandlerPath(exe);
                RegisterAccess.Acces.SetInstallDir(dir);
                RegisterAccess.Acces.SetDefaultCopyHandlerFlag(true);
            }
            else
            {
                RegisterAccess.Acces.SetDefaultCopyHandlerFlag(false);
            }
        }

        public static void UpdateAutoStart(Configuration config)
        {
            var enable = IsResident(config);

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
                {
                    if (key == null) return;

                    if (enable)
                    {
                        var exe = System.Windows.Forms.Application.ExecutablePath;
                        var value = string.Format("\"{0}\" --tray", exe);
                        key.SetValue(RunValueName, value);
                    }
                    else
                    {
                        if (key.GetValueNames().Contains(RunValueName))
                            key.DeleteValue(RunValueName, false);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static bool TrySetDefaultCopyHandler(bool enable, Window owner, out string errorMessage)
        {
            errorMessage = null;

            if (enable)
            {
                if (!TryRegisterShellExtension(out errorMessage))
                    return false;
            }
            else
            {
                if (!TryUnregisterShellExtension(out errorMessage))
                    return false;
            }

            return true;
        }

        private static bool TryRegisterShellExtension(out string errorMessage)
        {
            errorMessage = null;
            var dllPath = ResolveShellExtDllPath();
            if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
            {
                errorMessage = GetResourceString("t128", "Default copy handler requires the shell extension to be registered. Please run the installer as admin.");
                return false;
            }

            return TryRunAdminHelper("register", dllPath, out errorMessage);
        }

        private static bool TryUnregisterShellExtension(out string errorMessage)
        {
            errorMessage = null;
            var dllPath = ResolveShellExtDllPath();
            if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
                return true;

            return TryRunAdminHelper("unregister", dllPath, out errorMessage);
        }

        private static string ResolveShellExtDllPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var candidate = Path.Combine(baseDir, "NeathCopyShell.dll");
            return candidate;
        }

        private static bool TryRunAdminHelper(string command, string dllPath, out string errorMessage)
        {
            errorMessage = null;

            var helperPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AdminHelperExeName);
            if (!File.Exists(helperPath))
            {
                errorMessage = GetResourceString("t138", "Administrative helper not found. Please reinstall NeathCopy.");
                return false;
            }

            try
            {
                var args = string.Format("{0} --dll \"{1}\"", command, dllPath);
                var psi = new ProcessStartInfo(helperPath, args)
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null)
                    {
                        errorMessage = GetResourceString("t129", "Failed to register the shell extension. Please run as administrator.");
                        return false;
                    }

                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        errorMessage = GetResourceString("t129", "Failed to register the shell extension. Please run as administrator.");
                        return false;
                    }

                    return true;
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                errorMessage = GetResourceString("t137", "Administrator approval was canceled.");
                return false;
            }
            catch (Exception)
            {
                errorMessage = GetResourceString("t129", "Failed to register the shell extension. Please run as administrator.");
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
