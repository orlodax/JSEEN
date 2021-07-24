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
            displayPane.SizeChanged += MainContainer_SizeChanged;
        }

        private void MainContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mainContainer.ChangeView(double.MaxValue, 0, 1);
        }

        /// <param name="e" contains the VM></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DataContext = e.Parameter;
        }
    }
}
