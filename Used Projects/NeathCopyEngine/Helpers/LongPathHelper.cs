using System;
using System.IO;

namespace NeathCopyEngine.Helpers
{
    public static class LongPathHelper
    {
        private const int LongPathThreshold = 240;

        public static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (path.StartsWith(@"\\?\"))
                return path;

            string fullPath = Path.GetFullPath(path);

            if (fullPath.Length < LongPathThreshold)
                return fullPath;

            if (fullPath.StartsWith(@"\\"))
            {
                // UNC path
                return @"\\?\UNC\" + fullPath.Substring(2);
            }

            return @"\\?\" + fullPath;
        }
    }
}
