using NeathCopy.Module2_Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeathCopy.ViewModels
{
    public class ConfigurationWindow_VM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<string> AddDataBehaviours { get; set; }
        public ConfigurationWindow_VM()
        {
            AddDataBehaviours = Configuration.addDataFac.AddDataBehaviours;
        }
    }
}
