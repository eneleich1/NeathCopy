using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeathCopyEngine.Exceptions
{
    public class NotEnoughSpaceException:Exception
    {
        public NotEnoughSpaceException(string driver):base("There is not enough free space on driver "+driver)
        {
        }
    }
}
