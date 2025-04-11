using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace JSEEN.Models;

public class TreeItem
{
    public string? Name { get; set; }
    public string? Content { get; set; }
    public Microsoft.UI.Xaml.Controls.Symbol Glyph { get; set; }
    public ObservableCollection<TreeItem> Children { get; set; } = [];
    public JToken? JObject { get; set; } = null;
    public IStorageItem StorageItem { get; set; }

    public TreeItem(IStorageItem item)
    {
        StorageItem = item;
        switch (item)
        {
            case StorageFile file:
                Glyph = Microsoft.UI.Xaml.Controls.Symbol.Document;
                Name = file.DisplayName;
                break;
            case StorageFolder folder:
                Glyph = Microsoft.UI.Xaml.Controls.Symbol.Folder;
                Name = folder.DisplayName;
                break;
            default:
                break;
        }
    }
}
