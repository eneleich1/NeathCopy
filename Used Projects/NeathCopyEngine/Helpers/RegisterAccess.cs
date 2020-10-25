using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;

namespace NeathCopyEngine.Helpers
{
    public class RegisterAccess
    {
        /// <summary>
        /// Get or set the Company-Software'RegisterKey.
        /// </summary>
        public string CompanyKey { get; set; }
        /// <summary>
        /// Get or set the register main key wich contain CompanyKey
        /// </summary>
        public RegistryKey MainKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LastArgumentsKey { get; set; }

        public string SettingsKeys { get; set; }

        public static RegisterAccess Acces = new RegisterAccess();

        public RegisterAccess(string companyKey, string lastsArgsKey,string settingsKey, RegistryKey mainKey)
        {
            CompanyKey = companyKey;
            MainKey = mainKey;
            LastArgumentsKey = lastsArgsKey;
            SettingsKeys = settingsKey;
        }
        public RegisterAccess():this(@"SOFTWARE\Eneleich\NeathCopy"
            , @"SOFTWARE\Eneleich\NeathCopy\LastArguments"
            , @"SOFTWARE\Eneleich\NeathCopy\Settigs"
            , Registry.CurrentUser)
        {
          
        }


        /// <summary>
        /// Enter in register and get the application install directory.
        /// </summary>
        /// <returns></returns>
        public string GetInstallDir()
        {
            //return Environment.CurrentDirectory;
            using (var key = MainKey.OpenSubKey(CompanyKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return "";

                object value = key.GetValue("InstallDir");
                if (value == null) return Application.StartupPath;
                return (string)value;
            }
        }

        /// <summary>
        /// Enter in register and get the errors logs path.
        /// </summary>
        /// <returns></returns>
        public string GetLogsDir()
        {
            //return Environment.CurrentDirectory;
            using (var key = MainKey.OpenSubKey(CompanyKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return "";

                object value = key.GetValue("LogsDir");
                if (value == null) return Application.StartupPath;
                return (string)value;
            }
        }

        /// <summary>
        /// Enter in register and the FilesList Path.
        /// FilesList is the files wich contains the files will be copied or moved.
        /// </summary>
        /// <returns></returns>
        public string GetFilesListPath()
        {
            //return Environment.CurrentDirectory;
            using (var key = MainKey.OpenSubKey(CompanyKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return "";

                object value = key.GetValue("FilesList");
                if (value == null) return System.IO.Path.Combine(Application.StartupPath, "FilesList.txt");
                return (string)value;
            }
        }

        /// <summary>
        /// Retrieve the arguments of the last request operation.
        /// </summary>
        /// <returns></returns>
        public RequestInfo GetLastCopyRequestInfo()
        {
            using (var key = MainKey.OpenSubKey(LastArgumentsKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return null;

                return new RequestInfo((string)key.GetValue("Operation"), (string)key.GetValue("Source"), (string)key.GetValue("Destiny")) {  Content= RquestContent.All};
            }
        }
        /// <summary>
        /// Retrieve the Handle of active CopyHandlersManager Window.
        /// </summary>
        /// <returns></returns>
        public int GetActiveCopyHandle()
        {
            using (var key = MainKey.OpenSubKey(CompanyKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return -1;

                object value = key.GetValue("Handle");
                return int.Parse((string)value);
            }
        }
        
        /// <summary>
        /// Set the last request arguments in to the registry.
        /// </summary>
        /// <param name="requestInfo"></param>
        public void SetLastArguments(string[] args)
        {
            using (var key = MainKey.OpenSubKey(LastArgumentsKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return;

                key.SetValue("Operation", args[0]);
                key.SetValue("Source", args[1]);
                key.SetValue("Destiny", args[2]);
            }
        }
     
        /// <summary>
        /// Enter in register and specific if there is any other NeathCopy runing.
        /// </summary>
        /// <returns></returns>
        public bool ExistAnyNeathCopy()
        {
            using (var key = MainKey.OpenSubKey(CompanyKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return false;

                object value = key.GetValue("Exist");

                if (value == null) return false;
                return (string)value == "1";
            }
        }
      
        /// <summary>
        /// Set the Copy Handle value and Exist registry'key to 1.
        /// </summary>
        /// <param name="hwnd"></param>
        public void RegisterCopyHandler(IntPtr hwnd)
        {
            using (var key = MainKey.OpenSubKey(CompanyKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return;

                key.SetValue("Exist", "1");
                key.SetValue("Handle", hwnd);
            }
        }
        /// <summary>
        /// Set the Copy Handle value to 0 and Exist registry'key to 0.
       /// </summary>
       /// <param name="hwnd"></param>
        public void UnregisterCopyHandler()
        {
            using (var key = MainKey.OpenSubKey(CompanyKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return;

                key.SetValue("Exist", "0");
                key.SetValue("Handle", "0");
            }
        }

        public string GetConfigurationValue(string aspect)
        {
            using (var key = MainKey.OpenSubKey(SettingsKeys, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return "";

                object value = key.GetValue(aspect);
                return value.ToString();
            }
        }
        public bool SetConfigurationValue(string aspect,object value)
        {
            using (var key = MainKey.OpenSubKey(SettingsKeys, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return false;

                key.SetValue(aspect, value);
                return true;
            }
        }

        public bool ExistConfiguration()
        {
            using (var key = MainKey.OpenSubKey(CompanyKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return false;

                var res = key.GetValue("ExistConfig");

                return res==null?false:res.ToString() == "1";
            }
        }

    }
}
