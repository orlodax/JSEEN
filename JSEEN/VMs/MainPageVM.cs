using JSEEN.Classes;
using JSEEN.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JSEEN.VMs
{
    public class MainPageVM : Observable
    {
        #region Props, fields, cmds
        private ObservableCollection<StorageFile> files = new ObservableCollection<StorageFile>();
        private StorageFile selectedFile;
        private StorageFolder workspace = null;
        private static JObject currentJObject;
        private StackPanel panelsView;
        private ObservableCollection<SingleLayer> panels = new ObservableCollection<SingleLayer>();

        public ObservableCollection<StorageFile> Files { get => files; set => SetValue(ref files, value); }
        public StorageFile SelectedFile { get => selectedFile; set { SetValue(ref selectedFile, value); SelectedFileChanged(); } }
        public StorageFolder Workspace { get => workspace; set => SetValue(ref workspace, value); }
        public JObject CurrentJObject { get => currentJObject; set => SetValue(ref currentJObject, value); }
        
        // binding to the usercontrol visualizing everything, contains the layers of the json, stacked horizontally, fed by following list named Panels
        public StackPanel PanelsView { get => panelsView; set => SetValue(ref panelsView, value); }
        // list of grid/stackpanel holding a single layer's controls
        public ObservableCollection<SingleLayer> Panels { get => panels; set => SetValue(ref panels, value); }

        public ICommand ChooseFolder { get; private set; }

        #endregion

        #region CTOR
        public MainPageVM()
        {
            //to reset the folder in debug
            //ApplicationData.Current.LocalSettings.Values["workSpace"] = null;
            ChooseFolder = new RelayCommand(Exec_ChooseFolder);

            Panels.CollectionChanged += Panels_CollectionChanged;

            Init();
        }
        private async void Init()
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("workSpace"))
                Workspace = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("workSpace");

            if (Workspace != null)
                LoadFiles(Workspace);
            else
                Exec_ChooseFolder(null);
        }
        #endregion

        #region I/O
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
                // recursive
                if (item is StorageFolder subFolder)
                    LoadFiles(subFolder);
            }
        }
        private void SaveFile()
        {
            //if (SelectedNode != null && SelectedChapter != null)
            //{
            //    if (LocalSettings.Values.TryGetValue("workSpace", out object path))
            //    {
            //        var workSpace = await StorageFolder.GetFolderFromPathAsync(path.ToString());
            //        if (workSpace != null)
            //        {
            //            if (ChapterFiles.TryGetValue(SelectedChapter, out StorageFile file))
            //            {
            //                string uglyString = Newtonsoft.Json.JsonConvert.SerializeObject(SelectedChapter);
            //                string okString = JToken.Parse(uglyString).ToString();
            //                await FileIO.WriteTextAsync(file, okString);
            //            }
            //        }
            //    }
            //}
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
                    CurrentJObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(json);
                    PanelsView = new StackPanel()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Orientation = Orientation.Horizontal
                    };
                    Panels.Clear();
                    Panels.Add(new SingleLayer() { DataContext = new SingleLayerVM(CurrentJObject.Children(), Panels) });
                }
            }
        }
        private void Panels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshPanels();
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
        #endregion

        #region Command methods
        private async void Exec_ChooseFolder(object parameter)
        {
            // if we are changing workspace, clear access list
            if (Workspace != null)
                StorageApplicationPermissions.FutureAccessList.Clear();

            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            Workspace = await folderPicker.PickSingleFolderAsync();
            if (Workspace != null)
            {
                // Application now has read/write access to all contents in the picked folder
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("workSpace", Workspace);

                LoadFiles(Workspace);
            }
        }
        #endregion
    }
}
