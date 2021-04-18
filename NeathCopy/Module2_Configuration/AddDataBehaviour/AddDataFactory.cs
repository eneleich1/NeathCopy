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
            AddDataBehaviours = new List<string> { "AddToFirsth", "AddToLast", "AddToSameDestiny", "AddToSameVolumen" };
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
                case "AddToSameVolumen":
                    return new AddToLast();
                default:return new AddToSameVolumen();
            }
        }

    }
}
