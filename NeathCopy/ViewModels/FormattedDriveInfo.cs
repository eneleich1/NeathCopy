using NeathCopyEngine.DataTools;
using NeathCopyEngine.Helpers;
using System;
using System.IO;

namespace NeathCopy.ViewModels
{
    public class FormattedDriveInfo
    {
        public string Volumen { get; set; }
        public MySize Capacity { get; set; }
        public MySize UsedSpace { get; set; }
        public MySize FreeSpace { get; set; }
        public MySize RequireSpace { get; set; }
        public MySize NeedMore { get; set; }
        public DriveType VolumenType { get; set; }

        public FormattedDriveInfo(IDriveInfo info, long freeSpace, long requireSpace)
        {
            Volumen = string.Format("[{2}]: {0} ({1})", info.VolumeLabel, info.Name.Substring(0, info.Name.Length - 1), info.DriveFormat);
            VolumenType = info.DriveType;
            Capacity = new MySize(info.TotalSize);
            UsedSpace = new MySize(info.TotalSize - freeSpace);
            FreeSpace = new MySize(freeSpace);
            RequireSpace = new MySize(requireSpace);
            NeedMore = new MySize(requireSpace - freeSpace);
        }
    }
}
