using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeathCopy.Module2_Configuration.AddDataBehaviour
{
    /// <summary>
    /// Abstract class for add data to files list behaviour.
    /// </summary>
    public abstract class AddData
    {
        public string Name { get; set; }

        public AddData(string name)
        {
            Name = name;
        }

        public abstract void Execute(IEnumerable<VisualCopy> VisualsCopys);
    }
}
