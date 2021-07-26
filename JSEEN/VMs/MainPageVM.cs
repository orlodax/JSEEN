using JSEEN.Classes;
using JSEEN.UI;
using JSEEN.WorkspaceTree;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace JSEEN.VMs
{
    public class MainPageVM : Observable
    {
        #region Props and fields
        /// <summary>
        /// Selected Item from the treeview, contains the string json and reference to its JObject
        /// </summary>
        public TreeItem CurrentItem { get; set; }

        /// <summary>
        /// The root folder - all jsons and subfolders with jsons will be loaded in the treeview (file explorer)
        /// </summary>
        private StorageFolder workspace = null;
        public StorageFolder Workspace { get => workspace; set => SetValue(ref workspace, value); }

        /// <summary>
        /// List feeding the treeview
        /// </summary>
        private List<TreeItem> workspaceTree = new List<TreeItem>();
        public List<TreeItem> WorkspaceTree { get => workspaceTree; set => SetValue(ref workspaceTree, value); }
        
        /// <summary>
        /// Displays path of currently selected JToken
        /// </summary>
        private string jPath;
        public string JPath { get => jPath; private set => SetValue(ref jPath, value); }

        /// <summary>
        /// Binding to the usercontrol visualizing everything, contains the layers of the json, stacked horizontally, fed by SingleLayer observable collection
        /// </summary>
        private StackPanel panelsView = new StackPanel();
        public StackPanel PanelsView { get => panelsView; private set => SetValue(ref panelsView, value); }

        /// <summary>
        /// List of grid/stackpanel holding a single layer's controls
        /// </summary>
        public static ObservableCollection<SingleLayer> Panels { get; set; } = new ObservableCollection<SingleLayer>();
        #endregion

        #region Commands definitions
        public ICommand ChooseFolder { get; private set; }
        public ICommand SaveFiles { get; private set; }
        public ICommand TreeItemSelected { get; private set; }
        
        #endregion

        #region CTOR
        public MainPageVM()
        {
            //to reset the folder in debug
            //ApplicationData.Current.LocalSettings.Values["workSpace"] = null;
            ChooseFolder = new RelayCommand(Exec_ChooseFolder);
            SaveFiles = new RelayCommand(Exec_SaveFiles);
            TreeItemSelected = new RelayCommand(Exec_TreeItemSelected);

            Panels.CollectionChanged += Panels_CollectionChanged;

            Init();
        }
        private async void Init()
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("workSpace"))
                Workspace = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("workSpace");

            if (Workspace != null)
                WorkspaceTree = await PopulateWorspaceRecursively(Workspace);
            else
                Exec_ChooseFolder(null);
        }
        #endregion

        #region "Events"
        private void Panels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                PanelsView.Children.Clear();
                PanelsView = new StackPanel()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Orientation = Orientation.Horizontal
                };

                var lastPanel = Panels.Last();

                // update jpath
                JPath = (lastPanel.DataContext as SingleLayerVM).JToken.Path;

                // highlight selected elements...
                IEnumerable<JToken> tokens = Panels.Select(p => (p.DataContext as SingleLayerVM).JToken);
                foreach (SingleLayer sl in Panels)
                {
                    // use foreach to repopulate PanelsView container
                    PanelsView.Children.Add(sl);

                    // turn off selected SingleLayers
                    var slVM = sl.DataContext as SingleLayerVM;
                    slVM.Background = null;

                    // using list of tokens, iterate over buttons to highlight the selected one (compare their JToken)
                    IEnumerable<FrameworkElement> buttons = slVM.Controls.Where(c => c is NestingButton);
                    foreach (FrameworkElement nb in buttons)
                    {
                        var nbVM = (nb as NestingButton).DataContext as NestingButtonVM;

                        if (tokens.Contains(nbVM.JToken))
                            nbVM.Background = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark3"]);
                        else
                            nbVM.Background = new SolidColorBrush(new Windows.UI.Color { A = 0, R = 0, G = 0, B = 0 });
                    }

                    // either highlight last panel or display null
                    if (sl == lastPanel)
                    {
                        var lastLayerVM = sl.DataContext as SingleLayerVM;
                        if (!lastLayerVM.Panel.Children.Any())
                            lastLayerVM.Panel.Children.Add(Helpers.ControlsHelper.CreateNullTextBlock());
                        else
                            lastLayerVM.Background = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["SystemAccentColorDark3"]);
                    }
                }
            }
        }
        #endregion

        #region Command methods
        private async void Exec_ChooseFolder(object parameter)
        {
            // if we are changing Workspace, clear access list
            if (Workspace != null)
                StorageApplicationPermissions.FutureAccessList.Clear();

            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            Workspace = await folderPicker.PickSingleFolderAsync();
            if (Workspace != null)
            {
                // Application now has read/write access to all contents in the picked folder
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("workSpace", Workspace);

                WorkspaceTree.Clear();
                WorkspaceTree = await PopulateWorspaceRecursively(Workspace);
            }
        }
        // recursively opens all jsons under main folder selected as Workspace
        private async Task<List<TreeItem>> PopulateWorspaceRecursively(StorageFolder folder)
        {
            var treeList = new List<TreeItem>();

            IReadOnlyList<IStorageItem> items = await folder.GetItemsAsync();

            foreach (IStorageItem item in items)
            {
                if (item is StorageFile file && file.FileType == ".json")
                {
                    treeList.Add(new TreeItem(file)
                    {
                        Content = await FileIO.ReadTextAsync(file),
                        Name = file.DisplayName
                    });
                }

                if (item is StorageFolder subFolder)
                {
                    treeList.Add(new TreeItem(subFolder)
                    {
                        Children = await PopulateWorspaceRecursively(subFolder),
                        Name = subFolder.DisplayName
                    });
                }
            }

            // remove folders without jsons inside
            treeList.RemoveAll(t => t.StorageItem is StorageFolder && !t.Children.Any());

            return treeList;
        }
        private async void Exec_SaveFiles(object parameter)
        {
            foreach (TreeItem treeItem in WorkspaceTree.Where(t => t.StorageItem is StorageFile))
            {
                if (treeItem.JObject != null)
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(treeItem.JObject);
                    await FileIO.WriteTextAsync(treeItem.StorageItem as StorageFile, json);
                }
            }
        }
        private async void Exec_TreeItemSelected(object parameter)
        {
            if (parameter != null)
            {
                var treeItem = (parameter as TreeViewItemInvokedEventArgs).InvokedItem as TreeItem;

                if (!string.IsNullOrEmpty(treeItem.Content))
                {
                    //save previous item and swith to the current one
                    if (CurrentItem != null)
                        await FileIO.WriteTextAsync(CurrentItem.StorageItem as StorageFile, Newtonsoft.Json.JsonConvert.SerializeObject(CurrentItem.JObject));
                    CurrentItem = treeItem;

                    if (CurrentItem.JObject == null)
                    {
                        try
                        {
                            CurrentItem.JObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(treeItem.Content);
                        }
                        catch (Exception e)
                        {
                            ContentDialog dialog = new ContentDialog
                            {
                                Title = "Error",
                                CloseButtonText = "Close",
                                DefaultButton = ContentDialogButton.Close,
                                Content = e.Message
                            };

                            _ = await dialog.ShowAsync();
                        }
                    }

                    PanelsView = new StackPanel()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Orientation = Orientation.Horizontal
                    };
                    Panels.Clear();
                    Panels.Add(new SingleLayer() { DataContext = new SingleLayerVM(CurrentItem.JObject.Root) });
                }
            }
        }
        #endregion
    }
}