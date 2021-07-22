﻿using JSEEN.Classes;
using JSEEN.UI;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace JSEEN.VMs
{
    public class NestingButtonVM
    {
        private readonly int index;
        private readonly JToken property;
        private ObservableCollection<SingleLayer> panels;

        public string CharIcon { get; private set; }
        public string Type { get; private set; }
        public string Name { get; private set; }

        public ICommand ButtonClick { get; private set; }

        public NestingButtonVM(JToken property, ObservableCollection<SingleLayer> panels)
        {
            this.property = property;
            this.panels = panels;
            index = panels.Count;

            ButtonClick = new RelayCommand(Exec_ButtonClick);

            Name = property.Path;

            switch (property.Type)
            {
                case JTokenType.Array:
                    CharIcon = " [ ]";
                    Type = "array";

                    if (Name.Contains("."))
                        Name = FindArrayName(property);

                    break;

                case JTokenType.Object:
                    CharIcon = " { }";
                    Type = "object";

                    if (Name.Contains("."))
                        Name = ParseObjectName();

                    break;

                default:
                    break;
            }
        }

        #region Commands
        private void Exec_ButtonClick(object parameter)
        {
            // rebuild panel collection up to where the button was clicked
            if (index > 0)
            {
                var newPanels = new List<SingleLayer>();
                for (int i = 0; i <= index; i++)
                    newPanels.Add(panels[i]);

                panels.Clear();
                foreach (SingleLayer panel in newPanels)
                    panels.Add(panel);
            }

            panels.Add(new SingleLayer() { DataContext = new SingleLayerVM(property, panels) });
        }

        private string FindArrayName(JToken property)
        {
            if (property is JProperty)
                return ((JProperty)property).Name;
            else
                return FindArrayName(property.Parent);
        }
        private string ParseObjectName()
        {
            string[] segments = Name.Split(".");
            return segments[segments.GetUpperBound(0)];
        }
        #endregion
    }
}
