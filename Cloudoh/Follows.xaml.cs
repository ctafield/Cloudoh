using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Cloudoh.Classes;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.ExtensionMethods;
using Cloudoh.ViewModels;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;

namespace Cloudoh
{
    public partial class Follows : PhoneApplicationPage
    {

        public FollowsViewModel ViewModel { get; set; }

        private long UserId { get; set; }

        public Follows()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ViewModel == null)
            {
                if (NavigationContext.QueryString.ContainsKey("UserId"))
                {
                    long userId;
                    if (long.TryParse(NavigationContext.QueryString["UserId"], out userId))
                    {
                        UserId = userId;
                        ViewModel = new FollowsViewModel();
                        DataContext = ViewModel;
                    }
                    else
                        NavigationService.GoBack();
                }
                else
                {
                    NavigationService.GoBack();
                }

                if (NavigationContext.QueryString.ContainsKey("Page"))
                {
                    switch (NavigationContext.QueryString["Page"])
                    {
                        case "following":
                            pivotMain.SelectedIndex = 1;
                            break;
                        case "followers":
                            pivotMain.SelectedIndex = 0;
                            break;
                    }
                }

            }            

        }

        private void pivotMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (pivotMain.SelectedIndex)
            {
                case 0:
                    RefreshFollowers();
                    break;

                case 1:
                    RefreshFollowing();
                    break;
            }
        }

        private void RefreshFollowing()
        {

            if (ViewModel.Following.Count > 0)
                return;

            var api = new SoundcloudApi();
            api.GetAllFollowingCompletedEvent += ApiOnGetAllFollowingCompletedEvent;
            api.GetAllFollowing(UserId);
        }

        private void ApiOnGetAllFollowingCompletedEvent(object sender, EventArgs eventArgs)
        {
            var api = sender as SoundcloudApi;

            if (api == null)
                return;

            UiHelper.SafeDispatch(() =>
            {
                // todo: change to whatever we use
                if (api.Following != null && api.Following.Count > 0)
                {

                    foreach (var user in api.Following)
                    {
                        ViewModel.Following.Add(user.AsViewModel());
                    }

                    noResultsFollowing.Visibility = Visibility.Collapsed;
                    lstFollowing.Visibility = Visibility.Visible;
                }
                else
                {
                    noResultsFollowing.Visibility = Visibility.Visible;
                    lstFollowing.Visibility = Visibility.Collapsed;
                }
            });
            
        }

        private void RefreshFollowers()
        {

            if (ViewModel.Followers.Count > 0)
                return;

            var api = new SoundcloudApi();
            api.GetAllFollowersCompletedEvent += ApiOnGetAllFollowersCompletedEvent;
            api.GetAllFollowers(UserId);
        }

        private void ApiOnGetAllFollowersCompletedEvent(object sender, EventArgs eventArgs)
        {
            var api = sender as SoundcloudApi;

            if (api == null)
                return;

            UiHelper.SafeDispatch(() =>
            {
                // todo: change to whatever we use
                if (api.Followers != null && api.Followers.Count > 0)
                {

                    foreach (var user in api.Followers)
                    {
                        ViewModel.Followers.Add(user.AsViewModel());
                    }

                    noResultsFollowers.Visibility = Visibility.Collapsed;
                    lstFollowers.Visibility = Visibility.Visible;
                }
                else
                {
                    noResultsFollowers.Visibility = Visibility.Visible;
                    lstFollowers.Visibility = Visibility.Collapsed;
                }
            }); 
        }

        private void lstSearch_SelectionChanged(object sender, ListBoxItemTapEventArgs listBoxItemTapEventArgs)
        {

            var box = sender as RadDataBoundListBox;
            if (box == null)
                return;

            var model = listBoxItemTapEventArgs.Item.DataContext as SoundcloudUserViewModel;
            if (model == null)
                return;

            App.ViewModel.CurrentSoundcloudProfile = model.Clone();

            box.SelectedItem = null;

            NavigationService.Navigate(new Uri("/SoundcloudProfile.xaml?userId=" + model.Id, UriKind.Relative));
                        
        }

    }
}