
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace InstallerActions
{
    partial class Installer1
    {
        static string neathCopyFolder = $"C:\\Users\\Public\\AppData\\NeathCopy";
        static string filesList = Path.Combine(neathCopyFolder, "FilesList.txt");

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            
            SetRegisterPaths();

            components = new System.ComponentModel.Container();

            //Create the user folder
            var dinfo = Directory.CreateDirectory(neathCopyFolder);
            
            //create the fileList
            var fs = File.Create(filesList);
            fs.Dispose();

            
        }

        /// <summary>
        /// Set the path for Logs directory and FilesList.txt file
        /// </summary>
        public void SetRegisterPaths()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Eneleich\NeathCopy", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key == null) return;

                key.SetValue("LogsDir", neathCopyFolder);
                key.SetValue("FilesList", filesList);
            }



            //    using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Eneleich\NeathCopy", RegistryKeyPermissionCheck.ReadWriteSubTree))
            //    {
            //        MessageBox.Show(key.ToString());
            //        if (key == null) return;

            //    try
            //    {
            //        object value = key.GetValue("FilesList");
            //        MessageBox.Show(value.ToString());
            //        key.SetValue("LogsDir", "esto esta de madre");
            //        key.SetValue("FilesList", filesList);
            //        var nestorKey = key.CreateObjRef(typeof(string));

            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //    }
            //}

        }

        #endregion
    }
}