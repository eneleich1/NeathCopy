using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Forms;
using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.Helpers;

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
            var normalizedCurrentDir = LongPathHelper.Normalize(currentDir.FullName);
            var subdirs = Directory.GetDirectories(normalizedCurrentDir);

            DirectoryDataInfo child = null;
            DirectoryInfo dinfo = null;
            FileDataInfo file = null;

            //Retrieve the amount of files inside currentDir.
            int filesCount = 0;

            try
            {
                #region Algorihtmy

                EnsureDirectoryMetadata(currentDir, normalizedCurrentDir);

                //All childs in PostOrden
                foreach (var d in subdirs)
                {
                    var childFullName = Path.Combine(currentDir.FullName, Path.GetFileName(d));
                    var normalizedChild = LongPathHelper.Normalize(childFullName);
                    var childInfo = new DirectoryInfo(normalizedChild);
                    child = new DirectoryDataInfo
                    {
                        SourceDirectoryLength = this.SourceDirectoryLength,
                        Destiny = this.Destiny,
                        FullName = childFullName,
                        DestinyPath = Path.Combine(currentDir.DestinyPath, Path.GetFileName(childFullName)),
                        FileAttributes = childInfo.Attributes,
                        CreationTime = childInfo.CreationTime,
                        LastAccessTime = childInfo.LastAccessTime,
                        LastWriteTime = childInfo.LastWriteTime
                    };

                    filesCount += PostOrden(child, filesList, ref count);
                }

                //Retrieve all files
                dinfo = new DirectoryInfo(normalizedCurrentDir);
                var files = dinfo.GetFiles();
                filesCount += files.Length;
                foreach (var f in files)
                {
                    file = new FileDataInfo()
                    {
                        SourceDirectoryLength = this.SourceDirectoryLength,
                        Destiny = this.Destiny,
                        FullName = Path.Combine(currentDir.FullName, f.Name),
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

            Directories.Add(currentDir);

            return filesCount;
        }

        private void EnsureDirectoryMetadata(DirectoryDataInfo currentDir, string normalizedCurrentDir)
        {
            if (currentDir == null) return;

            if (currentDir.CreationTime == default(DateTime)
                && currentDir.LastAccessTime == default(DateTime)
                && currentDir.LastWriteTime == default(DateTime)
                && currentDir.FileAttributes == default(FileAttributes))
            {
                var dinfo = new DirectoryInfo(normalizedCurrentDir);
                currentDir.FileAttributes = dinfo.Attributes;
                currentDir.CreationTime = dinfo.CreationTime;
                currentDir.LastAccessTime = dinfo.LastAccessTime;
                currentDir.LastWriteTime = dinfo.LastWriteTime;
            }
        }

        public override void FastMove()
        {
            var src = LongPathHelper.Normalize(FullName);
            var dst = LongPathHelper.Normalize(DestinyPath);
            Directory.Move(src, dst);
        }
    }
}
