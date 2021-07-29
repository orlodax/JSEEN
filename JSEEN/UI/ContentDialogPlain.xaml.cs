using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JSEEN.UI
{
    public sealed partial class ContentDialogPlain : ContentDialog
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ContentDialogPlain), new PropertyMetadata(default(string)));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ContentDialogPlain()
        {
            InitializeComponent();
            Text = "newJsonFile";
        }
    }
}
