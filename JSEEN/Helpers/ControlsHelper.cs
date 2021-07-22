using JSEEN.UI;
using JSEEN.VMs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using System.Linq;

namespace JSEEN.Helpers
{
    public static class ControlsHelper
    {
        #region Json parser
        internal static List<FrameworkElement> GetLayerControls(JToken prop, ObservableCollection<SingleLayer> panels, string propertyName = null)
        {
            var controls = new List<FrameworkElement>();

            foreach (JToken child in prop)
            {
                if (child.Type is JTokenType.Object || child.Type is JTokenType.Array)
                    controls.Add(new NestingButton() { DataContext = new NestingButtonVM(child, panels) });
                else
                {
                    if (string.IsNullOrEmpty(propertyName))
                        propertyName = child.Path;

                    if (child is JProperty)
                        propertyName = (child as JProperty).Name;

                    if (child.Children().Count() > 0)
                    {
                        foreach (JToken niece in child)
                        {
                            if (niece is JProperty)
                                propertyName = (niece as JProperty).Name;

                            GetControls(niece, controls, propertyName, panels);
                        }
                    }
                    else
                    {
                        GetControls(child, controls, propertyName, panels);
                    }
                }
            }
            return controls;
        }
        private static void GetControls(JToken child, List<FrameworkElement> controls, string propertyName, ObservableCollection<SingleLayer> panels)
        {
            switch (child.Type)
            {
                case JTokenType.Property:
                    controls.AddRange(GetLayerControls(child, panels, propertyName));
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
        #endregion

        #region Controls Builder
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
                Header = propertyName,
                Width = 180,
                TextWrapping = TextWrapping.Wrap
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

        internal static TextBlock CreateNullTextBlock()
        {
            return new TextBlock
            {
                Text = "null",
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                Width = 180,
            };
        }
        #endregion
    }
}
