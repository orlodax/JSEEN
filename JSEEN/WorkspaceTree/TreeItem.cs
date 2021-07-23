using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.Storage;

namespace JSEEN.WorkspaceTree
{
    public class TreeItem
    {
        public string Name { get; set; }
        public Windows.UI.Xaml.Controls.Symbol Glyph { get; set; }
        public List<TreeItem> Children { get; set; } = new List<TreeItem>();
        public IStorageItem StorageItem { get; set; }
        public JObject JObject { get; set; }
    }
}
