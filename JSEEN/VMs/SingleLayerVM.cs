using JSEEN.Classes;
using JSEEN.Helpers;
using JSEEN.UI;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JSEEN.VMs
{
    public class SingleLayerVM : Observable
    {
        private StackPanel panel = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Left };

        public StackPanel Panel { get => panel; set => SetValue(ref panel, value); }

        public SingleLayerVM(IEnumerable<JToken> propList, ObservableCollection<SingleLayer> panels)
        {
            foreach (FrameworkElement control in ControlsHelper.GetLayerControls(propList, panels))
                Panel.Children.Add(control);
        }
    }
}
