using Alphaleonis.Win32.Filesystem;
using NeathCopy.Module1_ShellExt;
using NeathCopy.Module2_Configuration;
using NeathCopy.UsedWindows;
using NeathCopyEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace NeathCopy.ViewModels
{
    public class VisualsCopysHandlerViewModel : ViewModelBase
    {
        public void CheckAndFixRequest()
        {
            var browseDestiny = new UserDropUIWindow();

            if (StartupClass.requestInfo.Operation != "copy"
                && StartupClass.requestInfo.Operation != "move" && StartupClass.requestInfo.Operation != "fastmove")
            {
                browseDestiny.SetTitle(string.Format("Invalid Operation: {0}, select Copy or Move.", StartupClass.requestInfo.Operation));
                browseDestiny.SetDestiny(StartupClass.requestInfo.Destiny);
                browseDestiny.ShowDialog();
                if (browseDestiny.DlgResult)
                {
                    StartupClass.requestInfo.Destiny = browseDestiny.Destiny;
                    StartupClass.requestInfo.Operation = browseDestiny.Operation;
                    StartupClass.requestInfo.Content = RquestContent.Sources | RquestContent.Operation;
                }
                else Process.GetCurrentProcess().Kill();
            }

            //Destiny
            else if (!Directory.Exists(StartupClass.requestInfo.Destiny))
            {
                browseDestiny.SetTitle(string.Format("Destiny: {0} do not exist. Browse other", StartupClass.requestInfo.Destiny));
                browseDestiny.SetOperation(StartupClass.requestInfo.Operation);
                browseDestiny.SetDestiny(StartupClass.requestInfo.Destiny);

                browseDestiny.ShowDialog();

                if (browseDestiny.DlgResult)
                {
                    StartupClass.requestInfo.Destiny = browseDestiny.Destiny;
                    StartupClass.requestInfo.Content = RquestContent.All;
                }
                else Process.GetCurrentProcess().Kill();
            }
            else StartupClass.requestInfo.Content = RquestContent.All;

            browseDestiny.Close();
        }

        public void HandleAddDataInfo(IEnumerable<VisualCopy> visualsCopys)
        {
            Configuration.Main.addDataBehaviour.Execute(visualsCopys);
        }
    }
}
