using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeathCopy.Module2_Configuration.AddDataBehaviour
{
    public class AddToLast : AddData
    {
        public AddToLast() : base("AddToLast") { }
        public override void Execute(IEnumerable<VisualCopy> VisualsCopys)
        {
            var requestInfo = RegisterAccess.Acces.GetLastCopyRequestInfo();

            var first = VisualsCopys.First();
            if (first.RequestInf.Content == RquestContent.None)
            {
                if (first.State == VisualCopy.VisualCopyState.Finished) return;
                Configuration.Main.SetRunningState(first, requestInfo);
            }
            else
            {
                var active = VisualsCopys.Last();
                if (active.State == VisualCopy.VisualCopyState.Finished) return;

                //If operations mach, them AddToCopy.
                if (active.RequestInf.Operation == requestInfo.Operation)
                {
                    //Add items to only active CopyHandle in BackGround
                    Task.Factory.StartNew(() =>
                    {
                        active.AddData(requestInfo, false);
                    });
                }
                else
                {
                    Configuration.Main.SetRunningState(VisualsCopysHandler.MainContainer.AddNew(), requestInfo);
                }
            }

            Configuration.Main.PLaySoundAfterOperation(Configuration.Main.PlaySound_After_ADD_DATA, Configuration.Main.AddData_Sound);
        }
    }
}
