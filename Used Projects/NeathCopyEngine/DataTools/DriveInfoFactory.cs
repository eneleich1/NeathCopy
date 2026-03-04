using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NeathCopyEngine.Helpers;

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
        public static IDriveInfo CreateDriveInfo(string path)
        {
            var displayPath = PathDisplayHelper.ToDisplayPath(path);
            if (string.IsNullOrWhiteSpace(displayPath))
                throw new ArgumentException("Path for drive information cannot be null or empty.", nameof(path));

            if (TryGetUncShareRoot(displayPath, out _))
                return new NetworkDriveInfo(displayPath);

            return new SystemDriveInfo(displayPath);
        }

        public static bool TryGetRefreshPath(string path, out string refreshPath)
        {
            refreshPath = null;
            var displayPath = PathDisplayHelper.ToDisplayPath(path);
            if (string.IsNullOrWhiteSpace(displayPath))
                return false;

            if (TryGetUncShareRoot(displayPath, out var uncShareRoot))
            {
                refreshPath = uncShareRoot;
                return true;
            }

            var localRoot = PathDisplayHelper.GetRootForDriveInfo(displayPath);
            if (string.IsNullOrWhiteSpace(localRoot))
                return false;

            refreshPath = localRoot;
            return true;
        }

        public static bool TryGetUncShareRoot(string path, out string uncShareRoot)
        {
            uncShareRoot = null;
            var displayPath = PathDisplayHelper.ToDisplayPath(path);
            if (string.IsNullOrWhiteSpace(displayPath))
                return false;

            if (!displayPath.StartsWith(@"\\", StringComparison.Ordinal))
                return false;

            var trimmed = displayPath.TrimEnd('\\');
            var parts = trimmed.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return false;

            uncShareRoot = string.Format(@"\\{0}\{1}\", parts[0], parts[1]);
            return true;
        }
    }

    public class SystemDriveInfo:IDriveInfo
    {
        DriveInfo driveInfo;
        string Path;

        public SystemDriveInfo(string path)
        {
            Path = PathDisplayHelper.ToDisplayPath(path);
            driveInfo = new DriveInfo(PathDisplayHelper.GetRootForDriveInfo(Path));
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
        string normalizedPath;
        string uncShareRoot;

        public NetworkDriveInfo(string path)
        {
            var displayPath = PathDisplayHelper.ToDisplayPath(path);
            if (!DriveInfoFactory.TryGetUncShareRoot(displayPath, out var shareRoot))
                throw new ArgumentException("Path must be a valid UNC path.", nameof(path));

            normalizedPath = displayPath;
            uncShareRoot = shareRoot;

            if (!DiskSpaceNative.GetDiskFreeSpaceEx(uncShareRoot, out var freeBytesAvailable, out var totalNumberOfBytes, out var totalNumberOfFreeBytes))
                throw new IOException("Unable to retrieve drive information for network path.");

            availableFreeSpace = (long)freeBytesAvailable;
            totalFreeSpace = (long)totalNumberOfFreeBytes;
            totalSize = (long)totalNumberOfBytes;
        }

        public long AvailableFreeSpace => availableFreeSpace;

        public long TotalFreeSpace => totalFreeSpace;

        public long TotalSize => totalSize;

        public string VolumeLabel => "Net";

        public string Name => uncShareRoot;

        public string RootDirectory => uncShareRoot;

        public DriveType DriveType => DriveType.Network;

        public string DriveFormat => "Net Format";

        public IDriveInfo Clone()
        {
           return new NetworkDriveInfo(normalizedPath);
        }
    }

    internal static class DiskSpaceNative
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
    }


}
