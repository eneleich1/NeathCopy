using NeathCopy.Services;
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
            RequestInfo requestInfo;
            if (string.Equals(Configuration.Main.IntegrationMode, IntegrationManager.LegacyMode, StringComparison.OrdinalIgnoreCase))
            {
                requestInfo = RegisterAccess.Acces.GetLastCopyRequestInfo();
            }
            else
            {
                requestInfo = RegisterAccess.Acces.TryConsumePendingCopyRequestInfo()
                    ?? RegisterAccess.Acces.GetLastCopyRequestInfo();
            }
            if (requestInfo == null)
                return;

            var visualsList = VisualsCopys == null ? new List<VisualCopy>() : VisualsCopys.Where(v => v != null).ToList();
            if (visualsList.Count == 0)
            {
                var created = Configuration.Main.AddNewVisualCopy();
                if (created != null)
                    Configuration.Main.SetRunningState(created, requestInfo);

                Configuration.Main.PLaySoundAfterOperation(Configuration.Main.PlaySound_After_ADD_DATA, Configuration.Main.AddData_Sound);
                return;
            }

            var first = visualsList.First();
            if (first.RequestInf.Content == RquestContent.None)
            {
                if (first.State == VisualCopy.VisualCopyState.Finished) return;
                Configuration.Main.SetRunningState(first, requestInfo);
            }
            else
            {
                var active = visualsList.Last();
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
                    var empty = visualsList.FirstOrDefault(v => v.RequestInf == null || v.RequestInf.Content == RquestContent.None);
                    if (empty != null && empty.State != VisualCopy.VisualCopyState.Finished)
                        Configuration.Main.SetRunningState(empty, requestInfo);
                    else
                        Configuration.Main.SetRunningState(Configuration.Main.AddNewVisualCopy(), requestInfo);
                }
            }

            Configuration.Main.PLaySoundAfterOperation(Configuration.Main.PlaySound_After_ADD_DATA, Configuration.Main.AddData_Sound);
        }
    }
}

