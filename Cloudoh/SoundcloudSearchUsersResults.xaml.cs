using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Cloudoh.Classes;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.ExtensionMethods;
using Cloudoh.ViewModels;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;

namespace Cloudoh
{

    public partial class SoundcloudSearchUsersResults : PhoneApplicationPage
    {

        private string Query;
        private bool LoadedData { get; set; }
        public ObservableCollection<SoundcloudUserViewModel> SearchResults { get; set; }

        public SoundcloudSearchUsersResults()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);

            if (!NavigationContext.QueryString.TryGetValue("term", out Query))
                NavigationService.GoBack();

            Query = HttpUtility.UrlDecode(Query);

            if (!LoadedData)
            {
                SearchResults = new ObservableCollection<SoundcloudUserViewModel>();
                DataContext = this;
                PerformSearch(true);
            }

        }

        private void PerformSearch(bool setIndicator)
        {

            UiHelper.SafeDispatch(() =>
            {
                txtQuery.Text = Query.ToLower();
                lstSearchResults.Visibility = Visibility.Collapsed;
                noResults.Visibility = Visibility.Collapsed;
            });

            if (setIndicator)
            {
                UiHelper.ShowProgressBar("searching for " + Query);
            }

            DoTheSearch();

            LoadedData = true;
        }

        private void DoTheSearch()
        {
            var api = new SoundcloudApi();
            api.SearchByUserCompletedEvent += ApiOnSearchByTrackCompletedEvent;
            api.SearchByUser(Query);
        }

        private void ApiOnSearchByTrackCompletedEvent(object sender, EventArgs eventArgs)
        {

            UiHelper.HideProgressBar();

            var api = sender as SoundcloudApi;

            if (api == null)
                return;

            UiHelper.SafeDispatch(() =>
            {
                // todo: change to whatever we use
                if (api.SearchUsers != null && api.SearchUsers.Count > 0)
                {

                    foreach (var user in api.SearchUsers)
                    {
                        SearchResults.Add(user.AsViewModel());
                    }

                    noResults.Visibility = Visibility.Collapsed;
                    lstSearchResults.Visibility = Visibility.Visible;
                }
                else
                {
                    noResults.Visibility = Visibility.Visible;
                    lstSearchResults.Visibility = Visibility.Collapsed;
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

            App.ViewModel.CurrentSoundcloudProfile = model;

            box.SelectedItem = null;

            NavigationService.Navigate(new Uri("/SoundcloudProfile.xaml?userId=" + model.Id, UriKind.Relative));
        }

    }

}