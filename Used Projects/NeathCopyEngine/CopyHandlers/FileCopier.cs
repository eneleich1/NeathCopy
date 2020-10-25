using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using NeathCopyEngine.DataTools;
using System.Threading;
using NeathCopyEngine.Exceptions;

namespace NeathCopyEngine.CopyHandlers
{
    /// <summary>
    /// Allow to copy a file to a destiny path.
    /// </summary>
    public abstract class FileCopier
    {
        /// <summary>
        /// Object For locking. Use this object in your action loop to allow pause: lock (syncObj) { }.
        /// </summary>
        public string Name { get; set; }
        public string Description { get; set; }
        public FileDataInfo CurrentFile { get; protected set; }
        public FileStream Reader { get; protected set; }
        public FileStream Writer { get; protected set; }

        /// <summary>
        /// Get the total bytes transferred of file is been copy.
        /// </summary>
        public long FileBytesTransferred { get; set; }
        /// <summary>
        /// Get the total bytes transferred since this instance was created.
        /// </summary>
        public long TotalBytesTransferred { get; set; }

        /// <summary>
        /// Copy the specific sourceFile to destinyPath new location.
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="destinyPath"></param>
        public abstract void CopyFile(FileDataInfo file);
        public abstract FileCopier Clone();
        public abstract void Cancel();

        public abstract void Skip();
        public override string ToString()
        {
            return Name;
        }
    }
    public enum FileCopyOptions
    {
        OverwriteIfFileExist, SkipIfFileExist, AllwaysSkip, AllwaysOverride, AllwaysAsk, Cancel,OverrideDifferent,AllwaysOverrideDifferent
    }

    /// <summary>
    /// FileCopier based on buffer.
    /// </summary>
    public class BufferFileCopier : FileCopier
    {
        protected int bufferSize;
        public int BufferSize
        {
            get
            {
                return bufferSize;
            }
            set
            {
                bufferSize = value;
                buffer = new byte[bufferSize];
            }
        }
      
        protected byte[] buffer;
        protected int readBytes;

        public BufferFileCopier(int bufferSize)
            : base()
        {
            BufferSize = bufferSize;
            buffer = new byte[BufferSize];

            Name = "BufferFileCopier";
        }

        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;

            Reader = file.GetStreamToRead();
            Writer = file.GetStreamToWrite(FileMode.Create);

            readBytes = 0;
            FileBytesTransferred = 0;

            unsafe
            {
                do
                {
                    //Dinamic bufferSize. Dynamic buffer size.
                    readBytes = Reader.Read(buffer, 0, (int)(file.Size>BufferSize?BufferSize:file.Size));
                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;

                } while (readBytes > 0);
            }

            Reader.Close();
            Reader.Dispose();

            Writer.Flush();
            Writer.Close();
            Writer.Dispose();

            Writer = null;

        }

        public override FileCopier Clone()
        {
            return new BufferFileCopier(BufferSize);
        }

        public virtual void ReleaseResources()
        {
            Reader.Dispose();

            Monitor.Enter(Writer);

            if (Writer != null && Writer.CanWrite)
            {
                //Writer.SetLength(FileBytesTransferred);
                Writer.Flush();
                Writer.Close();
            }

            Monitor.Exit(Writer);

            if (CurrentFile.CopyState == CopyState.Processing)
                System.IO.File.Delete(CurrentFile.DestinyPath);
        }

        public override void Cancel()
        {
            try
            {
                //Monitor.Enter(Writer);

                if (CurrentFile == null) return;

                //If currentFile is not equals null but copy have finished.
                //This Produce exception: Can not acces to a closed file.

                Writer.SetLength(FileBytesTransferred);
                Writer.Flush();
                Writer.Close();

                //Monitor.Exit(Writer);
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.Message + " in FileCopier.Cancel");
            }

            finally
            {
                Alphaleonis.Win32.Filesystem.File.Delete(CurrentFile.DestinyPath);
            }

        }

        public override void Skip()
        {
            if (CurrentFile == null) return;

            try
            {
                //If currentFile is not equals null but copy have finished.
                //This Produce exception: Can not acces to a closed file.
                if (Writer != null && Writer.CanWrite)
                {
                    Writer.SetLength(FileBytesTransferred);
                    Writer.Flush();
                    Writer.Close();
                }
            }
            catch (Exception) { }
            finally
            {
                CurrentFile = null;
            }
        }

    }
    /// <summary>
    /// FileCopier based on ProducerConsumer threading framework.
    /// </summary>
    public class ProducerConsumerFileCopier : BufferFileCopier
    {
        int buffersListCapacity = 1;
        bool producerFinish = false;
        bool finish = false;
        Queue<Buffer_Length> queve = new Queue<Buffer_Length>();
        static object thisobject = new Object();

        public ProducerConsumerFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "ProducerConsumerFileCopier";
        }
        void Producer_ReadBytes(FileDataInfo file)
        {
            int length = 0;
            var reader = file.GetStreamToRead();

            while (reader.Position<reader.Length)
            {
                if (queve.Count < buffersListCapacity)
                {
                    lock (thisobject)
                    {
                        buffer = new byte[BufferSize];
                        length = reader.Read(buffer, 0, BufferSize);
                        queve.Enqueue(new Buffer_Length(buffer, length));
                    }
                }
            }

            producerFinish = true;
        }
        void Consumer_WriteBytes(FileDataInfo file)
        {
            Buffer_Length bl;
            var writer = file.GetStreamToWrite(FileMode.Create);

            while (!(queve.Count == 0 && producerFinish))
            {
                if (queve.Count > 0)
                {
                    lock (thisobject)
                    {
                        bl = queve.Dequeue();
                    }

                    writer.Write(bl.buffer, 0, bl.length);
                    FileBytesTransferred += bl.length;
                    TotalBytesTransferred += bl.length;
                }
            }

            finish = true;
        }
        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;

            finish = false;
            producerFinish = false;
            FileBytesTransferred = 0;

            Task.Factory.StartNew(() => Producer_ReadBytes(file));
            Task.Factory.StartNew(() => Consumer_WriteBytes(file));

            while (!finish) { }
        }
        public override void ReleaseResources()
        {
            Reader.Dispose();
            Writer.Dispose();
        }
        public override FileCopier Clone()
        {
            return new ProducerConsumerFileCopier(BufferSize);
        }

        struct Buffer_Length
        {
            public byte[] buffer;
            public int length;

            public Buffer_Length(byte[] buffer_2, int length_2)
            {
                // TODO: Complete member initialization
                this.buffer = buffer_2;
                this.length = length_2;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class BufferBinaryWriterFileCopier : BufferFileCopier
    {
        public BufferBinaryWriterFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "BufferBinaryWriterFileCopier";
        }

        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;
            Reader = file.GetStreamToRead();
            Writer = file.GetStreamToWrite(FileMode.Create);
            var writer1 = new BinaryWriter(Writer);

            readBytes = 0;
            FileBytesTransferred = 0;

            unsafe
            {
                do
                {
                    //Dinamic bufferSize. Dynamic buffer size.
                    readBytes = Reader.Read(buffer, 0, (int)(file.Size > BufferSize ? BufferSize : file.Size));
                    writer1.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;

                } while (readBytes > 0);
            }

            Reader.Close();
            Reader.Dispose();

            Writer.Flush();
            Writer.Close();
            Writer.Dispose();
        }
        public override FileCopier Clone()
        {
            return new BufferBinaryWriterFileCopier(BufferSize);
        }

    }
    /// <summary>
    /// Recomended 1 Mb of BufferSize.
    /// Based on FileStream.
    /// SetLength of new FileStream.
    /// Remove Flush call, use only dispose.
    /// If source & destination is on different HDDs, then reading and writing are processed respectively in parallel by separate threads.
    /// But if they are on the same HDDs, then reading is processed until the user-defined buffer size is filled. Once filled, data can be written anywhere immediately.
    /// </summary>
    public class FasterBufferFileCopier : BufferFileCopier
    {
        Alphaleonis.Win32.Filesystem.DriveInfo driverInfo;
        string fileName;
        public FasterBufferFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "FasterBufferFileCopier";
        }

        public override void ReleaseResources()
        {
            Reader.Dispose();
            Writer.Dispose();

            //if (CurrentFile != null)
            //    File.Delete(CurrentFile.DestinyPath);
        }
        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;
            fileName = Delimon.Win32.IO.Path.GetPathRoot(file.DestinyPath);
            
            driverInfo = new Alphaleonis.Win32.Filesystem.DriveInfo(fileName);

            using (Reader = file.GetStreamToRead())
            {
                using (Writer = file.GetStreamToWrite(FileMode.Create))
                    {
                        //if (driverInfo.DriveType == DriveType.Removable && driverInfo.DriveFormat=="FAT32")
                        //{
                        //    ExecuteCopySafe(driverInfo, file);
                        //}
                        //else ExecuteCopy(driverInfo, file);

                        ExecuteCopy(driverInfo, file);
                    }
                //}
            }

        }

        /// <summary>
        /// Execute copy without set the file length before copy.
        /// </summary>
        /// <param name="driverInfo"></param>
        /// <param name="file"></param>
        protected virtual void ExecuteCopySafe(Alphaleonis.Win32.Filesystem.DriveInfo driverInfo, FileDataInfo file)
        {
            if (driverInfo.TotalFreeSpace < file.Size)
            {
                ReleaseResources();

                throw new NotEnoughSpaceException(string.Format("{0} ({1})", driverInfo.VolumeLabel, driverInfo.Name));
            }

            //Writer.SetLength(Reader.Length);

            readBytes = 0;
            FileBytesTransferred = 0;

            while ((readBytes = Reader.Read(buffer, 0, BufferSize)) > 0)
            {
                // Request the lock, and block until it is obtained.
                Monitor.Enter(Writer);

                //If operation was cancelled, them the writer has been released
                //and this statement produce: The Thread has been aborted exception.
                //Them capture the exception and terminate the current file copy process.
                //try
                //{
                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;
                //}
                //catch (Exception ex)
                //{
                //    System.Windows.MessageBox.Show(ex.Message + " in FileCopier.Copy");
                //}

                // Ensure that the lock is released.
                Monitor.Exit(Writer);

            }

            CurrentFile = null;
        }

        Exception executeCopyException;
        protected virtual void ExecuteCopy(Alphaleonis.Win32.Filesystem.DriveInfo driverInfo, FileDataInfo file)
        {
            if (driverInfo.TotalFreeSpace < file.Size)
            {
                ReleaseResources();

                throw new NotEnoughSpaceException(string.Format("{0} ({1})", driverInfo.VolumeLabel, driverInfo.Name));
            }

            Writer.SetLength(Reader.Length);

            readBytes = 0;
            FileBytesTransferred = 0;

            executeCopyException = null;

            try
            {

                while ((readBytes = Reader.Read(buffer, 0, BufferSize)) > 0)
                {
                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;
                }

            }
            catch (Exception ex)
            {
                executeCopyException = ex;
            }
            finally
            {
                ReleaseResources();
                CurrentFile = null;
                if(executeCopyException!=null)throw executeCopyException;
            }
        }
        public override FileCopier Clone()
        {
            return new FasterBufferFileCopier(BufferSize);
        }
    }


    /// <summary>
    /// Change the buffer dynamic.
    /// </summary>
    public class DynamicBufferFileCopier : FasterBufferFileCopier
    {
        public DynamicBufferFileCopier(int bufferSize)
           : base(bufferSize)
        {
            Name = "DynamicBufferFileCopier";
        }

        protected override void ExecuteCopy(Alphaleonis.Win32.Filesystem.DriveInfo driverInfo, FileDataInfo file)
        {
            buffer = new byte[CalculateBufferSize(file.Size)];
            base.ExecuteCopy(driverInfo, file);
        }

        protected virtual int CalculateBufferSize(long size)
        {
            //return (int)Math.Log(size, 2) + 1;
            return 4*1024*1024;//1Mb
        }
    }

    /// <summary>
    /// Best Gain Copy Performance by Mdonatas
    /// </summary>
    public class MdonatasBufferFileCopier : BufferFileCopier
    {
        byte[] bufferForWrite, bufferForRead;

        public MdonatasBufferFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "MdonatasBufferFileCopier";

            bufferForWrite = new byte[bufferSize];
            bufferForRead = new byte[bufferSize];
        }

        public override void ReleaseResources()
        {
            Reader.Dispose();
            Writer.Dispose();

            if (CurrentFile != null)
                System.IO.File.Delete(CurrentFile.DestinyPath);
        }
        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;
            var driverInfo = new System.IO.DriveInfo(Delimon.Win32.IO.Path.GetPathRoot(file.DestinyPath));
            string tmp_file = "";

            using (Reader = file.GetStreamToRead())
            {
                if (file.DestinyPath.Length > 248)
                {
                    //Create the file in short name
                    tmp_file = Delimon.Win32.IO.Path.Combine(file.DestinyDirectoryPath, "tmp_a19");

                    using (Writer = new FileStream(tmp_file, FileMode.Create, FileAccess.Write))
                    {
                        //if (driverInfo.DriveType == DriveType.Removable)
                        //{
                        //    ExecuteCopySafe(driverInfo, file);
                        //}
                        //else ExecuteCopy(driverInfo, file);
                        ExecuteCopy(driverInfo, file);
                    }

                    //Move the file to it's original destinyPath
                    if (Delimon.Win32.IO.File.Exists(file.DestinyPath))
                        Delimon.Win32.IO.File.Delete(file.DestinyPath);
                    Delimon.Win32.IO.File.Move(tmp_file, file.DestinyPath);
                }
                else
                {
                    using (Writer = file.GetStreamToWrite(FileMode.Create))
                    {
                        //if (driverInfo.DriveType == DriveType.Removable && driverInfo.DriveFormat=="FAT32")
                        //{
                        //    ExecuteCopySafe(driverInfo, file);
                        //}
                        //else ExecuteCopy(driverInfo, file);

                        ExecuteCopy(driverInfo, file);
                    }
                }
            }

        }

        /// <summary>
        /// Execute copy without set the file length before copy.
        /// </summary>
        /// <param name="driverInfo"></param>
        /// <param name="file"></param>
        private void ExecuteCopySafe(System.IO.DriveInfo driverInfo, FileDataInfo file)
        {
            if (driverInfo.TotalFreeSpace < file.Size)
            {
                ReleaseResources();

                throw new NotEnoughSpaceException(string.Format("{0} ({1})", driverInfo.VolumeLabel, driverInfo.Name));
            }

            //Writer.SetLength(Reader.Length);

            readBytes = 0;
            FileBytesTransferred = 0;

            while ((readBytes = Reader.Read(buffer, 0, BufferSize)) > 0)
            {
                // Request the lock, and block until it is obtained.
                Monitor.Enter(Writer);

                //If operation was cancelled, them the writer has been released
                //and this statement produce: The Thread has been aborted exception.
                //Them capture the exception and terminate the current file copy process.
                try
                {
                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;
                }
                catch (Exception ex)
                {
                    return;
                }

                // Ensure that the lock is released.
                Monitor.Exit(Writer);

            }

            CurrentFile = null;
        }
        private void ExecuteCopy(System.IO.DriveInfo driverInfo, FileDataInfo file)
        {
            if (driverInfo.TotalFreeSpace < file.Size)
            {
                ReleaseResources();

                throw new NotEnoughSpaceException(string.Format("{0} ({1})", driverInfo.VolumeLabel, driverInfo.Name));
            }

            Writer.SetLength(Reader.Length);

            readBytes = 0;
            FileBytesTransferred = 0;

            while ((readBytes = Reader.Read(buffer, 0, BufferSize)) > 0)
            {
                // Request the lock, and block until it is obtained.
                Monitor.Enter(Writer);

                //If operation was cancelled, them the writer has been released
                //and this statement produce: The Thread has been aborted exception.
                //Them capture the exception and terminate the current file copy process.
                try
                {
                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;
                }
                catch (Exception ex)
                {
                    return;
                    //MessageBox.Show(ex.Message + " in FileCopier.Copy");
                }

                // Ensure that the lock is released.
                Monitor.Exit(Writer);

            }

            CurrentFile = null;
        }
        public override FileCopier Clone()
        {
            return new FasterBufferFileCopier(BufferSize);
        }
    }
    /// <summary>
    /// Only Copy the files, not it's content
    /// </summary>
    public class NotCopyFileCopier : FasterBufferFileCopier
    {
        public NotCopyFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "NotCopyFileCopier";
        }

        protected override void ExecuteCopySafe(Alphaleonis.Win32.Filesystem.DriveInfo driverInfo, FileDataInfo file)
        {
            CurrentFile = null;
        }
        protected override void ExecuteCopy(Alphaleonis.Win32.Filesystem.DriveInfo driverInfo, FileDataInfo file)
        {
            CurrentFile = null;
        }
        public override FileCopier Clone()
        {
            return new NotCopyFileCopier(BufferSize);
        }
    }

    public class SaveBufferFileCopier : BufferFileCopier
    {
        public SaveBufferFileCopier(int bufferSize)
            : base(bufferSize)
        {
            Name = "SaveBufferFileCopier";
        }

        public override void CopyFile(FileDataInfo file)
        {
            CurrentFile = file;
            Reader = file.GetStreamToRead();

            using (Writer = file.GetStreamToWrite(FileMode.Create))
            {
                readBytes = 0;
                FileBytesTransferred = 0;

                while ((readBytes = Reader.Read(buffer, 0, BufferSize)) > 0)
                {
                    Writer.SetLength(Writer.Length + readBytes);

                    Writer.Write(buffer, 0, readBytes);

                    //Status
                    FileBytesTransferred += readBytes;
                    TotalBytesTransferred += readBytes;

                }
            }

            Reader.Dispose();
        }
        public override FileCopier Clone()
        {
            return new SaveBufferFileCopier(BufferSize);
        }
    }
}