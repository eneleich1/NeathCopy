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
            var src = LongPathHelper.Normalize(FullName);
            return File.Open(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        public FileStream GetStreamToWrite(FileMode mode)
        {
            var dst = LongPathHelper.Normalize(DestinyPath);
            return new FileInfo(dst).Create();
        }

        public override void FastMove()
        {
            var src = LongPathHelper.Normalize(FullName);
            var dst = LongPathHelper.Normalize(DestinyPath);
            File.Move(src, dst);
        }

        public static bool Md5Check(string file1, string file2)
        {
            var finfo1 = new FileInfo(LongPathHelper.Normalize(file1));
            var finfo2 = new FileInfo(LongPathHelper.Normalize(file2));

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
                using (var stream = File.OpenRead(LongPathHelper.Normalize(fileName)))
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
