using NeathCopy.Services;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
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
            RequestInfo info;
            if (string.Equals(Configuration.Main.IntegrationMode, IntegrationManager.LegacyMode, StringComparison.OrdinalIgnoreCase))
            {
                info = RegisterAccess.Acces.GetLastCopyRequestInfo();
            }
            else
            {
                info = RegisterAccess.Acces.TryConsumePendingCopyRequestInfo()
                    ?? RegisterAccess.Acces.GetLastCopyRequestInfo();
            }
            if (info == null)
                return;

            var visualsList = VisualsCopys == null ? new List<VisualCopy>() : VisualsCopys.Where(v => v != null).ToList();
            if (visualsList.Count == 0)
            {
                var created = Configuration.Main.AddNewVisualCopy();
                if (created != null)
                    Configuration.Main.SetRunningState(created, info);

                Configuration.Main.PLaySoundAfterOperation(Configuration.Main.PlaySound_After_ADD_DATA, Configuration.Main.AddData_Sound);
                return;
            }

            var first = visualsList.First();
            if (first.RequestInf.Content == RquestContent.None)
            {
                if (first.State == VisualCopy.VisualCopyState.Finished) return;
                Configuration.Main.SetRunningState(first, info);
            }
            else
            {
                var visuals = visualsList.Where(v => PathDisplayHelper.GetRootForDriveInfo(v.RequestInf.Destiny) == PathDisplayHelper.GetRootForDriveInfo(info.Destiny)
                && v.RequestInf.Operation == info.Operation && v.State != VisualCopy.VisualCopyState.Finished);

                if (visuals != null && visuals.Count() > 0)
                {
                    //Add items to only active CopyHandle in Background
                    visuals.First().AddData(info, false);
                }
                else
                {
                    var empty = visualsList.FirstOrDefault(v => v.RequestInf == null || v.RequestInf.Content == RquestContent.None);
                    if (empty != null && empty.State != VisualCopy.VisualCopyState.Finished)
                        Configuration.Main.SetRunningState(empty, info);
                    else
                        Configuration.Main.SetRunningState(Configuration.Main.AddNewVisualCopy(), info);
                }
            }

            Configuration.Main.PLaySoundAfterOperation(Configuration.Main.PlaySound_After_ADD_DATA, Configuration.Main.AddData_Sound);
        }
    }
}

