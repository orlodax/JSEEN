using JSEEN.Views;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JSEEN.ViewModels;

public partial class NestingButtonVM : ObservableObject
{
    private readonly int index;

    public string? CharIcon { get; private set; }
    public string? Type { get; private set; }
    public string Name { get; private set; }
    public JToken JToken { get; private set; }

    private Brush? background;
    public Brush? Background { get => background; set => SetProperty(ref background, value); }

    public ICommand ButtonClick { get; private set; }

    public NestingButtonVM(JToken jToken, int singleLayerIndex)
    {
        JToken = jToken;
        index = singleLayerIndex;

        ButtonClick = new RelayCommand(Exec_ButtonClick);

        Name = JToken.Path;

        switch (JToken.Type)
        {
            case JTokenType.Array:
                CharIcon = " [ ]";
                Type = "array";

                if (Name.Contains('.'))
                    Name = FindArrayName(JToken);
                break;

            case JTokenType.Object:
                CharIcon = " { }";
                Type = "object";

                if (Name.Contains('.'))
                    Name = ParseObjectName();

                // arrays' new children will have path like $['{path}']
                if (Name.StartsWith("['") && Name.EndsWith("']"))
                    Name = Name[2..^2];
                break;

            default:
                break;
        }
    }

    #region Commands
    private void Exec_ButtonClick()
    {
        // rebuild panel collection up to where the button was clicked
        if (index >= 0)
        {
            List<SingleLayer> newPanels = [];
            for (int i = 0; i <= index; i++)
                newPanels.Add(MainPageVM.Panels[i]);

            MainPageVM.Panels.Clear();
            foreach (SingleLayer panel in newPanels)
                MainPageVM.Panels.Add(panel);
        }

        MainPageVM.Panels.Add(new SingleLayer() { DataContext = new SingleLayerVM(JToken, index + 1) });

        Background = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightListAccentMediumBrush"];
    }

    private static string FindArrayName(JToken? jToken)
    {
        if (jToken is JProperty property)
            return property.Name;
        else if (jToken is not null)
            return FindArrayName(jToken.Parent);

        return string.Empty;
    }
    private string ParseObjectName()
    {
        string[] segments = Name.Split(".");
        return segments[segments.GetUpperBound(0)];
    }
    #endregion
}
