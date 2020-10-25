using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace NeathCopyEngine.Helpers
{
    public class OnFinishFuncitons
    {
        #region Extern Functions

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
             string lpFileName,
             uint dwDesiredAccess,
             uint dwShareMode,
             IntPtr SecurityAttributes,
             uint dwCreationDisposition,
             uint dwFlagsAndAttributes,
             IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            byte[] lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );

        #endregion

        #region Constants

        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const uint FILE_SHARE_READ = 0x1;
        const uint FILE_SHARE_WRITE = 0x2;
        const uint FSCTL_LOCK_VOLUME = 0x00090018;
        const uint FSCTL_DISMOUNT_VOLUME = 0x00090020;
        const uint IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
        const uint IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804;

        #endregion

        #region Auxiliars Methods

        private static bool ConfirmarionDialog(IntPtr handle)
        {
            uint byteReturned;

            if (DeviceIoControl(handle, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero))
                return MessageBox.Show("Are you shure do you want eject device", "Confirmation Dialog", MessageBoxButton.YesNo) == MessageBoxResult.Yes;

            //Thread.Sleep(500);

            return false;
        }

        private static bool PreventRemovalOfVolume(IntPtr handle, bool prevent)
        {
            byte[] buf = new byte[1];
            uint retVal;

            buf[0] = (prevent) ? (byte)1 : (byte)0;
            return DeviceIoControl(handle, IOCTL_STORAGE_MEDIA_REMOVAL, buf, 1, IntPtr.Zero, 0, out retVal, IntPtr.Zero);
        }

        private static bool DismountVolume(IntPtr handle)
        {
            uint byteReturned;
            return DeviceIoControl(handle, FSCTL_DISMOUNT_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
        }

        private static bool AutoEjectVolume(IntPtr handle)
        {
            uint byteReturned;
            return DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
        }

        private static bool CloseVolume(IntPtr handle)
        {
            return CloseHandle(handle);
        }

        #endregion

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private static IntPtr handle = IntPtr.Zero;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driveLetter">This should be the drive letter. Format: F:</param>
        /// <param name="confirmationDlg">Specific if a confirmation dialog will displayed</param>
        /// <returns></returns>
        public static bool Eject(string driveLetter, bool confirmationDlg, DriveType driveType)
        {
            if (driveType != DriveType.Removable) return false;

            string filename = @"\\.\" + driveLetter[0] + ":";
            handle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);

            if (confirmationDlg)
            {
                if (ConfirmarionDialog(handle) && DismountVolume(handle))
                {
                    PreventRemovalOfVolume(handle, false);
                    return AutoEjectVolume(handle);
                }
            }
            else
            {
                if (DismountVolume(handle))
                {
                    PreventRemovalOfVolume(handle, false);
                    return AutoEjectVolume(handle);
                }
            }

            CloseHandle(handle);
            return false;
        }

        public static void Hibernate()
        {
            SetSuspendState(true, true, true);
        }

        public static void Sleep()
        {
            SetSuspendState(false, true, true);
        }

        public static void Shutdown()
        {
            Process.Start("shutdown", "/s /t 0");
        }
    }
}
