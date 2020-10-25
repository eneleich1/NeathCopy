using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Delimon.Win32.IO;
using System.Security.AccessControl;
using System.Windows.Forms;
using NeathCopyEngine.CopyHandlers;

namespace NeathCopyEngine.DataTools
{
    public class DirectoryDataInfo : DataInfo
    {
        public override List<FileDataInfo> GetFiles(ref int Count)
        {
            var filesList = new List<FileDataInfo>();
            PostOrden(this, filesList, ref Count);

            return filesList;
        }

        private int PostOrden(DirectoryDataInfo currentDir, List<FileDataInfo> filesList, ref int count)
        {
            //Push all directories
            var subdirs = Delimon.Win32.IO.Directory.GetDirectories(currentDir.FullName);

            DirectoryDataInfo child = null;
            DirectoryInfo dinfo = null;
            FileDataInfo file = null;

            //Retrieve the amount of files inside currentDir.
            int filesCount = 0;

            try
            {
                #region Algorihtmy

                //All childs in PostOrden
                foreach (var d in subdirs)
                {
                    child = new DirectoryDataInfo
                    {
                        FullName = d,
                        DestinyPath = Path.Combine(currentDir.DestinyPath, Path.GetFileName(d))
                    };

                    filesCount += PostOrden(child, filesList, ref count);
                }

                //Retrieve all files
                dinfo = new DirectoryInfo(currentDir.FullName);
                var files = dinfo.GetFiles();
                filesCount += files.Length;
                foreach (var f in files)
                {
                    file = new FileDataInfo()
                    {
                        SourceDirectoryLength = this.SourceDirectoryLength,
                        Destiny = this.Destiny,
                        FullName = Path.Combine(dinfo.FullName, f.Name),
                        DestinyDirectoryPath = currentDir.DestinyPath,
                        Name = Path.GetFileName(f.Name),
                        DestinyPath = Path.Combine(currentDir.DestinyPath, f.Name),
                        Size = f.Length,
                        FileAttributes = f.Attributes,
                        CreationTime = f.CreationTime,
                        LastAccessTime = f.LastAccessTime,
                        LastWriteTime = f.LastWriteTime
                    };

                    filesList.Add(file);

                    count++;//Increase Reference Count
                }

                #endregion  
            }
            catch(Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopyEngine", "DirectoryDataInfo", "PostOrden"));

                var message = Error.GetErrorLogInLine(ex.Message, "NeathCopyEngine", "DirectoryDataInfo", "PostOrden");
                using (var w = new System.IO.StreamWriter(new System.IO.FileStream("Errors Log.txt", System.IO.FileMode.Append, System.IO.FileAccess.Write)))
                {
                    w.WriteLine("-------------------------------");
                    w.WriteLine(System.DateTime.Now);
                    w.WriteLine(message);
                }
            }

            if (filesCount==0)
                EmptyDirs.Add(currentDir);

            return filesCount;
        }

        public override void FastMove()
        {
            Directory.Move(FullName, DestinyPath);
        }
    }
}
