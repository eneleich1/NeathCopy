using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Text;
using NeathCopyEngine.DataTools;
using NeathCopyEngine;

namespace LongPath
{    
    public static class Directory
    {        
        /// <summary>
        /// Deletes the specified directory and all sub-directories and files.
        /// </summary>
        /// <param name="directory">Directory to be deleted.  Must begin with \\?\ for local paths or \\?\UNC\ for network paths.</param>
        public static void Delete(string directory)
        {
            WIN32_FIND_DATA findData;

            IntPtr findHandle = Directory.FindFirstFile(directory + @"\*", out findData);

            if (findHandle != INVALID_HANDLE_VALUE)
            {
                bool found;

                do
                {
                    string currentFileName = findData.cFileName;

                    // if this is a directory, find its contents
                    if (((int)findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
                    {
                        if (currentFileName != "." && currentFileName != "..")
                        {
                            string adir = System.IO.Path.Combine(directory, currentFileName);
                            Delete(adir);
                            bool result = Directory.RemoveDirectory(adir);
                            int lastWin32Error = Marshal.GetLastWin32Error();
                            if (!result)
                            {
                                throw new System.ComponentModel.Win32Exception(lastWin32Error);
                            }
                        }
                    }
                    else // it's a file; add it to the results
                    {
                        File.Delete(System.IO.Path.Combine(directory, currentFileName));
                    }

                    // find next
                    found = Directory.FindNextFile(findHandle, out findData);
                }
                while (found);
            }

            // close the find handle
            Directory.FindClose(findHandle);
        }

        /// <summary>
        /// Check if the specified directory exists.
        /// </summary>
        /// <param name="directory">Path of directory to check.  Must begin with \\?\ for local paths or \\?\UNC\ for network paths.</param>
        /// <returns></returns>
        public static bool Exists(string directory)
        {
            FileAttributes fa = Directory.GetFileAttributes(directory);

            if ((int)fa == -1)
            {
                return false;
            }
            return fa.HasFlag(FileAttributes.FILE_ATTRIBUTE_DIRECTORY);
        }

        /// <summary>
        /// Gets file system entries for the directory provided by the directory argument.  This is not a recursive function only file system entries for the provided directory are returned.
        /// </summary>
        /// <param name="directory">Directory to return results for.</param>
        /// <returns>An array of file system entries from the provided directory.</returns>
        public static string[] GetFileSystemEntries(string directory)
        {
            List<string> results = new List<string>();
            WIN32_FIND_DATA findData;
            IntPtr findHandle = Directory.FindFirstFile(directory + @"\*", out findData);

            if (findHandle != INVALID_HANDLE_VALUE)
            {
                bool found;

                do
                {
                    string currentFileName = findData.cFileName;

                    // if this is a directory, find its contents
                    if (((int)findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
                    {
                        if (currentFileName != "." && currentFileName != "..")
                        {
                            //results.Add(findData.cFileName);
                            results.Add(Path.Combine(directory, currentFileName));
                        }
                    }
                    else
                    {
                        results.Add(findData.cFileName);
                        results.Add(Path.Combine(directory, currentFileName));
                    }

                    // find next
                    found = Directory.FindNextFile(findHandle, out findData);
                }
                while (found);
            }

            // close the find handle
            Directory.FindClose(findHandle);

            return results.ToArray();
        }

        /// <summary>
        /// Create all directories inside path begining at root. path
        /// must be a FullDirectoryName.
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectoriesInPath(string path)
        {
            if (path != null)
                Directory.CreateDirectory(PathUtils.ToLongPath(path));

        }

        public static string FixDirectoryPath(string path)
        {
            if (path.Length == 2) return System.IO.Path.GetPathRoot(path);

            return path.Length > MAX_DIR_PATH ? PreAppendLPP(path) : path;
        }
        public static string FixFilePath(string path)
        {
            return path.Length > MAX_FILE_PATH ? PreAppendLPP(path) : path;
        }
        public static string PreAppendLPP(string path)
        {
            return @"\\?\" + path;
        }
        public static string RemoveLPP(string path)
        {
            return path.Remove(0, 4);
        }

        #region Create Directories in Path clases

        class Lexer
        {
            public string DirFullName { get; set; }
            int i = 0;

            public Lexer(string dirFullName)
            {
                DirFullName = dirFullName;

                Reset();
            }

            public void Reset()
            {
                i = 0;
            }
            public Token NextToken()
            {
                if (DirFullName==null || i > DirFullName.Length)
                    return new Token { Type = 1 };

                SkipSpaces();

                return ReadText();
            }
            private Token ReadText()
            {
                var res = "";

                for (; i < DirFullName.Length && DirFullName[i] != System.IO.Path.DirectorySeparatorChar; i++)
                    res += DirFullName[i];

                return new Token { Text = res, Type = 0 };
            }
            private void SkipSpaces()
            {
                for (; i < DirFullName.Length && DirFullName[i] == System.IO.Path.DirectorySeparatorChar; i++) { }
            }
        }
        class Token
        {
            public string Text;
            /// <summary>
            /// 0 -> Text Token
            /// 1 -> EOF Token
            /// </summary>
            public byte Type;

            public override string ToString()
            {
                return Text;
            }
        }

        #endregion

        #region constants
        public static int MAX_FILE_PATH = 260;
        public static int MAX_DIR_PATH = 248;

        internal const int MAX_PATH = 260;
        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        internal const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        #endregion

        #region externs

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FindClose(IntPtr hFindFile);

        //BOOL WINAPI FindNextFile(__in          HANDLE hFindFile,__out         LPWIN32_FIND_DATA lpFindFileData);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern FileAttributes GetFileAttributes(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CreateDirectory(string lpPathName, IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RemoveDirectory(string lpPathName);

        #endregion

        #region helpers
        private static readonly string slash = System.IO.Path.DirectorySeparatorChar.ToString();
        private static readonly char cslash = System.IO.Path.DirectorySeparatorChar;
        #endregion
        
    }
    
    public class File
    {
        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="file">Path of file to be deleted.  Must begin with \\?\ for local paths or \\?\UNC\ for network paths.</param>
        public static void Delete(string file)
        {
            DeleteFile(file);
        }

        /// <summary>
        /// Opens a file stream to the file specified in the path argument.
        /// </summary>
        /// <param name="path">Path to file to open.  Must begin with \\?\ for local paths or \\?\UNC\ for network paths.</param>
        /// <param name="mode">Specifies how the file is opened or if existing data in the file is retained.</param>
        /// <param name="access">Specifies type of access to the file, read, write or both.</param>
        /// <param name="share">Specifies share access capabilities of subsequent open calls to this file.</param>
        /// <returns>A FileStream to the file specified in the path argument.</returns>
        public static System.IO.FileStream Open(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share,FileDataInfo filedi)
        {
            System.IO.FileStream result = null;

            SafeFileHandle fileHandle = CreateFile(path, getAccessFromAccess(access), getShareFromShare(share), IntPtr.Zero, getDispositionFromMode(mode), 0, IntPtr.Zero);
            int lastWin32Error = Marshal.GetLastWin32Error();
            if (fileHandle.IsInvalid)
            {
                throw new System.ComponentModel.Win32Exception(lastWin32Error);
            }

            result = new System.IO.FileStream(fileHandle, access);
            if (mode == System.IO.FileMode.Append)
            {
                result.Seek(0, System.IO.SeekOrigin.End);
            }
            return result;
        }
        public static bool SetAccesTime(DataInfo dataInfo)
        {
            //I need convert from FILETIME to DataTime
            //and still may not work, when DataInfo have a longPath this  ;
            var lastAccesTime = dataInfo.LastAccessTime;
            System.IO.File.SetLastAccessTime(dataInfo.DestinyPath, DateTime.Now);

            //SafeFileHandle fileHandle = CreateFile(dataInfo.DestinyPath
            //    , getAccessFromAccess(System.IO.FileAccess.Write)
            //    , getShareFromShare(System.IO.FileShare.Write)
            //    , IntPtr.Zero, getDispositionFromMode(System.IO.FileMode.Open)
            //    , 0
            //    , IntPtr.Zero);

            //IntPtr hFile=fileHandle.DangerousGetHandle();

            //bool res = SetFileTime(hFile
            //, dataInfo.CreationTime
            //, dataInfo.LastAccessTime
            //, dataInfo.LastWriteTime);

            return true;
        }

        public static IntPtr CreateFile(string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share)
        {
            SafeFileHandle fileHandle = CreateFileW(path, getAccessFromAccess(access), getShareFromShare(share), IntPtr.Zero, getDispositionFromMode(mode), 0, IntPtr.Zero);
            int lastWin32Error = Marshal.GetLastWin32Error();
            if (fileHandle.IsInvalid)
            {
                throw new System.ComponentModel.Win32Exception(lastWin32Error);
            }

            return fileHandle.DangerousGetHandle();
        }
      
        public static _BY_HANDLE_FILE_INFORMATION GetFileInformation(string file)
        {
            _BY_HANDLE_FILE_INFORMATION fileInfo;
            var path = LongPath.Directory.FixFilePath(file);

            SafeFileHandle fileHandle = CreateFile(path, getAccessFromAccess(FileAccess.Read),
                getShareFromShare(FileShare.None), IntPtr.Zero, getDispositionFromMode(FileMode.Open), 0, IntPtr.Zero);

            File.GetFileInformationByHandle(fileHandle, out fileInfo);

            fileHandle.Close();

            return fileInfo;
        }

        /// <summary>
        /// Check if the specified file exists.
        /// </summary>
        /// <param name="directory">Path of directory to check.  Must begin with \\?\ for local paths or \\?\UNC\ for network paths.</param>
        /// <returns></returns>
        public static bool Exists(string file)
        {
            FileAttributes fa = Directory.GetFileAttributes(file);
            if ((int)fa == -1)
            {
                return false;
            }
            return !fa.HasFlag(FileAttributes.FILE_ATTRIBUTE_DIRECTORY);
        }
        /// <summary>
        /// Copies a file using the managed <see cref="System.IO.File.Copy"/> method.
        /// </summary>
        /// <param name="sourceFile">The source file path.</param>
        /// <param name="destinationFile">The destination file path.</param>
        /// <param name="failIfDestinationExists">If true, the copy fails when the destination exists.</param>
        public static void Copy(string sourceFile, string destinationFile, bool failIfDestinationExists)
        {
            System.IO.File.Copy(sourceFile, destinationFile, !failIfDestinationExists);
        }
        public static void Move(string sourceFile, string destinationFile, bool failIfDestinationExists)
        {
            Copy(sourceFile, destinationFile, failIfDestinationExists);
            Delete(sourceFile);
        }

        private static EFileShare getShareFromShare(System.IO.FileShare share)
        {
            switch (share)
            {
                case System.IO.FileShare.Delete:
                    return EFileShare.Delete;
                case System.IO.FileShare.Inheritable:
                    throw new NotSupportedException("Inheritible is not supported.");
                case System.IO.FileShare.None:
                    return EFileShare.None;
                case System.IO.FileShare.Read:
                    return EFileShare.Read;
                case System.IO.FileShare.ReadWrite:
                    return EFileShare.Read | EFileShare.Write;
                case System.IO.FileShare.Write:
                    return EFileShare.Write;
            }
            throw new NotSupportedException();
        }
        private static EFileAccess getAccessFromAccess(System.IO.FileAccess access)
        {
            switch (access)
            {
                case System.IO.FileAccess.Read:
                    return EFileAccess.GenericRead;
                case System.IO.FileAccess.Write:
                    return EFileAccess.GenericWrite;
                case System.IO.FileAccess.ReadWrite:
                    return EFileAccess.GenericRead | EFileAccess.GenericWrite;
            }
            throw new NotSupportedException();
        }
        private static ECreationDisposition getDispositionFromMode(System.IO.FileMode mode)
        {
            switch (mode)
            {
                case System.IO.FileMode.Create:
                    return  ECreationDisposition.CreateAlways;
                case System.IO.FileMode.CreateNew:
                    return ECreationDisposition.New;
                case System.IO.FileMode.Open:
                    return ECreationDisposition.OpenExisting;
                case System.IO.FileMode.OpenOrCreate:
                    return ECreationDisposition.OpenAlways;
                case System.IO.FileMode.Truncate:
                    return ECreationDisposition.TruncateExisting;
                case System.IO.FileMode.Append:
                    return ECreationDisposition.OpenAlways;
            }
            throw new NotSupportedException();
        }

        #region constants
        public static int MAX_FILE_PATH = 260;
        public static int MAX_DIR_PATH = 248;
        internal static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        internal static int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        #endregion

        #region externs

        //BOOL WINAPI SetFileAttributes(__in          LPCTSTR lpFileName,__in          DWORD dwFileAttributes)
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetFileAttributes(string lpFileName, FileAttributes dwFileAttributes);

//        BOOL WINAPI SetFileTime(
//  __in      HANDLE hFile,
//  __in_opt  const FILETIME *lpCreationTime,
//  __in_opt  const FILETIME *lpLastAccessTime,
//  __in_opt  const FILETIME *lpLastWriteTime
//);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetFileTime(IntPtr hFile, System.Runtime.InteropServices.ComTypes.FILETIME CreationTime
            , System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime
            , System.Runtime.InteropServices.ComTypes.FILETIME lpLastWriteTime);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern FileAttributes GetFileAttributes(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteFile(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFile(
            string lpFileName,
            EFileAccess dwDesiredAccess,
            EFileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            ECreationDisposition dwCreationDisposition,
            EFileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

    //    CreateFileW(
    //_In_ LPCWSTR lpFileName,
    //_In_ DWORD dwDesiredAccess,
    //_In_ DWORD dwShareMode,
    //_In_opt_ LPSECURITY_ATTRIBUTES lpSecurityAttributes,
    //_In_ DWORD dwCreationDisposition,
    //_In_ DWORD dwFlagsAndAttributes,
    //_In_opt_ HANDLE hTemplateFile
    //);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafeFileHandle CreateFileW(
            string lpFileName,
            EFileAccess dwDesiredAccess,
            EFileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            ECreationDisposition dwCreationDisposition,
            EFileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetFileInformationByHandle(SafeFileHandle hFile, out  _BY_HANDLE_FILE_INFORMATION lpFileInformation);

        #endregion

        #region enums
        [Flags]
        public enum EFileAccess : uint
        {
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,
        }
        [Flags]
        public enum EFileShare : uint
        {
            None = 0x00000000,
            Read = 0x00000001,
            Write = 0x00000002,
            Delete = 0x00000004,
        }
        public enum ECreationDisposition : uint
        {
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5,
        }
        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }
        #endregion

        #region structs

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILETIME
        {
            internal uint dwLowDateTime;
            internal uint dwHighDateTime;
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }
     
        #endregion
    }

    #region enums

    [Flags]
    internal enum FileAttributes
    {
        FILE_ATTRIBUTE_READONLY = 1,
        FILE_ATTRIBUTE_HIDDEN = 2,
        FILE_ATTRIBUTE_SYSTEM = 4,
        FILE_ATTRIBUTE_DIRECTORY = 16,
        FILE_ATTRIBUTE_ARCHIVE = 32,
        FILE_ATTRIBUTE_DEVICE = 64,
        FILE_ATTRIBUTE_NORMAL = 128,
        FILE_ATTRIBUTE_TEMPORARY = 256,
        FILE_ATTRIBUTE_SPARSE_FILE = 512,
        FILE_ATTRIBUTE_REPARSE_POINT = 1024,
        FILE_ATTRIBUTE_COMPRESSED = 2048,
        FILE_ATTRIBUTE_OFFLINE = 4096,
        FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 8192,
        FILE_ATTRIBUTE_ENCRYPTED = 16384,
        FILE_ATTRIBUTE_VIRTUAL = 65536
    }
    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct _BY_HANDLE_FILE_INFORMATION
    {
        internal uint dwFileAttributes;
        internal FILETIME ftCreationTime;
        internal FILETIME ftLastAccessTime;
        internal FILETIME ftLastWriteTime;
        internal int dwVolumeSerialNumber;
        internal uint nFileSizeHigh;
        internal uint nFileSizeLow;
        internal uint nNumberOfLinks;
        internal uint nFileIndexHigh;
        internal uint nFileIndexLow;
        internal long Length
        {
            get
            {
                return Windef.MAKELONG(nFileSizeHigh, nFileSizeLow);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WIN32_FIND_DATA
    {
        internal System.IO.FileAttributes dwFileAttributes;
        internal System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        internal uint nFileSizeHigh;
        internal uint nFileSizeLow;
        internal uint dwReserved0;
        internal uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        internal string cFileName;
        // not using this
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        internal string cAlternate;

        internal long Length
        {
            get
            {
                return Windef.MAKELONG(nFileSizeHigh, nFileSizeLow);

                //long n = 0;
                //n = n.SetHightWord(nFileSizeHigh);
                //n = n.SetLowWord(nFileSizeLow);
                //return n;
            }
        }
    }

    #endregion
}
