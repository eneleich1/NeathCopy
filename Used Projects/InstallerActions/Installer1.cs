using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace InstallerActions
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        public Installer1()
        {
            InitializeComponent();

            CreateAppFolderAndFiles();
        }

        //C:\Users\Public\AppData\NeathCopy\FilesList.txt
        void CreateAppFolderAndFiles()
        {
            try
            {
                string winDrive = Path.GetPathRoot(Environment.SystemDirectory);

                var appDir = Path.Combine(winDrive, @"Users\Public\AppData\NeathCopy");
                var filesList = Path.Combine(appDir, "FilesList.txt");

                Directory.CreateDirectory(appDir);
                File.Create(filesList);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
