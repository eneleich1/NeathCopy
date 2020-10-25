using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeathCopyEngine.CopyHandlers;
using NeathCopyEngine.Exceptions;
using Delimon.Win32.IO;

namespace NeathCopyEngine.DataTools
{
    public abstract class DataInfo
    {
        public string FullName{get;set;}
        public string DestinyPath { get; set; }
        public List<string> DestinyPaths = new List<string>();
        public string Name { get; set; }
        /// <summary>
        /// Original Destiny of this DataInfo.
        /// DestinyPath = Destiny/FileName on roots DataInfo.
        /// Destiny is the first part of File wich was Discovered
        /// </summary>
        public string Destiny { get; set; }
        public int SourceDirectoryLength { get; set; }

        public Delimon.Win32.IO.FileAttributes FileAttributes { get; set; }
        public DateTime CreationTime;
        public DateTime LastAccessTime;
        public DateTime LastWriteTime;

        /// <summary>
        /// Get or set the current state of this instance.
        /// Use in Files To Copy or Move list.
        /// </summary>
        public CopyState CopyState { get; set; }

        public List<DirectoryDataInfo> EmptyDirs = new List<DirectoryDataInfo>();

        public DataInfo()
        {
            CopyState = DataTools.CopyState.Waiting;
        }

        public abstract void FastMove();
        /// <summary>
        /// Retrieve a Enumerable of FileDataInfo wich files'information to copy.
        /// </summary>
        /// <returns></returns>
        public abstract List<FileDataInfo> GetFiles(ref int Count);

        /// <summary>
        /// Hold destiny path names use to set on Directory(File)DataInfo.
        /// </summary>
        public static HashSet<string> DestinyPathsNames = new HashSet<string>();
        /// <summary>
        /// Parse the FileSystemName and retrieve a FileDataInfo or DirectoryDataInfo
        /// with DestinyPath as destPath param.
        /// </summary>
        /// <param name="FileSystemName"></param>
        /// <param name="destPath"></param>
        /// <returns></returns>
        public static DataInfo Parse(string fileSystemName, string destinyPath)
        {

            var resultingDestinyPath = Path.Combine(destinyPath, Path.GetFileName(fileSystemName));

            var di = new Delimon.Win32.IO.DirectoryInfo(fileSystemName);

            //If is directory
            if (Alphaleonis.Win32.Filesystem.Directory.Exists(fileSystemName))
            {
                if (resultingDestinyPath == fileSystemName)
                {
                    resultingDestinyPath += " - Copy";

                    for (int i = 1; DestinyPathsNames.Contains(resultingDestinyPath)
                           || Directory.Exists(resultingDestinyPath); i++)
                    {
                        resultingDestinyPath += i.ToString();
                    }
                }

                return new DirectoryDataInfo
                {
                    SourceDirectoryLength=Path.GetDirectoryName(fileSystemName).Length,
                    Destiny=destinyPath,
                    FullName = fileSystemName,
                    DestinyPath = resultingDestinyPath,
                    FileAttributes=di.Attributes
                };
            }
            //if is a file
            else if (Alphaleonis.Win32.Filesystem.File.Exists(fileSystemName))
            {
                if (resultingDestinyPath == fileSystemName)
                {
                    var ext = System.IO.Path.GetExtension(fileSystemName);
                    var extLess = resultingDestinyPath.Remove(resultingDestinyPath.Length - 4, 4);
                    resultingDestinyPath = string.Format("{0} - Copy{1}", extLess, ext);

                    for (int i = 1; DestinyPathsNames.Contains(resultingDestinyPath)
                        || File.Exists(resultingDestinyPath); i++)
                    {
                        extLess = resultingDestinyPath.Remove(resultingDestinyPath.Length - 4, 4);
                        resultingDestinyPath = string.Format("{0}_{1}{2}", extLess, i, ext);
                    }
                }

                var finfo = new FileInfo(fileSystemName);

                //Delimon.Win32.IO.File.GetCreationTime(fileSystemName);
                var res = new FileDataInfo
                {
                    SourceDirectoryLength = Path.GetDirectoryName(fileSystemName).Length,
                    Destiny =destinyPath,
                    FullName = fileSystemName,
                    Name = finfo.Name,
                    DestinyDirectoryPath = destinyPath,
                    DestinyPath = resultingDestinyPath,
                    Size = finfo.Length,
                    FileAttributes = finfo.Attributes,
                    CreationTime=finfo.CreationTime,
                    LastAccessTime=finfo.LastAccessTime,
                    LastWriteTime=finfo.LastWriteTime
                };

                return res;
            }

            //This filesystem do not exist
            throw new FileSystemNotExistException(fileSystemName);

        }

        public override string ToString()
        {
            return string.Format("[FullName: {0}, Destiny: {1}]", FullName, DestinyPath);
        }
    }
}
