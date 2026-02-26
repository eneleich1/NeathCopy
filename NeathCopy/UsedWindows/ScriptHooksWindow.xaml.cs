using Microsoft.Win32;
using NeathCopy.Services;
using NeathCopyEngine.Helpers;
using System;
using System.Windows;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for ScriptHooksWindow.xaml
    /// </summary>
    public partial class ScriptHooksWindow : Window
    {
        public ScriptHooksWindow()
        {
            InitializeComponent();
            Loaded += ScriptHooksWindow_Loaded;
        }

        private void ScriptHooksWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            SuccessEnabledCheckBox.IsChecked = ReadEnabled("Hook_Success_Enabled");
            SuccessPathTextBox.Text = RegisterAccess.Acces.GetConfigurationValue("Hook_Success_Path");
            SuccessArgsTextBox.Text = RegisterAccess.Acces.GetConfigurationValue("Hook_Success_Args");
            SuccessIncludeContextCheckBox.IsChecked = ReadEnabled("Hook_Success_IncludeContext");

            ErrorEnabledCheckBox.IsChecked = ReadEnabled("Hook_Error_Enabled");
            ErrorPathTextBox.Text = RegisterAccess.Acces.GetConfigurationValue("Hook_Error_Path");
            ErrorArgsTextBox.Text = RegisterAccess.Acces.GetConfigurationValue("Hook_Error_Args");
            ErrorIncludeContextCheckBox.IsChecked = ReadEnabled("Hook_Error_IncludeContext");

            CancelEnabledCheckBox.IsChecked = ReadEnabled("Hook_Cancel_Enabled");
            CancelPathTextBox.Text = RegisterAccess.Acces.GetConfigurationValue("Hook_Cancel_Path");
            CancelArgsTextBox.Text = RegisterAccess.Acces.GetConfigurationValue("Hook_Cancel_Args");
            CancelIncludeContextCheckBox.IsChecked = ReadEnabled("Hook_Cancel_IncludeContext");
        }

        private static bool ReadEnabled(string key)
        {
            var value = RegisterAccess.Acces.GetConfigurationValue(key);
            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSettings())
                return;

            SaveSettings();
            MessageBox.Show("Script hooks saved.");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SuccessBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            SuccessPathTextBox.Text = BrowseForScript(SuccessPathTextBox.Text);
        }

        private void ErrorBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorPathTextBox.Text = BrowseForScript(ErrorPathTextBox.Text);
        }

        private void CancelBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            CancelPathTextBox.Text = BrowseForScript(CancelPathTextBox.Text);
        }

        private void SuccessTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSettings())
                return;

            SaveSettings();
            ScriptHookService.RunHook(HookType.Success, BuildTestContext());
        }

        private void ErrorTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSettings())
                return;

            SaveSettings();
            ScriptHookService.RunHook(HookType.Error, BuildTestContext());
        }

        private void CancelTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSettings())
                return;

            SaveSettings();
            ScriptHookService.RunHook(HookType.Cancel, BuildTestContext());
        }

        private static string BrowseForScript(string initialPath)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "All files|*.*",
                FileName = initialPath ?? ""
            };

            return dialog.ShowDialog() == true ? dialog.FileName : initialPath;
        }

        private bool ValidateSettings()
        {
            if (!ValidateSection(SuccessEnabledCheckBox.IsChecked, SuccessPathTextBox.Text, "Success"))
                return false;
            if (!ValidateSection(ErrorEnabledCheckBox.IsChecked, ErrorPathTextBox.Text, "Error"))
                return false;
            if (!ValidateSection(CancelEnabledCheckBox.IsChecked, CancelPathTextBox.Text, "Cancel"))
                return false;

            return true;
        }

        private bool ValidateSection(bool? enabled, string path, string label)
        {
            if (enabled == true && string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show(label + " hook is enabled but path is empty.");
                return false;
            }

            return true;
        }

        private void SaveSettings()
        {
            RegisterAccess.Acces.SetConfigurationValue("Hook_Success_Enabled", SuccessEnabledCheckBox.IsChecked == true ? "1" : "0");
            RegisterAccess.Acces.SetConfigurationValue("Hook_Success_Path", SuccessPathTextBox.Text ?? "");
            RegisterAccess.Acces.SetConfigurationValue("Hook_Success_Args", SuccessArgsTextBox.Text ?? "");
            RegisterAccess.Acces.SetConfigurationValue("Hook_Success_IncludeContext", SuccessIncludeContextCheckBox.IsChecked == true ? "1" : "0");

            RegisterAccess.Acces.SetConfigurationValue("Hook_Error_Enabled", ErrorEnabledCheckBox.IsChecked == true ? "1" : "0");
            RegisterAccess.Acces.SetConfigurationValue("Hook_Error_Path", ErrorPathTextBox.Text ?? "");
            RegisterAccess.Acces.SetConfigurationValue("Hook_Error_Args", ErrorArgsTextBox.Text ?? "");
            RegisterAccess.Acces.SetConfigurationValue("Hook_Error_IncludeContext", ErrorIncludeContextCheckBox.IsChecked == true ? "1" : "0");

            RegisterAccess.Acces.SetConfigurationValue("Hook_Cancel_Enabled", CancelEnabledCheckBox.IsChecked == true ? "1" : "0");
            RegisterAccess.Acces.SetConfigurationValue("Hook_Cancel_Path", CancelPathTextBox.Text ?? "");
            RegisterAccess.Acces.SetConfigurationValue("Hook_Cancel_Args", CancelArgsTextBox.Text ?? "");
            RegisterAccess.Acces.SetConfigurationValue("Hook_Cancel_IncludeContext", CancelIncludeContextCheckBox.IsChecked == true ? "1" : "0");
        }

        private static ScriptHookContext BuildTestContext()
        {
            return new ScriptHookContext
            {
                Operation = "copy",
                Source = "",
                Destination = "",
                Engine = "",
                BufferSize = 0,
                FilesCount = 0,
                TotalBytes = 0,
                ElapsedMs = 0,
                SpeedBytesPerSec = 0,
                ErrorsCount = 0,
                CancelCause = "",
                RequestSources = new System.Collections.Generic.List<string>(),
                Errors = new System.Collections.Generic.List<string>()
            };
        }
    }
}
