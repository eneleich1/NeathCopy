using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeathCopyEngine.Exceptions
{
    class FileSystemNotExistException:Exception
    {
        public FileSystemNotExistException(string fileSystemName)
            : base(string.Format("The File or Directory {0} do not exist", fileSystemName))
        {
        }
    }
}
