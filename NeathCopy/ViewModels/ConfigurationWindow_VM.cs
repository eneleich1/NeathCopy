using NeathCopy.Module2_Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace NeathCopy.ViewModels
{
    public class ConfigurationWindow_VM : ViewModelBase
    {
        private List<string> addDataBehaviours;

        public List<string> AddDataBehaviours
        {
            get => addDataBehaviours;
            set => SetProperty(ref addDataBehaviours, value);
        }

        public ConfigurationWindow_VM()
        {
            AddDataBehaviours = Configuration.addDataFac.AddDataBehaviours;
        }

        public string NeathCopyVersion => "NeathCopy " + GetType().Assembly.GetName().Version.ToString(3);
    }
}
