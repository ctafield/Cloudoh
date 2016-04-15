using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.ViewModels;
using CrittercismSDK;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Cloudoh
{

    public partial class MainPage : PhoneApplicationPage
    {

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
            InteractionEffectManager.AllowedTypes.Add(typeof(RadImageButton));

            try
            {
                //Shows the rate reminder message, according to the settings of the RateReminder.
                var app = Application.Current as App;
                if (app != null)
                    app.rateReminder.Notify();
            }
            catch (Exception ex)
            {
                Crittercism.LogHandledException(ex);
            }

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                if (NavigationContext.QueryString.ContainsKey("clear"))
                {
                    while (NavigationService.CanGoBack)
                        NavigationService.RemoveBackEntry();
                }
            }

            // Ensure that application state is restored appropriately
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.RefreshFavouritesCompletedEvent += RefreshFavouritesCompletedEvent;
                App.ViewModel.RefreshDashboardCompletedEvent += RefreshDashboardCompletedEvent;
                App.ViewModel.RefreshPlaylistsCompletedEvent += RefreshPlaylistsCompletedEvent;

                ThreadPool.QueueUserWorkItem(delegate
                                                 {
                                                     App.ViewModel.LoadData();

                                                     // Set the data context of the listbox control to the sample data
                                                     UiHelper.SafeDispatch(() =>
                                                                               {

                                                                                   lstDashboard.BeginUpdate();
                                                                                   lstLikes.BeginUpdate();
                                                                                   DataContext = App.ViewModel;
                                                                                   lstDashboard.EndUpdate(true);
                                                                                   lstLikes.EndUpdate(true);

                                                                               });

                                                     if (e.NavigationMode == NavigationMode.New)
                                                     {
                                                         ProcessNewActions();
                                                     }

                                                 });
            }
            else if (e.NavigationMode == NavigationMode.New)
            {
                ProcessNewActions();
            }

        }


        private void ProcessNewActions()
        {

            UiHelper.SafeDispatchSync(() =>
            {

                try
                {
                    if (NavigationContext.QueryString.ContainsKey("CloudohKey"))
                    {
                        int id;
                        id = int.Parse(NavigationContext.QueryString["CloudohKey"]);

                        App.ViewModel.CurrentSoundcloud = new SoundcloudViewModel()
                        {
                            Id = id,
                            Type = "favoriting"
                        };

                        NavigationService.Navigate(new Uri("/SoundcloudDetails.xaml?autoPlay=true&autoPlayId=" + id, UriKind.Relative));
                    }
                    else if (NavigationContext.QueryString.ContainsKey("SharedLink"))
                    {
                        var sharedLink = NavigationContext.QueryString["SharedLink"];
                        NavigationService.Navigate(new Uri("/OpenSharedLink.xaml?SharedLink=" + Uri.EscapeDataString(sharedLink), UriKind.Relative));
                    }
                    else if (NavigationContext.QueryString.ContainsKey("ExternalTrackId"))
                    {
                        int id;
                        id = int.Parse(NavigationContext.QueryString["ExternalTrackId"]);

                        NavigationService.Navigate(new Uri("/SoundcloudDetails.xaml?ExternalTrackId=" + id, UriKind.Relative));
                    }
                    else if (NavigationContext.QueryString.ContainsKey("ExternalCustomPlaylistId"))
                    {
                        var id = NavigationContext.QueryString["ExternalCustomPlaylistId"];

                        string permaLink = string.Empty;

                        if (NavigationContext.QueryString.ContainsKey("PermaLink"))
                            permaLink = NavigationContext.QueryString["PermaLink"];

                        NavigationService.Navigate(new Uri("/SoundcloudCustomPlaylist.xaml?Id=" + id + "&PermaLink=" + permaLink, UriKind.Relative));
                    }
                    else if (NavigationContext.QueryString.ContainsKey("ExternalPlaylistId"))
                    {
                        var id = NavigationContext.QueryString["ExternalPlaylistId"];
                        var title = NavigationContext.QueryString["Title"];
                        NavigationService.Navigate(new Uri("/SoundcloudPlaylistDetails.xaml?ExternalPlaylistId=" + id + "&Title=" + title, UriKind.Relative));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            });
        }

        private void RefreshPlaylistsCompletedEvent(object sender, EventArgs e)
        {

        }

        private void RefreshDashboardCompletedEvent(object sender, EventArgs e)
        {
            UiHelper.SafeDispatch(() =>
            {
                lstDashboard.DataVirtualizationMode = DataVirtualizationMode.OnDemandAutomatic;
                lstDashboard.StopPullToRefreshLoading(true, true);
                UiHelper.HideProgressBar();
            });
        }

        private void RefreshFavouritesCompletedEvent(object sender, EventArgs e)
        {
            UiHelper.SafeDispatch(() =>
                                      {
                                          lstLikes.DataVirtualizationMode = DataVirtualizationMode.OnDemandAutomatic;
                                          lstLikes.StopPullToRefreshLoading(true, true);
                                          UiHelper.HideProgressBar();
                                      });
        }

        private void btn_Refresh(object sender, EventArgs e)
        {

            switch (pivotMain.SelectedIndex)
            {
                case 0: // home
                    //App.ViewModel.SoundcloudDashboard.Clear();
                    UiHelper.ShowProgressBar("refreshing");
                    App.ViewModel.RefreshStream();
                    break;

                case 1: // favourites
                    //App.ViewModel.SoundcloudFavourites.Clear();
                    UiHelper.ShowProgressBar("refreshing");
                    App.ViewModel.RefreshFavourites();
                    break;
            }

        }

        private void btn_Help(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void btn_Profile(object sender, EventArgs e)
        {
            var userId = App.ViewModel.CurrentUserViewModel.Id;

            string uri = "/SoundcloudProfile.xaml?accountId=" + userId + "&userId=" + userId;
            NavigationService.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void btnTrack_Click(object sender, RoutedEventArgs e)
        {
            btnSearch.Focus();
            SearchByTrack();
        }

        private void btnGenre_Click(object sender, RoutedEventArgs e)
        {
            btnSearchGenre.Focus();
            SearchByGenre();
        }

        private void SearchByGenre()
        {
            var searchPhrase = txtQuery.Text.Trim();

            if (string.IsNullOrEmpty(searchPhrase))
                return;

            AddItemToSearchHistory(searchPhrase, SearchTypeEnum.Genre);

            NavigationService.Navigate(new Uri("/SoundcloudSearchResults.xaml?title=" + HttpUtility.UrlEncodeUnicode(searchPhrase) + "&genre=" + HttpUtility.UrlEncodeUnicode(searchPhrase), UriKind.Relative));
        }

        private void SearchByTrack()
        {

            var searchPhrase = txtQuery.Text.Trim();

            if (string.IsNullOrEmpty(searchPhrase))
                return;

            AddItemToSearchHistory(searchPhrase, SearchTypeEnum.Track);

            NavigationService.Navigate(new Uri("/SoundcloudSearchResults.xaml?term=" + HttpUtility.UrlEncodeUnicode(searchPhrase), UriKind.Relative));
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            btnSearchUser.Focus();
            SearchByUsers();
        }

        private void SearchByUsers()
        {
            var searchPhrase = txtQuery.Text.Trim();

            if (string.IsNullOrEmpty(searchPhrase))
                return;

            AddItemToSearchHistory(searchPhrase, SearchTypeEnum.User);

            NavigationService.Navigate(new Uri("/SoundcloudSearchUsersResults.xaml?term=" + HttpUtility.UrlEncodeUnicode(searchPhrase), UriKind.Relative));
        }

        private void AddItemToSearchHistory(string searchPhrase, SearchTypeEnum searchType)
        {
            var historyItem = new SearchHistoryItem
                                  {
                                      Query = searchPhrase,
                                      SearchType = searchType
                                  };

            foreach (var item in App.ViewModel.RecentSearches.ToList())
            {
                if (item.Query == historyItem.Query && item.SearchType == historyItem.SearchType)
                {
                    App.ViewModel.RecentSearches.Remove(item);
                    break;
                }
            }

            App.ViewModel.RecentSearches.Insert(0, historyItem);

            // remove the last one until we only have enough items
            while (App.ViewModel.RecentSearches.Count > 8)
                App.ViewModel.RecentSearches.RemoveAt(App.ViewModel.RecentSearches.Count - 1);

            ThreadPool.QueueUserWorkItem(delegate
                                             {
                                                 App.ViewModel.SaveRecentSearches();
                                             });
        }

        private void genreTap(object sender, GestureEventArgs e)
        {
            var item = lstGenre.SelectedItem as SoundcloudGenre;

            if (item == null)
                return;

            NavigationService.Navigate(new Uri("/SoundcloudSearchResults.xaml?title=" + HttpUtility.UrlEncodeUnicode(item.Title) + "&genre=" + HttpUtility.UrlEncodeUnicode(item.Genre), UriKind.Relative));
        }

        private void recentSearchTap(object sender, RoutedEventArgs routedEventArgs)
        {
            var dataBoundListBox = sender as Button;
            var searchItem = dataBoundListBox.DataContext as SearchHistoryItem;

            switch (searchItem.SearchType)
            {
                case SearchTypeEnum.Genre:
                    NavigationService.Navigate(new Uri("/SoundcloudSearchResults.xaml?title=" + HttpUtility.UrlEncodeUnicode(searchItem.Query) + "&genre=" + HttpUtility.UrlEncodeUnicode(searchItem.Query), UriKind.Relative));
                    break;

                case SearchTypeEnum.Track:
                    NavigationService.Navigate(new Uri("/SoundcloudSearchResults.xaml?term=" + HttpUtility.UrlEncodeUnicode(searchItem.Query), UriKind.Relative));
                    break;

                case SearchTypeEnum.User:
                    NavigationService.Navigate(new Uri("/SoundcloudSearchUsersResults.xaml?term=" + HttpUtility.UrlEncodeUnicode(searchItem.Query), UriKind.Relative));
                    break;
            }
        }

        private void btn_Playlists(object sender, EventArgs e)
        {
            pivotMain.SelectedIndex = 2;
        }

        private void btn_Search(object sender, EventArgs e)
        {
            pivotMain.SelectedIndex = 3;
        }

        private async void moreLikes_Requested(object sender, EventArgs e)
        {
            await App.ViewModel.LoadMoreFavourites();

            if (App.ViewModel.NewFavouritesCount == 0)
            {
                UiHelper.SafeDispatch(() =>
                {
                    lstLikes.DataVirtualizationMode = DataVirtualizationMode.None;
                });
            }
        }

        private async void moreDashboard_Requested(object sender, EventArgs e)
        {

            await App.ViewModel.LoadMoreDashboard();

            if (App.ViewModel.NewDashboardCount == 0)
            {
                UiHelper.SafeDispatch(() =>
                {
                    lstDashboard.DataVirtualizationMode = DataVirtualizationMode.None;
                });
            }
        }

        private void btn_Home(object sender, EventArgs e)
        {
            pivotMain.SelectedIndex = 0;
        }

        private void PivotMain_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (pivotMain.SelectedIndex)
            {
                case 0:
                    ApplicationBar = Resources["menuRefresh"] as ApplicationBar;
                    break;
                case 1:
                    ApplicationBar = Resources["menuRefresh"] as ApplicationBar;
                    break;
                default:
                    ApplicationBar = Resources["menuHome"] as ApplicationBar;
                    break;
            }
        }

        private void LstDashboard_OnRefreshRequested(object sender, EventArgs e)
        {
            App.ViewModel.RefreshStream();
        }

        private void LstLikes_OnRefreshRequested(object sender, EventArgs e)
        {
            App.ViewModel.RefreshFavourites();
        }

        private void btn_Downloads(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Downloads.xaml", UriKind.Relative));
        }

        private void btn_Settings(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void MenuRefresh_OnStateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {

            return;

            // commented out for now as it causes the page to resize and look ropey.
            var appBar = sender as ApplicationBar;

            if (appBar == null)
                return;

            appBar.Opacity = (e.IsMenuVisible) ? 1.0 : 0.85;

        }

    }

}
