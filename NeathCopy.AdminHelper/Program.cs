using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NeathCopy.AdminHelper
{
    internal static class Program
    {
        private const int ExitSuccess = 0;
        private const int ExitUsageError = 1;
        private const int ExitDllMissing = 2;
        private const int ExitRegsvrFailed = 3;
        private const int ExitUnhandled = 5;

        public static int Main(string[] args)
        {
            try
            {
                if (args == null || args.Length == 0)
                {
                    WriteUsage("Missing command.");
                    return ExitUsageError;
                }

                var command = args[0].Trim().ToLowerInvariant();
                var dllPath = GetArgumentValue(args, "--dll");

                if (string.IsNullOrWhiteSpace(dllPath))
                {
                    WriteUsage("Missing --dll argument.");
                    return ExitUsageError;
                }

                if (!File.Exists(dllPath))
                {
                    Log($"DLL not found: {dllPath}");
                    Console.WriteLine($"DLL not found: {dllPath}");
                    return ExitDllMissing;
                }

                var register = command == "register";
                if (!register && command != "unregister")
                {
                    WriteUsage($"Unknown command: {command}");
                    return ExitUsageError;
                }

                var exitCode = RunRegsvr32(dllPath, register);
                if (exitCode != 0)
                {
                    Log($"regsvr32 failed with exit code {exitCode} for {dllPath}");
                    Console.WriteLine($"regsvr32 failed with exit code {exitCode}");
                    return ExitRegsvrFailed;
                }

                Log($"regsvr32 {(register ? "register" : "unregister")} succeeded for {dllPath}");
                Console.WriteLine("OK");

                if (args.Any(a => string.Equals(a, "--restart-explorer", StringComparison.OrdinalIgnoreCase)))
                {
                    RestartExplorer();
                }

                return ExitSuccess;
            }
            catch (Exception ex)
            {
                Log($"Unhandled error: {ex.Message}");
                Console.WriteLine("Unhandled error.");
                return ExitUnhandled;
            }
        }

        private static string GetArgumentValue(string[] args, string key)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }

            return null;
        }

        private static int RunRegsvr32(string dllPath, bool register)
        {
            try
            {
                var args = register
                    ? string.Format("/s \"{0}\"", dllPath)
                    : string.Format("/s /u \"{0}\"", dllPath);

                var psi = new ProcessStartInfo("regsvr32.exe", args)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit();
                    return process == null ? -1 : process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to run regsvr32: {ex.Message}");
                return -1;
            }
        }

        private static void RestartExplorer()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("explorer"))
                {
                    process.Kill();
                    process.WaitForExit();
                }

                Process.Start("explorer.exe");
                Log("Explorer restarted.");
            }
            catch (Exception ex)
            {
                Log($"Failed to restart Explorer: {ex.Message}");
            }
        }

        private static void WriteUsage(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Usage: NeathCopy.AdminHelper register --dll <path> | unregister --dll <path>");
            Log(message);
        }

        private static void Log(string message)
        {
            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Eneleich", "NeathCopy", "logs");
                Directory.CreateDirectory(logDir);

                var logPath = Path.Combine(logDir, "adminhelper.log");
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(logPath, line);
            }
            catch (Exception)
            {
            }
        }
    }
}
