using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace NeathCopyEngine.DataTools
{
    public class FileDataInfo : DataInfo
    {
        public long Size{get;set;}

        public MySize FormatedSize
        {
            get { return new MySize(Size); }
        } 

        public string DestinyDirectoryPath { get; set; }

        public override List<FileDataInfo> GetFiles(ref int Count)
        {
            Count++;
            return new List<FileDataInfo> { this };
        }
        public FileStream GetStreamToRead()
        {
           return Delimon.Win32.IO.File.Open(FullName, Delimon.Win32.IO.FileMode.Open,
                Delimon.Win32.IO.FileAccess.Read, Delimon.Win32.IO.FileShare.ReadWrite);
        }
        public FileStream GetStreamToWrite(FileMode mode)
        {
            return new Delimon.Win32.IO.FileInfo(DestinyPath).Create();
        }

        public override void FastMove()
        {
            File.Move(FullName, DestinyPath);
        }

        public static bool Md5Check(string file1, string file2)
        {
            var finfo1 = new Alphaleonis.Win32.Filesystem.FileInfo(file1);
            var finfo2 = new Alphaleonis.Win32.Filesystem.FileInfo(file2);

            if (finfo1.Length != finfo2.Length) return false;

            // Full-file MD5 comparison (not partial checksum).
            try
            {
                var hash1 = md5Checksum(file1);
                var hash2 = md5Checksum(file2);

                return string.Equals(hash1, hash2, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string md5Checksum(string fileName)
        {

            using (var md5 = MD5.Create())
            {
                using (var stream = Alphaleonis.Win32.Filesystem.File.OpenRead(fileName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty);
                }
            }
        }

    }

    public enum CopyState
    {
        Waiting,Copied,Moved,Processing,Skiped,Error
    }
}
