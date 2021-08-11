using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delimon.Win32.IO;

namespace NeathCopyEngine.DataTools
{
    public interface IDriveInfo
    {
        long AvailableFreeSpace { get; }
        long TotalFreeSpace { get; }
        long TotalSize { get; }
        string VolumeLabel { get; }
        string Name { get; }
        string RootDirectory { get; }
        string DriveFormat { get; }
        DriveType DriveType { get; }
        IDriveInfo Clone();
    }

    public class DriveInfoFactory
    {
        static Uri uri;

        public static IDriveInfo CreateDriveInfo(string path)
        {
            uri = new Uri(path);
            if (uri.IsUnc) return new NetworkDriveInfo(path);
            return new SystemDriveInfo(path);
        }
    }

    public class SystemDriveInfo:IDriveInfo
    {
        DriveInfo driveInfo;
        string Path;

        public SystemDriveInfo(string path)
        {
            Path = path;
            driveInfo = new DriveInfo(path);
        }

        public long AvailableFreeSpace => driveInfo.AvailableFreeSpace;

        public long TotalFreeSpace => driveInfo.TotalFreeSpace;

        public long TotalSize => driveInfo.TotalSize;

        public string VolumeLabel => driveInfo.VolumeLabel;

        public string Name => driveInfo.Name;

        public string RootDirectory => driveInfo.RootDirectory.FullName;

        public DriveType DriveType => driveInfo.DriveType;

        public string DriveFormat => driveInfo.DriveFormat;

        public IDriveInfo Clone()
        {
            return new SystemDriveInfo(Path);
        }
    }

    public class NetworkDriveInfo : IDriveInfo
    {
        long availableFreeSpace, totalFreeSpace, totalSize;
        string Path;

        public NetworkDriveInfo(string path)
        {
            var nd = new Alphaleonis.Win32.Network.DriveConnection(path);
            var driveInfo = new DriveInfo(nd.LocalName);

            availableFreeSpace=driveInfo.AvailableFreeSpace;
            totalFreeSpace = driveInfo.TotalFreeSpace;
            totalSize = driveInfo.TotalSize;
            Path = nd.Share;
            nd.Dispose();
        }

        public long AvailableFreeSpace => availableFreeSpace;

        public long TotalFreeSpace => totalFreeSpace;

        public long TotalSize => totalSize;

        public string VolumeLabel => "Net";

        public string Name => "Net";

        public string RootDirectory => "Net";

        public DriveType DriveType => DriveType.Network;

        public string DriveFormat => "Net Format";

        public IDriveInfo Clone()
        {
           return new NetworkDriveInfo(Path);
        }
    }

}
