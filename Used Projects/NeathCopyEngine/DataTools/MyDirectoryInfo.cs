using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NeathCopyEngine.DataTools
{
    class MyDirectoryInfo
    {
        List<FileDataInfo> Files;
        List<string> Directories;
        //bool FileSystemsLoaded;

        //internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        //internal const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        public string FullName { get; set; }
        public string DestinyPath { get; set; }

        public MyDirectoryInfo(string fullName, string destinyPath)
        {
            FullName = fullName;
            DestinyPath = destinyPath;

            Files = new List<FileDataInfo>();
            Directories = new List<string>();
        }

        //void LoadFileSystems()
        //{
        //    string safeFullName = @"\\?\" + FullName;
        //    List<string> results = new List<string>();
        //    WIN32_FIND_DATA findData;
        //    IntPtr findHandle = LongPath.Directory.FindFirstFile(safeFullName + @"\*", out findData);

        //    string newFullName="";
        //    string currentFileName = "";

        //    if (findHandle != INVALID_HANDLE_VALUE)
        //    {
        //        bool found;

        //        do
        //        {
        //            currentFileName = findData.cFileName;
        //            newFullName = Path.Combine(FullName, currentFileName);

        //            // if this is a directory, find its contents
        //            if (((int)findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
        //            {
        //                if (currentFileName != "." && currentFileName != "..")
        //                {
        //                    Directories.Add(newFullName);
        //                }
        //            }
        //            else // This is a file
        //            {
        //                Files.Add(new FileDataInfo
        //                {
        //                    FullName = newFullName,
        //                    DestinyDirectoryPath = DestinyPath,
        //                    Name = Path.GetFileName(newFullName),
        //                    DestinyPath = Path.Combine(DestinyPath, Path.GetFileName(newFullName)),
        //                    Size = findData.Length,
        //                    //FileAttributes = findData.dwFileAttributes,
        //                    //CreationTime=findData.ftCreationTime,
        //                    //LastAccessTime=findData.ftLastAccessTime,
        //                    //LastWriteTime=findData.ftLastWriteTime
        //                });
        //            }

        //            // find next
        //            found = LongPath.Directory.FindNextFile(findHandle, out findData);
        //        }
        //        while (found);
        //    }

        //    // close the find handle
        //    LongPath.File.FindClose(findHandle);

        //    FileSystemsLoaded = true;
        //}

        public List<FileDataInfo> GetFiles()
        {
            //if (!FileSystemsLoaded)
            //    LoadFileSystems();

            var dinfo = new DirectoryInfo(PathUtils.ToLongPath(FullName));
            Files = dinfo.GetFiles().Select(f => new FileDataInfo()
            {
                FullName = Path.Combine(FullName, f.Name),
                DestinyDirectoryPath = DestinyPath,
                Name = Path.GetFileName(f.Name),
                DestinyPath = Path.Combine(DestinyPath, Path.GetFileName(Path.Combine(FullName, f.Name))),
                Size = f.Length,
                FileAttributes = f.Attributes,
                CreationTime = f.CreationTime,
                LastAccessTime = f.LastAccessTime,
                LastWriteTime = f.LastWriteTime
            }).ToList();

            Files.Sort(Compare);

            return Files;
        }
        static int Compare(FileDataInfo f1, FileDataInfo f2)
        {
            return f1.Name.CompareTo(f2.Name);
        }

        public List<string> GetDirectories()
        {
            //if (!FileSystemsLoaded)
            //    LoadFileSystems();

            return Directory.GetDirectories(PathUtils.ToLongPath(FullName)).ToList();
            //Files = dinfo.GetFiles().Select(f => new FileDataInfo() { }).ToList();

            //return Directories;

        }
    }
}
