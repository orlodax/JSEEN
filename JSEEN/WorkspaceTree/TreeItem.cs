using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.Storage;

namespace JSEEN.WorkspaceTree
{
    public class TreeItem
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public Windows.UI.Xaml.Controls.Symbol Glyph { get; set; }
        public List<TreeItem> Children { get; set; } = new List<TreeItem>();
        public JToken JObject { get; set; } = null;
        public IStorageItem StorageItem { get; set; }

        public TreeItem(IStorageItem item)
        {
            StorageItem = item;
            switch (item)
            {
                case StorageFile _:
                    Glyph = Windows.UI.Xaml.Controls.Symbol.Document;
                    break;
                case StorageFolder _:
                    Glyph = Windows.UI.Xaml.Controls.Symbol.Folder;
                    break;
                default:
                    break;
            }
        }
    }
}
