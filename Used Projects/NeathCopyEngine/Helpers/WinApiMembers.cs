using System;
using System.Runtime.InteropServices;

namespace WinApiMembers
{
    /// <summary>
    /// Provides P/Invoke signatures for basic Win32 file operations.
    /// Native copy routines have been replaced by managed equivalents.
    /// </summary>
    public class FileCopyMembers
    {
        #region Constants

        #region GenericRghts: The requested access to the file or device, which can be summarized as read, write, both or neither (zero).

        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_EXECUTE = 0x20000000;
        public const uint GENERIC_ALL = 0x10000000;

        #endregion

        #endregion

        #region Externs Functions

        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern unsafe System.IntPtr CreateFile
        (
            string FileName,          // file name
            uint DesiredAccess,       // access mode
            uint ShareMode,           // share mode
            uint SecurityAttributes,  // Security Attributes
            CreationDispotition CreationDisposition, // how to create
            uint FlagsAndAttributes,  // file attributes
            int hTemplateFile         // handle to template file
        );

        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
        public static extern unsafe bool ReadFile
        (
            System.IntPtr hFile,      // handle to file
            void* pBuffer,            // data buffer
            int NumberOfBytesToRead,  // number of bytes to read
            int* pNumberOfBytesRead,  // number of bytes read
            int Overlapped            // overlapped buffer
        );

        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
        public static extern unsafe bool WriteFile
        (
            System.IntPtr hFile,      // handle to file
            void* pBuffer,            // data buffer
            int NumberOfBytesToRead,  // number of bytes to write
            int* pNumberOfBytesRead,  // number of bytes written
            int Overlapped            // overlapped buffer
        );

        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
        public static extern unsafe bool CloseHandle
        (
            System.IntPtr hObject // handle to object
        );

        #endregion

        #region Enums
        /// <summary>
        /// An action to take on a file or device that exists or does not exist.
        /// </summary>
        public enum CreationDispotition
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXISTING = 5
        }

        #endregion

        #region Structs


        #endregion
    }
}
