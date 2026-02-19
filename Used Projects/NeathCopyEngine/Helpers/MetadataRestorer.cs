using System;
using System.IO;
namespace NeathCopyEngine.Helpers
{
    public static class MetadataRestorer
    {
        public static void EnsureFileWritable(string destFilePath)
        {
            if (string.IsNullOrWhiteSpace(destFilePath)) return;

            var dest = LongPathHelper.Normalize(destFilePath);
            if (!File.Exists(dest)) return;

            var attrs = File.GetAttributes(dest);
            if ((attrs & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(dest, attrs & ~FileAttributes.ReadOnly);
        }

        public static void RestoreFileMetadata(string sourceFilePath, string destFilePath)
        {
            if (string.IsNullOrWhiteSpace(destFilePath)) return;

            var dest = LongPathHelper.Normalize(destFilePath);
            if (!File.Exists(dest)) return;

            var source = string.IsNullOrWhiteSpace(sourceFilePath) ? null : LongPathHelper.Normalize(sourceFilePath);
            if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
                source = dest;

            EnsureFileWritable(dest);

            var srcInfo = new FileInfo(source);
            File.SetCreationTime(dest, srcInfo.CreationTime);
            File.SetLastWriteTime(dest, srcInfo.LastWriteTime);
            File.SetLastAccessTime(dest, srcInfo.LastAccessTime);
            File.SetAttributes(dest, srcInfo.Attributes);
        }

        public static void RestoreDirectoryMetadata(string sourceDirPath, string destDirPath)
        {
            if (string.IsNullOrWhiteSpace(destDirPath)) return;

            var dest = LongPathHelper.Normalize(destDirPath);
            if (!Directory.Exists(dest)) return;

            var source = string.IsNullOrWhiteSpace(sourceDirPath) ? null : LongPathHelper.Normalize(sourceDirPath);
            if (string.IsNullOrWhiteSpace(source) || !Directory.Exists(source))
                source = dest;

            var srcInfo = new DirectoryInfo(source);
            Directory.SetCreationTime(dest, srcInfo.CreationTime);
            Directory.SetLastWriteTime(dest, srcInfo.LastWriteTime);
            Directory.SetLastAccessTime(dest, srcInfo.LastAccessTime);
            File.SetAttributes(dest, srcInfo.Attributes);
        }
    }
}
