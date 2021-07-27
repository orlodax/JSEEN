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
        public Brush Background { get => background; set => SetValue(ref background, value); }

        private StackPanel panel;
        public StackPanel Panel { get => panel; set => SetValue(ref panel, value); }
        
        public List<FrameworkElement> Controls { get; private set; }
        public JToken JToken { get; private set; }

        public SingleLayerVM(JToken jToken)
        {
            JToken = jToken;

            Panel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10),
                BorderBrush = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightListAccentMediumBrush"],
                BorderThickness = new Thickness(1)
            };
            Panel.SetBinding(Windows.UI.Xaml.Controls.Panel.BackgroundProperty, new Binding()
            {
                Source = Background,
                Path = new PropertyPath("Background"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            Controls = ControlsHelper.GetLayerControls(JToken);

            foreach (FrameworkElement control in Controls)
                Panel.Children.Add(control);
        }
    }
}
