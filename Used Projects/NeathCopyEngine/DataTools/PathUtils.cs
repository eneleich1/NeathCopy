using System;

namespace NeathCopyEngine.DataTools
{
    public static class PathUtils
    {
        public static string ToLongPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            if (path.StartsWith(@"\\?\"))
                return path;
            if (path.StartsWith(@"\\"))
                return @"\\?\UNC\" + path.Substring(2);
            return @"\\?\" + path;
        }
    }
}
