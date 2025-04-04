using JSEEN.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace JSEEN;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();

        DataContext = new MainPageVM();

        // when content is added to main container, scroll all the way up and right
        displayPane.SizeChanged += DisplayPane_SizeChanged;
        MainPageVM.Panels.CollectionChanged += Panels_CollectionChanged;
    }

    // for some reason I don't care enough to ascertain, both of the events need to call ScrollTo top right 
    private void DisplayPane_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width > e.PreviousSize.Width)
            ScrollTo(double.MaxValue, 0);
    }
    private void Panels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            ScrollTo(double.MaxValue, 0);
    }
    private void ScrollTo(double? X, double? Y)
    {
        _ = mainContainer.ChangeView(X, Y, 1);
    }
}
