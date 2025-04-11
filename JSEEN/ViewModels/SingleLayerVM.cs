using JSEEN.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using JSEEN.Views;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;

namespace JSEEN.ViewModels;

public partial class SingleLayerVM : ObservableObject
{
    #region Props
    private Brush? background;
    public Brush? Background { get => background; set => SetProperty(ref background, value); }

    private StackPanel panel = new()
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        Padding = new Thickness(10, 10, 10, 0)
    };
    public StackPanel Panel { get => panel; set => SetProperty(ref panel, value); }

    public int SingleLayerIndex { get; set; }
    public List<FrameworkElement> Controls { get; private set; }
    public JToken JToken { get; private set; }

    public ICommand Create { get; private set; }
    #endregion

    #region CTOR
    public SingleLayerVM(JToken jToken, int index)
    {
        Create = new RelayCommand<object>(async (parameter) => await Exec_Create(parameter));

        JToken = jToken;
        SingleLayerIndex = index;

        Panel.SetBinding(Microsoft.UI.Xaml.Controls.Panel.BackgroundProperty, new Binding()
        {
            Source = Background,
            Path = new PropertyPath("Background"),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        Controls = ControlsHelper.GetLayerControls(JToken, SingleLayerIndex);

        PopulatePanelChildren();
    }

    private void PopulatePanelChildren()
    {
        foreach (FrameworkElement control in Controls)
            Panel.Children.Add(control);
    }
    private async Task Exec_Create(object? parameter)
    {
        string propertyType = parameter?.ToString() ?? string.Empty;
        string propertyName = string.Empty;

        if (JToken.Type != JTokenType.Array)
        {
            var dialog = new ContentDialogPlain(propertyType);
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
                propertyName = dialog.Text;
        }

        FrameworkElement newControl = ControlsHelper.AddSingleControl(JToken, propertyName, propertyType, SingleLayerIndex);

        if (newControl != null)
        {
            // remove "null" label
            Panel.Children.Clear();

            Controls.Add(newControl);
            PopulatePanelChildren();
        }
    }
    #endregion
}