using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace WinApiMembers
{
    public class Kernel32
    {
        #region Callback Functions Declarations

         public delegate int CopyProgressRoutine(
            long TotalFileSize,
            long TotalBytesTransferred,
            long StreamSize,
            long StreamBytesTransferred,
            int StreamNumber,
            int CallbackReason,
            IntPtr hSourceFile,
            IntPtr hDestinationFile,
            IntPtr lpData
            );

         //        DWORD CALLBACK CopyProgressRoutine(
         //  __in      LARGE_INTEGER TotalFileSize,
         //  __in      LARGE_INTEGER TotalBytesTransferred,
         //  __in      LARGE_INTEGER StreamSize,
         //  __in      LARGE_INTEGER StreamBytesTransferred,
         //  __in      DWORD dwStreamNumber,
         //  __in      DWORD dwCallbackReason,
         //  __in      HANDLE hSourceFile,
         //  __in      HANDLE hDestinationFile,
         //  __in_opt  LPVOID lpData
         //);

        #endregion
       
        #region Externs Functions

        [SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern unsafe IntPtr CopyFileEx
        (
            string ExistingFileName,
            string NewFileName,
            CopyProgressRoutine ProgressRoutine,
            IntPtr Data,
            ref bool Cancel,
            CopyFlags CopyFlags
        );

        //        BOOL WINAPI CopyFileEx(
        //  __in      LPCTSTR lpExistingFileName,
        //  __in      LPCTSTR lpNewFileName,
        //  __in_opt  LPPROGRESS_ROUTINE lpProgressRoutine,
        //  __in_opt  LPVOID lpData,
        //  __in_opt  LPBOOL pbCancel,
        //  __in      DWORD dwCopyFlags
        //);

        #endregion

        #region Enums

        [Flags]
        public enum CopyFlags
        {
            COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008,
            COPY_FILE_COPY_SYMLINK = 0x00000800,
            COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
            COPY_FILE_NO_BUFFERING = 0x00001000,
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
            COPY_FILE_RESTARTABLE = 0x00000002
        }

        #endregion
    }

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

        [System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError = true)]
        public static extern unsafe IntPtr CopyFile
        (
            string ExistingFileName,
            string NewFileName,
            bool FailIfExists
        );

        //[System.Runtime.InteropServices.DllImport("Kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern unsafe IntPtr CopyFileEx
        (
            string ExistingFileName,
            string NewFileName,
            CopyProgressRoutine ProgressRoutine,
            IntPtr Data,
            ref bool Cancel,
            CopyFlags CopyFlags
        );

        //        BOOL WINAPI CopyFileEx(
        //  __in      LPCTSTR lpExistingFileName,
        //  __in      LPCTSTR lpNewFileName,
        //  __in_opt  LPPROGRESS_ROUTINE lpProgressRoutine,
        //  __in_opt  LPVOID lpData,
        //  __in_opt  LPBOOL pbCancel,
        //  __in      DWORD dwCopyFlags
        //);

        public delegate int CopyProgressRoutine(
            long TotalFileSize,
            long TotalBytesTransferred,
            long StreamSize,
            long StreamBytesTransferred,
            int StreamNumber,
            int CallbackReason,
            IntPtr hSourceFile,
            IntPtr hDestinationFile,
            IntPtr lpData
            );

        //        DWORD CALLBACK CopyProgressRoutine(
        //  __in      LARGE_INTEGER TotalFileSize,
        //  __in      LARGE_INTEGER TotalBytesTransferred,
        //  __in      LARGE_INTEGER StreamSize,
        //  __in      LARGE_INTEGER StreamBytesTransferred,
        //  __in      DWORD dwStreamNumber,
        //  __in      DWORD dwCallbackReason,
        //  __in      HANDLE hSourceFile,
        //  __in      HANDLE hDestinationFile,
        //  __in_opt  LPVOID lpData
        //);


        #endregion

        #region Enums

        /// <summary>
        /// Define CopyFile and CopyFileEx option flags.
        /// </summary>
        [Flags]
        public enum CopyFlags
        {
            COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008,
            COPY_FILE_COPY_SYMLINK = 0x00000800,
            COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
            COPY_FILE_NO_BUFFERING = 0x00001000,
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
            COPY_FILE_RESTARTABLE = 0x00000002
        }

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
