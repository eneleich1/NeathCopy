using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeathCopy.Module2_Configuration.AddDataBehaviour
{
    public class AddDataFactory
    {
        public List<string> AddDataBehaviours { get; set; }

        public AddDataFactory()
        {
            AddDataBehaviours = new List<string> { "AddToFirsth", "AddToLast", "AddToSameDestiny", "SameVolumen_Process_ADD_DATA" };
        }
        public AddData GetDefaultBehaviour()
        {
            return new AddToSameVolumen();
        }
        public AddData GetBehaviour(string name)
        {
            switch (name)
            {
                case "AddToFirsth":
                    return new AddToFirsth();
                case "AddToLast":
                    return new AddToLast();
                case "AddToSameDestiny":
                    return new AddToSameDestiny();
                case "SameVolumen_Process_ADD_DATA":
                    return new AddToSameVolumen();
                default:return new AddToSameVolumen();
            }
        }

    }
}
