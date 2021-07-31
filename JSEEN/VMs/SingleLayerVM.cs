using JSEEN.Classes;
using JSEEN.Helpers;
using JSEEN.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace JSEEN.VMs
{
    public class SingleLayerVM : Observable
    {
        #region Props
        private Brush background;
        public Brush Background { get => background; set => SetValue(ref background, value); }

        private StackPanel panel;
        public StackPanel Panel { get => panel; set => SetValue(ref panel, value); }

        public int SingleLayerIndex { get; set; }
        public List<FrameworkElement> Controls { get; private set; }
        public JToken JToken { get; private set; }

        public ICommand Create { get; private set; }
        #endregion

        #region CTOR
        public SingleLayerVM(JToken jToken, int index)
        {
            Create = new RelayCommand(Exec_Create);

            JToken = jToken;
            SingleLayerIndex = index;

            Panel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 10, 10, 0)
            };
            Panel.SetBinding(Windows.UI.Xaml.Controls.Panel.BackgroundProperty, new Binding()
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

            var dialog = new ContentDialogPlain(propertyType);
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                FrameworkElement newControl = ControlsHelper.AddSingleControl(JToken, dialog.Text, propertyType, SingleLayerIndex);

                // remove "null" label
                Panel.Children.Clear();

                Controls.Add(newControl);
                PopulatePanelChildren();
            }
        }
        #endregion
    }
}