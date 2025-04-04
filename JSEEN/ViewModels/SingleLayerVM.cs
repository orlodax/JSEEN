﻿using JSEEN.Helpers;
using JSEEN.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JSEEN.ViewModels;

public class SingleLayerVM : ObservableObject
{
    #region Props
    private Brush background;
    public Brush Background { get => background; set => SetProperty(ref background, value); }

    private StackPanel panel;
    public StackPanel Panel { get => panel; set => SetProperty(ref panel, value); }

    public int SingleLayerIndex { get; set; }
    public List<FrameworkElement> Controls { get; private set; }
    public JToken JToken { get; private set; }

    public ICommand Create { get; private set; }
    #endregion

    #region CTOR
    public SingleLayerVM(JToken jToken, int index)
    {
        Create = new RelayCommand<object>(Exec_Create);

        JToken = jToken;
        SingleLayerIndex = index;

        Panel = new StackPanel()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(10, 10, 10, 0)
        };
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
    #endregion

    #region Commands
    private async void Exec_Create(object parameter)
    {
        string propertyType = parameter.ToString();
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