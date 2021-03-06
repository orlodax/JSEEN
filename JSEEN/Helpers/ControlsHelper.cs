using JSEEN.UI;
using JSEEN.VMs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using System.Linq;

namespace JSEEN.Helpers
{
    public static class ControlsHelper
    {
        #region Json parser
        internal static List<FrameworkElement> GetLayerControls(JToken prop, string propertyName = null)
        {
            var controls = new List<FrameworkElement>();

            foreach (JToken child in prop)
            {
                if (child.Type is JTokenType.Object || child.Type is JTokenType.Array)
                    controls.Add(new NestingButton() { DataContext = new NestingButtonVM(child) });
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

                            GetControls(niece, controls, propertyName);
                        }
                    }
                    else
                    {
                        GetControls(child, controls, propertyName);
                    }
                }
            }
            return controls;
        }
        private static void GetControls(JToken child, List<FrameworkElement> controls, string propertyName)
        {
            switch (child.Type)
            {
                case JTokenType.Property:
                    controls.AddRange(GetLayerControls(child, propertyName));
                    break;

                case JTokenType.Object:
                case JTokenType.Array:
                    controls.Add(new NestingButton() { DataContext = new NestingButtonVM(child) });
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
                Width = 178,
                Margin = new Thickness(0, 0, 0, 10),
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
                FontSize = 14,
                Padding= new Thickness(10),
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                FontWeight = Windows.UI.Text.FontWeights.Bold,
                Width = 178
            };
        }
        #endregion
    }
}
