using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeathCopyEngine.Exceptions
{
    public class InvalidCopyOperationException : Exception
    {
        public InvalidCopyOperationException(string invalidOperation) : base(string.Format("The operation: {0} is invalid, try copy or move instead",invalidOperation))
        {
        }
    }
}
