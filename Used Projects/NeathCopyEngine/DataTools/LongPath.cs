using System.IO;
using NeathCopyEngine.DataTools;

namespace LongPath
{
    public static class Directory
    {
        public static void Delete(string directory)
        {
            System.IO.Directory.Delete(PathUtils.ToLongPath(directory), true);
        }

        public static bool Exists(string directory)
        {
            return System.IO.Directory.Exists(PathUtils.ToLongPath(directory));
        }

        public static string[] GetFileSystemEntries(string directory)
        {
            return System.IO.Directory.GetFileSystemEntries(PathUtils.ToLongPath(directory));
        }

        public static void CreateDirectoriesInPath(string path)
        {
            if (path != null)
            {
                System.IO.Directory.CreateDirectory(PathUtils.ToLongPath(path));
            }
        }
    }

    public static class File
    {
        public static void Delete(string file)
        {
            System.IO.File.Delete(PathUtils.ToLongPath(file));
        }

        public static bool Exists(string file)
        {
            return System.IO.File.Exists(PathUtils.ToLongPath(file));
        }

        public static void Copy(string sourceFile, string destinationFile, bool failIfDestinationExists)
        {
            System.IO.File.Copy(PathUtils.ToLongPath(sourceFile), PathUtils.ToLongPath(destinationFile), !failIfDestinationExists);
        }

        public static void Move(string sourceFile, string destinationFile, bool overwrite)
        {
            var dest = PathUtils.ToLongPath(destinationFile);
            if (overwrite && System.IO.File.Exists(dest))
            {
                System.IO.File.Delete(dest);
            }
            System.IO.File.Move(PathUtils.ToLongPath(sourceFile), dest);
        }
    }
}
