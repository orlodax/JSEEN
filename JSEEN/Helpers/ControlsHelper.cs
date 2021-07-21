using JSEEN.UI;
using JSEEN.VMs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace JSEEN.Helpers
{
    public static class ControlsHelper
    {
        internal static List<FrameworkElement> GetLayerControls(IEnumerable<JToken> propList, ObservableCollection<SingleLayer> panels)
        {
            List<FrameworkElement> controls = new List<FrameworkElement>();

            foreach (JToken prop in propList)
                controls.AddRange(GetNestedLayerControls(prop, panels));

            return controls;
        }

        internal static IEnumerable<FrameworkElement> GetNestedLayerControls(JToken prop, ObservableCollection<SingleLayer> panels)
        {
            List<FrameworkElement> controls = new List<FrameworkElement>();
            
            string propertyName = prop.Path;
            if (prop is JProperty)
                propertyName = (prop as JProperty).Name;

            int a = 0;
            foreach (JToken child in prop.Children())
                a++;

            if (a > 0)
            {
                foreach (JToken child in prop.Children())
                    GetControls(child, controls, propertyName, panels);
            }
            else
            {
                GetControls(prop, controls, propertyName, panels);
            }

            return controls;
        }

        private static void GetControls(JToken child, List<FrameworkElement> controls, string propertyName, ObservableCollection<SingleLayer> panels)
        {
            switch (child.Type)
            {
                case JTokenType.Property:
                    controls.AddRange(GetLayerControls(child.Children(), panels));
                    break;

                case JTokenType.Object:
                case JTokenType.Array:
                    controls.Add(new NestingButton() { DataContext = new NestingButtonVM(child, panels) });
                    break;

                case JTokenType.Boolean:
                    controls.Add(CreateCheckBox(child, propertyName));
                    break;

                case JTokenType.None:
                case JTokenType.Constructor:
                case JTokenType.Comment:
                case JTokenType.Float:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                case JTokenType.Integer:
                case JTokenType.String:
                    controls.Add(CreateTextBox(child, propertyName));
                    break;

                default:
                    break;
            }
        }

        private static readonly Binding defaultBinding = new Binding()
        {
            Path = new PropertyPath("Value"),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        private static TextBox CreateTextBox(JToken subToken, string propertyName)
        {
            var tb = new TextBox
            {
                DataContext = subToken,
                Header = propertyName
            };
            tb.SetBinding(TextBox.TextProperty, defaultBinding);

            return tb;
        }
        private static CheckBox CreateCheckBox(JToken subToken, string propertyName)
        {
            var cb = new CheckBox
            {
                DataContext = subToken,
                Content = propertyName
            };
            cb.SetBinding(Windows.UI.Xaml.Controls.Primitives.ToggleButton.IsCheckedProperty, defaultBinding);

            return cb;
        }
    }
}
