using JSEEN.VMs;
using System;
using System.Threading;
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

        /// <param name="e" contains the VM></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MainPageVM.Panels.CollectionChanged += Panels_CollectionChanged;

            DataContext = e.Parameter;
        }

        // for some reason I don't care enough to ascertain, both of the events needs to call FollowSelection
        private void MainContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FollowSelection();
        }
        private void Panels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                FollowSelection();
        }
        private void FollowSelection()
        {
            _ = mainContainer.ChangeView(double.MaxValue, 0, 1);
        }
    }
}
