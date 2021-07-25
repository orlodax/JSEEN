using JSEEN.Classes;
using JSEEN.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace JSEEN.VMs
{
    public class SingleLayerVM : Observable
    {
        private Brush background;
        private StackPanel panel;

        public Brush Background { get => background; set => SetValue(ref background, value); }
        public StackPanel Panel { get => panel; set => SetValue(ref panel, value); }
        public List<FrameworkElement> Controls { get; set; }

        public JToken JToken { get; private set; }

        public SingleLayerVM(JToken property)
        {
            JToken = property;

            Panel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10)
            };
            Panel.SetBinding(Windows.UI.Xaml.Controls.Panel.BackgroundProperty, new Binding()
            {
                Source = Background,
                Path = new PropertyPath("Background"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            Controls = ControlsHelper.GetLayerControls(property);

            foreach (FrameworkElement control in Controls)
                Panel.Children.Add(control);
        }
    }
}
