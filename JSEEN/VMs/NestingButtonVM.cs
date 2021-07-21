using JSEEN.Classes;
using JSEEN.UI;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JSEEN.VMs
{
    public class NestingButtonVM : Observable
    {
        private JToken property;
        private ObservableCollection<SingleLayer> panels;
        private string charIcon;
        private string text;
        private string name;

        public string CharIcon { get => charIcon; private set => SetValue(ref charIcon, value); }
        public string Type { get => text; private set => SetValue(ref text, value); }
        public string Name { get => name; private set => SetValue(ref name, value); }

        public ICommand ButtonClick { get; private set; }

        public NestingButtonVM(JToken property, ObservableCollection<SingleLayer> panels)
        {
            this.property = property;
            this.panels = panels;

            ButtonClick = new RelayCommand(Exec_ButtonClick);

            Name = property.Path;
            switch (property.Type)
            {
                case JTokenType.Array:
                    CharIcon = " [ ]";
                    Type = "array";
                    break;

                case JTokenType.Object:
                    CharIcon = " { }";
                    Type = "object";
                    break;

                default:
                    break;
            }
        }

        void Exec_ButtonClick(object parameter)
        {
            panels.Add(new SingleLayer() { DataContext = new SingleLayerVM(new List<JToken>() { property }, panels) });
        }
    }
}
