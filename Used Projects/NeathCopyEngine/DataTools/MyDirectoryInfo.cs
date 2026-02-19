using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NeathCopyEngine.Helpers;

namespace NeathCopyEngine.DataTools
{
    class MyDirectoryInfo
    {
        List<FileDataInfo> Files;
        List<string> Directories;
        //bool FileSystemsLoaded;

        public string FullName { get; set; }
        public string DestinyPath { get; set; }

        public MyDirectoryInfo(string fullName, string destinyPath)
        {
            FullName = fullName;
            DestinyPath = destinyPath;

            Files = new List<FileDataInfo>();
            Directories = new List<string>();
        }

        public List<FileDataInfo> GetFiles()
        {
            var normalizedFullName = LongPathHelper.Normalize(FullName);
            var dinfo = new DirectoryInfo(normalizedFullName);
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
            var normalizedFullName = LongPathHelper.Normalize(FullName);
            return Directory.GetDirectories(normalizedFullName).ToList();
        }
    }
}
