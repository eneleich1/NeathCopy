using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace NeathCopy.Services
{
    public enum HookType
    {
        Success,
        Error,
        Cancel
    }

    public sealed class ScriptHookContext
    {
        public string Operation { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Engine { get; set; }
        public int BufferSize { get; set; }
        public int FilesCount { get; set; }
        public long TotalBytes { get; set; }
        public long ElapsedMs { get; set; }
        public double SpeedBytesPerSec { get; set; }
        public int ErrorsCount { get; set; }
        public string CancelCause { get; set; }
        public List<string> RequestSources { get; set; }
        public List<string> Errors { get; set; }
    }

    public static class ScriptHookService
    {
        private const int DefaultTimeoutMs = 30000;

        public static void RunHook(HookType type, ScriptHookContext ctx)
        {
            Task.Run(() =>
            {
                try
                {
                    var settings = GetSettings(type);
                    if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.Path))
                        return;

                    var targetPath = Environment.ExpandEnvironmentVariables(settings.Path);
                    if (!File.Exists(targetPath))
                    {
                        Log(type, "Path not found: " + targetPath, null, null, null, null, null);
                        return;
                    }

                    var expandedArgs = ExpandArgs(settings.Args, ctx);
                    var contextPath = TryWriteContextFile(type, ctx, settings);
                    var command = BuildCommand(targetPath, expandedArgs, contextPath);
                    ExecuteProcess(type, command, targetPath, contextPath);
                }
                catch (Exception ex)
                {
                    Log(type, "Hook exception: " + ex.Message, null, null, null, null, null);
                }
            });
        }

        private static void ExecuteProcess(HookType type, HookCommand command, string targetPath, string contextPath)
        {
            string stdout = "";
            string stderr = "";
            int? exitCode = null;
            string status = "ok";

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = command.FileName,
                    Arguments = command.Arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = ResolveWorkingDirectory(targetPath)
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    var stdoutTask = process.StandardOutput.ReadToEndAsync();
                    var stderrTask = process.StandardError.ReadToEndAsync();

                    if (!process.WaitForExit(DefaultTimeoutMs))
                    {
                        status = "timeout";
                        try { process.Kill(); } catch { }
                    }

                    process.WaitForExit(2000);

                    if (stdoutTask.Wait(2000))
                        stdout = stdoutTask.Result;
                    if (stderrTask.Wait(2000))
                        stderr = stderrTask.Result;

                    if (process.HasExited)
                        exitCode = process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                status = "error: " + ex.Message;
            }

            Log(type, status, command, exitCode, stdout, stderr, contextPath);
        }

        private static string ResolveWorkingDirectory(string targetPath)
        {
            try
            {
                var dir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                    return dir;
            }
            catch
            {
            }

            return Environment.CurrentDirectory;
        }

        private static HookSettings GetSettings(HookType type)
        {
            var prefix = type == HookType.Success ? "Hook_Success" :
                type == HookType.Error ? "Hook_Error" : "Hook_Cancel";

            var enabledValue = RegisterAccess.Acces.GetConfigurationValue(prefix + "_Enabled");
            var enabled = string.Equals(enabledValue, "1", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(enabledValue, "true", StringComparison.OrdinalIgnoreCase);

            var includeContextValue = RegisterAccess.Acces.GetConfigurationValue(prefix + "_IncludeContext");
            var includeContext = string.Equals(includeContextValue, "1", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(includeContextValue, "true", StringComparison.OrdinalIgnoreCase);

            return new HookSettings
            {
                Enabled = enabled,
                Path = RegisterAccess.Acces.GetConfigurationValue(prefix + "_Path"),
                Args = RegisterAccess.Acces.GetConfigurationValue(prefix + "_Args"),
                IncludeContext = includeContext
            };
        }

        private static string ExpandArgs(string args, ScriptHookContext ctx)
        {
            if (string.IsNullOrEmpty(args))
                return "";

            return args
                .Replace("{Operation}", ctx?.Operation ?? "")
                .Replace("{Source}", ctx?.Source ?? "")
                .Replace("{Dest}", ctx?.Destination ?? "")
                .Replace("{Engine}", ctx?.Engine ?? "")
                .Replace("{BufferSize}", (ctx?.BufferSize ?? 0).ToString())
                .Replace("{FilesCount}", (ctx?.FilesCount ?? 0).ToString())
                .Replace("{Bytes}", (ctx?.TotalBytes ?? 0).ToString())
                .Replace("{ElapsedMs}", (ctx?.ElapsedMs ?? 0).ToString())
                .Replace("{SpeedBps}", (ctx?.SpeedBytesPerSec ?? 0).ToString("F0"))
                .Replace("{ErrorsCount}", (ctx?.ErrorsCount ?? 0).ToString())
                .Replace("{CancelCause}", ctx?.CancelCause ?? "");
        }

        private static HookCommand BuildCommand(string targetPath, string args, string contextPath)
        {
            var argsWithContext = BuildArgsWithContext(contextPath, args);
            var extension = Path.GetExtension(targetPath).ToLowerInvariant();

            if (extension == ".ps1")
            {
                var psArgs = string.Format("-NoProfile -ExecutionPolicy Bypass -File \"{0}\"{1}",
                    targetPath,
                    string.IsNullOrWhiteSpace(argsWithContext) ? "" : " " + argsWithContext);
                return new HookCommand("powershell.exe", psArgs);
            }

            if (extension == ".cmd" || extension == ".bat")
            {
                var cmdArgs = string.Format("/c \"\"{0}\"{1}\"",
                    targetPath,
                    string.IsNullOrWhiteSpace(argsWithContext) ? "" : " " + argsWithContext);
                return new HookCommand("cmd.exe", cmdArgs);
            }

            return new HookCommand(targetPath, argsWithContext ?? "");
        }

        private static void Log(HookType type, string status, HookCommand command, int? exitCode, string stdout, string stderr, string contextPath)
        {
            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Eneleich", "NeathCopy", "HookLogs");
                Directory.CreateDirectory(logDir);

                var sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine("Hook: " + type);
                sb.AppendLine("Status: " + status);
                if (command != null)
                    sb.AppendLine("Command: " + command.FileName + " " + command.Arguments);
                if (!string.IsNullOrWhiteSpace(contextPath))
                    sb.AppendLine("ContextFile: " + contextPath);
                if (exitCode.HasValue)
                    sb.AppendLine("ExitCode: " + exitCode.Value);
                if (!string.IsNullOrWhiteSpace(stdout))
                    sb.AppendLine("StdOut: " + stdout.Trim());
                if (!string.IsNullOrWhiteSpace(stderr))
                    sb.AppendLine("StdErr: " + stderr.Trim());

                var logPath = Path.Combine(logDir, "hooks.log");
                File.AppendAllText(logPath, sb.ToString());
            }
            catch
            {
                // Ignore logging failures.
            }
        }

        private class HookSettings
        {
            public bool Enabled { get; set; }
            public string Path { get; set; }
            public string Args { get; set; }
            public bool IncludeContext { get; set; }
        }

        private class HookCommand
        {
            public string FileName { get; }
            public string Arguments { get; }

            public HookCommand(string fileName, string arguments)
            {
                FileName = fileName;
                Arguments = arguments ?? "";
            }
        }

        private static string BuildArgsWithContext(string contextPath, string args)
        {
            if (string.IsNullOrWhiteSpace(contextPath))
                return args ?? "";

            if (string.IsNullOrWhiteSpace(args))
                return string.Format("-ContextFile \"{0}\"", contextPath);

            return string.Format("-ContextFile \"{0}\" {1}", contextPath, args);
        }

        private static string TryWriteContextFile(HookType type, ScriptHookContext ctx, HookSettings settings)
        {
            if (settings == null || !settings.IncludeContext)
                return null;

            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Eneleich", "NeathCopy", "HookLogs", "Context");
                Directory.CreateDirectory(logDir);

                var fileName = string.Format("{0}-{1}-{2}.json",
                    DateTime.Now.ToString("yyyyMMdd-HHmmss"),
                    type,
                    Guid.NewGuid().ToString("N"));
                var path = Path.Combine(logDir, fileName);

                var payload = new HookContextPayload
                {
                    HookType = type.ToString(),
                    Operation = ctx?.Operation ?? "",
                    Engine = ctx?.Engine ?? "",
                    BufferSize = ctx?.BufferSize ?? 0,
                    FilesCount = ctx?.FilesCount ?? 0,
                    TotalBytes = ctx?.TotalBytes ?? 0,
                    ElapsedMs = ctx?.ElapsedMs ?? 0,
                    SpeedBps = ctx?.SpeedBytesPerSec ?? 0,
                    RequestSources = ctx?.RequestSources ?? new List<string>(),
                    RequestDest = ctx?.Destination ?? "",
                    ErrorsCount = ctx?.ErrorsCount ?? 0,
                    Errors = ctx?.Errors,
                    CancelCause = ctx?.CancelCause ?? ""
                };

                var serializer = new DataContractJsonSerializer(typeof(HookContextPayload));
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    serializer.WriteObject(stream, payload);
                }

                return path;
            }
            catch (Exception ex)
            {
                Log(type, "Context serialization failed: " + ex.Message, null, null, null, null, null);
                return null;
            }
        }

        [DataContract]
        private class HookContextPayload
        {
            [DataMember] public string HookType { get; set; }
            [DataMember] public string Operation { get; set; }
            [DataMember] public string Engine { get; set; }
            [DataMember] public int BufferSize { get; set; }
            [DataMember] public int FilesCount { get; set; }
            [DataMember] public long TotalBytes { get; set; }
            [DataMember] public long ElapsedMs { get; set; }
            [DataMember] public double SpeedBps { get; set; }
            [DataMember] public List<string> RequestSources { get; set; }
            [DataMember] public string RequestDest { get; set; }
            [DataMember] public int ErrorsCount { get; set; }
            [DataMember] public List<string> Errors { get; set; }
            [DataMember] public string CancelCause { get; set; }
        }
    }
}
