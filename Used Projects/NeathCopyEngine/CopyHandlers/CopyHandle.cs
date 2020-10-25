using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeathCopyEngine.Helpers;
using System.IO;
using System.Collections;

namespace NeathCopyEngine.CopyHandlers
{
    public abstract class CopyHandle
    {

        /// <summary>
        /// Get the current CopyHandle'state.
        /// </summary>
        public CopyHandleState State { get; protected set; }
        /// <summary>
        /// Allow copy a file to a new location.
        /// </summary>
        public FileCopier FileCopier;
        /// <summary>
        /// Get an object that represent the list of all files to copy or move.
        /// Olso this contains the EmptyDirectories list.
        /// </summary>
        public FilesList DiscoverdList { get; protected set; }
        /// <summary>
        /// Get the list of errors ocurred in a specific operation
        /// </summary>
        public List<Error> Errors { get; protected set; }

        public CopyHandle()
        {
            State = CopyHandleState.NotStarted;
            DiscoverdList = new FilesList();
            Errors = new List<Error>();
        }

        /// <summary>
        /// Copy all files in DiscoverdList.
        /// </summary>
        public abstract void Copy();
        /// <summary>
        ///Move all files in DiscoverdList.
        /// </summary>
        public abstract void Move();
        /// <summary>
        /// Move all files from the DiscoverdList to the specific folder in 
        /// the same drive by renaming instead copiying and deleting
        /// </summary>
        public abstract void FastMove();

        /// <summary>
        /// Perform a FastMove operation based in requestInfo param.
        /// A FastMove is a Move to the same root.
        /// </summary>
        /// <param name="requestInfo"></param>
        public static void FastMove(RequestInfo requestInfo)
        {
            FileInfo finfo=null;
            DirectoryInfo dinfo=null;

            foreach (var source in requestInfo.Sources)
            {
                if (File.Exists(source))
                {
                    finfo = new FileInfo(source);
                    File.Move(source, Path.Combine(requestInfo.Destiny, finfo.Name));
                }
                else if (Directory.Exists(source))
                {
                    dinfo = new DirectoryInfo(source);
                    Directory.Move(source, Path.Combine(requestInfo.Destiny, dinfo.Name));
                }
            }
        }

    }

    public enum CopyHandleState
    {
        None,NotStarted,Runing,Paused,Canceled,Finished
    }
}
