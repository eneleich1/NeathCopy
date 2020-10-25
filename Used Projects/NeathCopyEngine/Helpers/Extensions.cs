using NeathCopyEngine.DataTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NeathCopyEngine.Helpers
{
    public static class Extensions
    {
        /// <summary>
        /// Remove the specific count of elemets from the collection starting at specific startIndex
        /// </summary>
        /// <param name="list"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        public static void RemoveRange(this ObservableCollection<FileDataInfo> list, int startIndex, int count)
        {
            for (int i = startIndex;i < startIndex+count; i++)
            {
                list.RemoveAt(i);
            }
        }
    }
}
