using JSEEN.VMs;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace JSEEN
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            // To customize titlebar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            AppTitleBar.Height = 32;
            Window.Current.SetTitleBar(AppTitleBar);

            // when content is added to main container, scroll all the way up and right
            displayPane.SizeChanged += DisplayPane_SizeChanged;
        }

        /// <param name="e" contains the VM></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainPageVM.Panels.CollectionChanged += Panels_CollectionChanged;

            DataContext = e.Parameter;
        }

        // for some reason I don't care enough to ascertain, both of the events need to call ScrollTo top right 
        private void DisplayPane_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > e.PreviousSize.Width)
                ScrollTo(double.MaxValue, 0);
        }
        private void Panels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                ScrollTo(double.MaxValue, 0);
        }
        private void ScrollTo(double? X, double? Y)
        {
            _ = mainContainer.ChangeView(X, Y, 1);
        }
    }
}
