﻿using JSEEN.Views;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using JSEEN.Models;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage.Pickers;

namespace JSEEN.ViewModels;

public partial class MainPageVM : ObservableObject
{
    #region Props and fields
    /// <summary>
    /// Selected Item from the treeview, contains the string json and reference to its JObject
    /// </summary>
    private TreeItem? currentItem;
    public TreeItem? CurrentItem
    {
        get => currentItem;
        set
        {
            Exec_SaveFile();
            if (SetProperty(ref currentItem, value))
                _ = SelectItem();
        }
    }

    /// <summary>
    /// The root folder - all jsons and subfolders with jsons will be loaded in the treeview (file explorer)
    /// </summary>
    private StorageFolder? workspace = null;
    public StorageFolder? Workspace { get => workspace; set => SetProperty(ref workspace, value); }

    /// <summary>
    /// List feeding the treeview
    /// </summary>
    private ObservableCollection<TreeItem> workspaceTree = [];
    public ObservableCollection<TreeItem> WorkspaceTree { get => workspaceTree; set => SetProperty(ref workspaceTree, value); }

    /// <summary>
    /// Displays path of currently selected JToken
    /// </summary>
    private string? jPath;
    public string? JPath { get => jPath; private set => SetProperty(ref jPath, value); }

    /// <summary>
    /// Binding to the usercontrol visualizing everything, contains the layers of the json, stacked horizontally, fed by SingleLayer observable collection
    /// </summary>
    private StackPanel panelsView = new();
    public StackPanel PanelsView { get => panelsView; private set => SetProperty(ref panelsView, value); }

    /// <summary>
    /// List of grid/stackpanel holding a single layer's controls
    /// </summary>
    public static ObservableCollection<SingleLayer> Panels { get; set; } = [];

    /// <summary>
    /// Progress bar loading folders
    /// </summary>
    private bool progressBarVisibility;
    public bool ProgressBarVisibility { get => progressBarVisibility; set => SetProperty(ref progressBarVisibility, value); }
    #endregion

    #region Commands definitions
    public ICommand ChooseFolder { get; }
    public ICommand SaveFile { get; }
    public ICommand NewFile { get; }
    #endregion

    public MainPageVM()
    {
        ChooseFolder = new RelayCommand(async () => await Exec_ChooseFolder());
        SaveFile = new RelayCommand(Exec_SaveFile);
        NewFile = new RelayCommand(Exec_NewFile);

        Panels.CollectionChanged += Panels_CollectionChanged;

        _ = LoadWorkspaceAsync();
    }

    private async Task LoadWorkspaceAsync()
    {
        Workspace = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(nameof(Workspace));
        if (Workspace != null)
        {
            WorkspaceTree = await PopulateWorspaceRecursively(Workspace);
        }
    }

    #region Event Handlers
    private void Panels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        {
            PanelsView.Children.Clear();
            PanelsView = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Orientation.Horizontal
            };
        }
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            PanelsView.Children.Clear();

            SingleLayer lastPanel = Panels.Last();

            // update jpath
            JPath = ((SingleLayerVM)lastPanel.DataContext).JToken.Path;

            // highlight selected elements...
            IEnumerable<JToken> tokens = Panels.Select(p => ((SingleLayerVM)p.DataContext).JToken);
            foreach (SingleLayer sl in Panels)
            {
                // use foreach to repopulate PanelsView container
                PanelsView.Children.Add(sl);

                // turn off selected SingleLayers
                var slVM = (SingleLayerVM)sl.DataContext;
                slVM.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));

                // using list of tokens, iterate over buttons to highlight the selected one (compare their JToken)
                IEnumerable<FrameworkElement> buttons = slVM.Controls.Where(c => c is NestingButton);
                foreach (FrameworkElement nb in buttons)
                {
                    var nbVM = (NestingButtonVM)((NestingButton)nb).DataContext;

                    if (tokens.Contains(nbVM.JToken))
                        nbVM.Background = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightListAccentMediumBrush"];
                    else
                        nbVM.Background = new SolidColorBrush(new Windows.UI.Color { A = 0, R = 0, G = 0, B = 0 });
                }

                // either highlight last panel or display null
                if (sl == lastPanel)
                {
                    var lastLayerVM = (SingleLayerVM)sl.DataContext;
                    if (lastLayerVM.Panel.Children.Count == 0)
                        lastLayerVM.Panel.Children.Add(Helpers.ControlsHelper.CreateNullTextBlock());
                    else
                        lastLayerVM.Background = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightListAccentMediumBrush"];
                }
            }
        }
    }
    private async Task Exec_ChooseFolder()
    {
        FileOpenPicker filePicker = new();
        // Ensure the picker is initialized with the current window's HWND
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, App.MainWindowHandle);
        filePicker.FileTypeFilter.Add(".json");

        StorageFile selectedFile = await filePicker.PickSingleFileAsync();
        if (selectedFile != null)
        {
            Workspace = await selectedFile.GetParentAsync();
            if (Workspace != null)
            {
                // Application now has read/write access to all contents in the picked folder
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(nameof(Workspace), Workspace);

                WorkspaceTree.Clear();
                PanelsView.Children?.Clear();
                Panels.Clear();

                WorkspaceTree = await PopulateWorspaceRecursively(Workspace);
                CurrentItem = WorkspaceTree.FirstOrDefault(i => i.Name?.Equals(selectedFile.DisplayName) == true);
            }
        }
    }
    // recursively opens all jsons under main folder selected as Workspace
    private async Task<ObservableCollection<TreeItem>> PopulateWorspaceRecursively(StorageFolder folder)
    {
        ProgressBarVisibility = true;

        List<TreeItem> treeList = [];

        IReadOnlyList<IStorageItem> items = await folder.GetItemsAsync();

        foreach (IStorageItem item in items)
        {
            if (item is StorageFile file && file.FileType == ".json")
            {
                treeList.Add(new TreeItem(file)
                {
                    Content = await FileIO.ReadTextAsync(file),
                });
            }

            if (item is StorageFolder subFolder)
            {
                treeList.Add(new TreeItem(subFolder)
                {
                    Children = await PopulateWorspaceRecursively(subFolder),
                });
            }
        }
        // remove folders without jsons inside
        treeList.RemoveAll(t => t.StorageItem is StorageFolder && !t.Children.Any());

        ProgressBarVisibility = false;

        return [.. treeList];
    }
    private async void Exec_SaveFile()
    {
        if (CurrentItem?.JObject != null)
            await FileIO.WriteTextAsync(CurrentItem.StorageItem as StorageFile, Newtonsoft.Json.JsonConvert.SerializeObject(CurrentItem.JObject, Newtonsoft.Json.Formatting.Indented));
    }
    private async Task SelectItem()
    {
        if (CurrentItem is null || string.IsNullOrEmpty(CurrentItem?.Content))
            return;

        try
        {
            if (CurrentItem.JObject == null)
                CurrentItem.JObject = Newtonsoft.Json.JsonConvert.DeserializeObject<JToken>(CurrentItem.Content);

            PanelsView = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Orientation.Horizontal
            };
            Panels.Clear();

            if (CurrentItem.JObject is not null)
                Panels.Add(new SingleLayer() { DataContext = new SingleLayerVM(CurrentItem.JObject.Root, Panels.Count) });
        }
        catch (Exception e)
        {
            Panels.Clear();

            var dialog = new ContentDialog
            {
                Title = "File not valid",
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                Content = e.Message
            };

            _ = await dialog.ShowAsync();
        }
    }
    private async void Exec_NewFile()
    {
        ContentDialogPlain dialog = new("File Name");
        ContentDialogResult result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            string fileName = dialog.Text;

            if (Workspace != null)
            {
                if (!WorkspaceTree.Any(t => t.Name == fileName))
                {
                    StorageFile newFile = await Workspace.CreateFileAsync(fileName + ".json");
                    await FileIO.WriteTextAsync(newFile, "{}");
                    TreeItem newItem = new(newFile)
                    {
                        Content = await FileIO.ReadTextAsync(newFile)
                    };

                    WorkspaceTree.Add(newItem);
                }
                else
                {
                    ContentDialog cd = new()
                    {
                        Title = "File already exists",
                        CloseButtonText = "Close",
                        DefaultButton = ContentDialogButton.Close,
                        Content = "Please provide a different name."
                    };

                    _ = await cd.ShowAsync();

                    Exec_NewFile();
                }
            }
        }
    }
    #endregion
}