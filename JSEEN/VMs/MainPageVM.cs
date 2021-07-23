using JSEEN.Classes;
using JSEEN.UI;
using JSEEN.WorkspaceTree;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JSEEN.VMs
{
    public class MainPageVM : Observable
    {
        #region Fields
        private StorageFolder workspace = null;
        private JObject currentJObject;
        #endregion

        #region Bindings and UI
        public List<TreeItem> WorkspaceTree { get; set; }
        public ObservableCollection<StorageFile> Files { get; private set; } = new ObservableCollection<StorageFile>();

        private string jPath;
        public string JPath { get => jPath; private set => SetValue(ref jPath, value); }

        private StorageFile selectedFile;
        public StorageFile SelectedFile { get => selectedFile; set { selectedFile = value; SelectedFileChanged(); } }

        // binding to the usercontrol visualizing everything, contains the layers of the json, stacked horizontally, fed by SingleLayer observable collection
        private StackPanel panelsView;
        public StackPanel PanelsView { get => panelsView; private set => SetValue(ref panelsView, value); }
        #endregion

        // list of grid/stackpanel holding a single layer's controls
        public ObservableCollection<SingleLayer> Panels { get; set; } = new ObservableCollection<SingleLayer>();

        #region Commands definitions
        public ICommand ChooseFolder { get; private set; }
        public ICommand SaveFile { get; private set; }
        public ICommand TreeItemSelected { get; private set; }
        #endregion

        #region CTOR
        public MainPageVM()
        {
            //to reset the folder in debug
            //ApplicationData.Current.LocalSettings.Values["workSpace"] = null;
            ChooseFolder = new RelayCommand(Exec_ChooseFolder);
            SaveFile = new RelayCommand(Exec_SaveFile);

            Panels.CollectionChanged += Panels_CollectionChanged;

            Init();
        }
        private async void Init()
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("workSpace"))
                workspace = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("workSpace");

            if (workspace != null)
                LoadFiles(workspace);
            else
                Exec_ChooseFolder(null);
        }
        #endregion

        #region "Events"
        private async void SelectedFileChanged()
        {
            if (SelectedFile != null)
            {
                string json = await FileIO.ReadTextAsync(SelectedFile);

                if (!string.IsNullOrEmpty(json))
                {
                    currentJObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
                    currentJObject.PropertyChanging += CurrentJObject_PropertyChanging;

                    PanelsView = new StackPanel()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Orientation = Orientation.Horizontal
                    };
                    Panels.Clear();
                    Panels.Add(new SingleLayer() { DataContext = new SingleLayerVM(currentJObject.Root, Panels) });
                }
            }
        }

        private void CurrentJObject_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
        {
            Exec_SaveFile(sender);
        }

        private void Panels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshPanels();
            CheckForNullAndUpdateJPath();
            //Exec_SaveFile(sender);
        }
        private void RefreshPanels()
        {
            PanelsView.Children.Clear();
            PanelsView = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Orientation.Horizontal
            };

            foreach (SingleLayer p in Panels)
                PanelsView.Children.Add(p);
        }
        private void CheckForNullAndUpdateJPath()
        {
            if (Panels.Any())
            {
                SingleLayer lastPanel = Panels.Last();
                var lastLayerVM = lastPanel.DataContext as SingleLayerVM;
                if (!lastLayerVM.Panel.Children.Any())
                    lastLayerVM.Panel.Children.Add(Helpers.ControlsHelper.CreateNullTextBlock());

                JPath = (lastPanel.DataContext as SingleLayerVM).JToken.Path;
            }
        }
        #endregion

        #region Command methods
        private async void Exec_ChooseFolder(object parameter)
        {
            Files.Clear();

            // if we are changing workspace, clear access list
            if (workspace != null)
                StorageApplicationPermissions.FutureAccessList.Clear();

            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            workspace = await folderPicker.PickSingleFolderAsync();
            if (workspace != null)
            {
                // Application now has read/write access to all contents in the picked folder
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("workSpace", workspace);

                LoadFiles(workspace);
            }
        }
        // recursively opens all jsons under main folder selected as workspace
        private async void LoadFiles(StorageFolder folder)
        {
            foreach (var item in await folder.GetItemsAsync())
            {
                if (item is StorageFile file)
                {
                    if (file.FileType == ".json")
                        Files.Add(file);
                }
                
                if (item is StorageFolder subFolder)
                    LoadFiles(subFolder);
            }
        }
        private async void Exec_SaveFile(object parameter)
        {
            if (SelectedFile != null)
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(currentJObject);

                await FileIO.WriteTextAsync(SelectedFile, json);
            }
        }
        #endregion
    }
}
