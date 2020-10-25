using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeathCopyEngine.CopyHandlers
{
    public abstract class Error
    {
        public string Message { get; set; }
        public int Code { get; protected set; }

        public static string GetErrorLog(string message,string module,string className,string method)
        {
            return string.Format("Message: {0}{4}Module: {1}{4}Class: {2}{4}Method: {3}"
                , message, module, className, method, Environment.NewLine);
        }
        public static string GetErrorLogInLine(string message, string module, string className, string method)
        {
            return string.Format("Message: {0}{4}Module: {1}{4}Class: {2}{4}Method: {3}"
                , message, module, className, method, ' ');
        }
    }

    public class CopyError : Error
    {
        public CopyError()
        {
            Code = 1;
        }
    }

    public class DiskFullError : Error
    {
        public DiskFullError()
        {
            Code = 2;
        }
    }

    public class FileNotExistError : Error
    {
        public FileNotExistError()
        {
            Code = 3;
        }
    }
    public class DirectoryNotExistError : Error
    {
        public DirectoryNotExistError()
        {
            Code = 4;
        }
    }
}
