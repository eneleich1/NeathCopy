using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

            return NeathCheckSum(finfo1) == NeathCheckSum(finfo2);
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

        /// <summary>
        /// Retrieve the NeathCheckSum Definition of the File.
        /// NeathCheckSum Compare the length and starting 1024 bytes
        /// and lastest 1024 bytes as well.
        /// </summary>
        /// <param name="finfo"></param>
        /// <returns></returns>
        private static string NeathCheckSum(Alphaleonis.Win32.Filesystem.FileInfo finfo)
        {
            int bytesCount = 1024;
            int totalBytes = bytesCount * 2;
            byte[] buffer = new byte[bytesCount];

            byte[] start = new byte[bytesCount];
            byte[] end = new byte[bytesCount];

            if (finfo.Length <= totalBytes)
            {
                using (FileStream fs = finfo.OpenRead())
                {
                    fs.Read(buffer, 0, bytesCount);

                    return BitConverter.ToString(buffer, 0);
                }
            }
            else
            {
                using (FileStream fs = finfo.OpenRead())
                {
                    fs.Read(start, 0, bytesCount);

                    fs.Position = finfo.Length - bytesCount;

                    fs.Read(end, 0, bytesCount);

                    return BitConverter.ToString(start, 0) + BitConverter.ToString(end, 0);
                }
            }
        }

    }

    public enum CopyState
    {
        Waiting,Copied,Moved,Processing,Skiped,Error
    }
}
