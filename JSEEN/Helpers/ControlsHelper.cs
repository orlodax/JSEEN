using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System.Linq;
using JSEEN.Views;
using JSEEN.ViewModels;

namespace JSEEN.Helpers;

public static class ControlsHelper
{
    #region Json parser
    internal static List<FrameworkElement> GetLayerControls(JToken prop, int singleLayerIndex, string propertyName = null)
    {
        var controls = new List<FrameworkElement>();

        for (int i = 0; i < prop.Children().Count(); i++)
        {
            JToken child = prop.Children().ElementAt(i);

            if (child.Type is JTokenType.Object || child.Type is JTokenType.Array)
            {
                controls.Add(new NestingButton() { DataContext = new NestingButtonVM(child, singleLayerIndex) });
            }
            else
            {
                if (string.IsNullOrEmpty(propertyName))
                    propertyName = child.Path;

                if (child is JProperty)
                    propertyName = (child as JProperty).Name;

                if (prop.Type == JTokenType.Array)
                    propertyName = prop.Path + $"[{i}]";

                if (child.Children().Any())
                {
                    foreach (JToken niece in child)
                    {
                        if (niece is JProperty)
                            propertyName = (niece as JProperty).Name;

                        GetControls(niece, controls, singleLayerIndex, propertyName);
                    }
                }
                else
                {
                    GetControls(child, controls, singleLayerIndex, propertyName);
                }
            }
        }
        return controls;
    }
    private static void GetControls(JToken child, List<FrameworkElement> controls, int singleLayerIndex, string propertyName)
    {
        switch (child.Type)
        {
            case JTokenType.Property:
                controls.AddRange(GetLayerControls(child, singleLayerIndex, propertyName));
                break;

            case JTokenType.Object:
            case JTokenType.Array:
                controls.Add(new NestingButton() { DataContext = new NestingButtonVM(child, singleLayerIndex) });
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
    internal static FrameworkElement AddSingleControl(JToken parent, string newPropertyName, string propertyType, int index)
    {
        if (parent.Type != JTokenType.Array)
        {
            if (string.IsNullOrEmpty(newPropertyName) && parent.Type != JTokenType.Array)
                newPropertyName = $"field{parent.Children().Count()}";
        }
        else
        {
            newPropertyName = parent.Path + $"[{parent.Children().Count()}]";
        }

        FrameworkElement control = null;
        JProperty property = null;
        switch (propertyType)
        {
            case "Field":
                property = new JProperty(newPropertyName, "null");
                control = CreateTextBox(property.Value, newPropertyName);
                break;

            case "Bool":
                property = new JProperty(newPropertyName, "null");
                control = CreateCheckBox(property.Value, newPropertyName);
                break;

            case "Object":
                property = new JProperty(newPropertyName, new JObject());
                control = new NestingButton() { DataContext = new NestingButtonVM(property.Value, index) };
                break;

            case "Array":
                property = new JProperty(newPropertyName, new List<object>());
                control = new NestingButton() { DataContext = new NestingButtonVM(property.Value, index) };
                break;

            default:
                break;
        }

        JEnumerable<JToken> children = parent.Children();
        if (children.Any())
        {
            if (parent.Type == JTokenType.Array)
                children.Last().AddAfterSelf(property.Value);
            else
                children.Last().AddAfterSelf(property);
        }
        else
        {
            if (parent.Type == JTokenType.Array)
                (parent as JContainer).AddFirst(property.Value);
            else
                (parent as JContainer).AddFirst(property);
        }

        return control;
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
        cb.SetBinding(Microsoft.UI.Xaml.Controls.Primitives.ToggleButton.IsCheckedProperty, defaultBinding);

        return cb;
    }

    internal static TextBlock CreateNullTextBlock()
    {
        return new TextBlock
        {
            Text = "null",
            FontSize = 14,
            Padding = new Thickness(10),
            FontStyle = Windows.UI.Text.FontStyle.Italic,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Width = 178
        };
    }
    #endregion
}
