using System;
using System.IO;

namespace NeathCopyEngine.Helpers
{
    public static class PathDisplayHelper
    {
        private const string LongPathPrefix = @"\\?\";
        private const string LongUncPrefix = @"\\?\UNC\";

        public static string ToDisplayPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (path.StartsWith(LongUncPrefix, StringComparison.OrdinalIgnoreCase))
                return @"\\" + path.Substring(LongUncPrefix.Length);

            if (path.StartsWith(LongPathPrefix, StringComparison.OrdinalIgnoreCase))
                return path.Substring(LongPathPrefix.Length);

            return path;
        }

        public static Uri ToFileUri(string path)
        {
            var displayPath = ToDisplayPath(path);
            return new Uri(displayPath);
        }

        public static string GetRootForDriveInfo(string path)
        {
            var displayPath = ToDisplayPath(path);
            return Path.GetPathRoot(displayPath);
        }
    }
}
