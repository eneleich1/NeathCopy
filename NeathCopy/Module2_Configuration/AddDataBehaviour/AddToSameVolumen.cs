using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeathCopy.Module2_Configuration.AddDataBehaviour
{
    public class AddToSameVolumen : AddData
    {
        public AddToSameVolumen() : base("AddToSameVolumen") { }
        public override void Execute(IEnumerable<VisualCopy> VisualsCopys)
        {
            var info = RegisterAccess.Acces.GetLastCopyRequestInfo();

            var first = VisualsCopys.First();
            if (first.RequestInf.Content == RquestContent.None)
            {
                if (first.State == VisualCopy.VisualCopyState.Finished) return;
                Configuration.Main.SetRunningState(first, info);
            }
            else
            {
                var visuals = VisualsCopys.Where(v => Path.GetPathRoot(v.RequestInf.Destiny) == Path.GetPathRoot(info.Destiny)
                && v.RequestInf.Operation == info.Operation && v.State != VisualCopy.VisualCopyState.Finished);

                if (visuals != null && visuals.Count() > 0)
                {
                    //Add items to only active CopyHandle in Background
                    visuals.First().AddData(info, false);
                }
                else
                {
                    Configuration.Main.SetRunningState(Configuration.Main.AddNewVisualCopy(), info);
                }
            }

            Configuration.Main.PLaySoundAfterOperation(Configuration.Main.PlaySound_After_ADD_DATA, Configuration.Main.AddData_Sound);
        }
    }
}
