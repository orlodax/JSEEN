using JSEEN.Helpers;
using JSEEN.UI;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JSEEN.VMs
{
    public class SingleLayerVM
    {
        public StackPanel Panel { get; private set; } = new StackPanel()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(10)
        };

        public JToken JToken { get; private set; }

        public SingleLayerVM(JToken property)
        {
            JToken = property;

            foreach (FrameworkElement control in ControlsHelper.GetLayerControls(property))
                Panel.Children.Add(control);
        }
    }
}
