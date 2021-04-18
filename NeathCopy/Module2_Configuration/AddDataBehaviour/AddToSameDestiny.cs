using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeathCopy.Module2_Configuration.AddDataBehaviour
{
    public class AddToSameDestiny : AddData
    {
        public AddToSameDestiny():base("AddToSameDestiny") { }
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
                var visuals = VisualsCopys.Where(v => v.RequestInf.Destiny == info.Destiny
                && v.RequestInf.Operation == info.Operation && v.State != VisualCopy.VisualCopyState.Finished);

                if (visuals != null && visuals.Count() > 0)
                {
                    //Add items to only active CopyHandle in Background
                    Task.Factory.StartNew(() =>
                    {
                        visuals.First().AddData(info, false);
                    });
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
