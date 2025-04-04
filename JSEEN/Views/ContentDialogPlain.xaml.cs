using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace JSEEN.Views;

public sealed partial class ContentDialogPlain : ContentDialog
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ContentDialogPlain), new PropertyMetadata(default(string)));
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ContentDialogPlain(string title)
    {
        InitializeComponent();
        Title = title + " Name";
    }
    public ContentDialogPlain()
    {
        InitializeComponent();
        Text = "newJsonFile";
    }
}
