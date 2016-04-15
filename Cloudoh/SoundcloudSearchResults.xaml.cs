using System;
using System.Collections.ObjectModel;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.ExtensionMethods;
using Cloudoh.ViewModels;
using Microsoft.Phone.Controls;

namespace Cloudoh
{
    public partial class SoundcloudSearchResults : PhoneApplicationPage
    {

        private string _title;
        private string _query;        
        private string _genre;
        private string _order;

        private bool LoadedData { get; set; }

        public SoundcloudSearchResults()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("term", out _query))
                _query = HttpUtility.UrlDecode(_query);

            if (NavigationContext.QueryString.TryGetValue("genre", out _genre))
            {
                _genre = HttpUtility.UrlDecode(_genre);
                NavigationContext.QueryString.TryGetValue("title", out _title);
            }

            if (string.IsNullOrEmpty(_query) && string.IsNullOrEmpty(_genre))
                NavigationService.GoBack();

            if (!LoadedData)
            {
                App.ViewModel.SoundcloudSearchResultsHot = new ObservableCollection<SoundcloudViewModel>();
                App.ViewModel.SoundcloudSearchResultsNew = new ObservableCollection<SoundcloudViewModel>();

                _order = "hotness";

                DataContext = null;
                DataContext = App.ViewModel;
                PerformSearch(true);
            }

        }

        private void PerformSearch(bool setIndicator)
        {

            UiHelper.SafeDispatch(() =>
            {
                var query = _query ?? _title;
                txtQuery.Text = query.ToLower();
                noResults.Visibility = Visibility.Collapsed;

                if (App.ViewModel.SearchSettings.IsFiltered)
                {
                    borderFiltered.Visibility = Visibility.Visible;
                }
                else
                {
                    borderFiltered.Visibility = Visibility.Collapsed;
                }

            });

            DoTheSearch();

            LoadedData = true;
        }

        private void DoTheSearch()
        {

            var api = new SoundcloudApi();

            if (_order == "hotness")
                api.SearchByTrackCompletedEvent += ApiOnSearchByTrackCompletedEvent;
            else
                api.SearchByTrackCompletedEvent += ApiOnSearchByTrackCompletedEventNew;

            // get the search parameters
            short? minBpm = null;
            short? maxBpm = null;
            TimeSpan? minLength = null;
            TimeSpan? maxLength = null;
            bool? filterDownloadable;

            filterDownloadable = App.ViewModel.SearchSettings.DownloadableOnly;
            if (App.ViewModel.SearchSettings.FilterOnBpm.GetValueOrDefault())
            {
                minBpm = App.ViewModel.SearchSettings.BpmMinimum;
                maxBpm = App.ViewModel.SearchSettings.BpmMaximum;
            }

            if (App.ViewModel.SearchSettings.FilterOnDuration.GetValueOrDefault())
            {
                minLength = App.ViewModel.SearchSettings.MinDuration;
                maxLength = App.ViewModel.SearchSettings.MaxDuration;
            }

            if (!string.IsNullOrEmpty(_query))
            {
                api.SearchByTrack(_query, _order, filterDownloadable, minLength, maxLength, minBpm, maxBpm);
            }
            else
            {
                api.SearchByGenre(_genre, _order, filterDownloadable, minLength, maxLength, minBpm, maxBpm);
            }
        }

        private void ApiOnSearchByTrackCompletedEvent(object sender, EventArgs eventArgs)
        {

            var api = sender as SoundcloudApi;

            if (api == null)
                return;

            UiHelper.SafeDispatch(() =>
            {
                if (api.SearchTracks != null && api.SearchTracks.Count > 0)
                {
                    var index = 0;

                    foreach (var track in api.SearchTracks)
                    {
                        var model = track.AsViewModel(index, ApplicationConstants.SoundcloudTypeEnum.SearchResultsHot);
                        App.ViewModel.SoundcloudSearchResultsHot.Add(model);
                        index++;
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

        private void ApiOnSearchByTrackCompletedEventNew(object sender, EventArgs eventArgs)
        {

            var api = sender as SoundcloudApi;

            if (api == null)
                return;

            UiHelper.SafeDispatch(() =>
            {
                if (api.SearchTracks != null && api.SearchTracks.Count > 0)
                {
                    var index = 0;

                    foreach (var track in api.SearchTracks)
                    {
                        var model = track.AsViewModel(index, ApplicationConstants.SoundcloudTypeEnum.SearchResultsNew);
                        App.ViewModel.SoundcloudSearchResultsNew.Add(model);
                        index++;
                    }

                    noResultsNew.Visibility = Visibility.Collapsed;
                    lstSearchResultsNew.Visibility = Visibility.Visible;
                }
                else
                {
                    noResultsNew.Visibility = Visibility.Visible;
                    lstSearchResultsNew.Visibility = Visibility.Collapsed;
                }
            });

        }

        private void pivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (e.RemovedItems != null)
            {
                if (e.RemovedItems.Count == 1 && e.RemovedItems[0] == null)
                    return;
            }

            switch (pivotMain.SelectedIndex)
            {
                case 0:
                    // hot
                    if (App.ViewModel.SoundcloudSearchResultsHot == null || App.ViewModel.SoundcloudSearchResultsHot.Count == 0)
                    {
                        _order = "hotness";
                        DoTheSearch();
                    }
                    break;

                case 1:
                    // new
                    if (App.ViewModel.SoundcloudSearchResultsNew == null || App.ViewModel.SoundcloudSearchResultsNew.Count == 0)
                    {
                        _order = "created_at";
                        DoTheSearch();                        
                    }
                    break;
            }
            
        }

        private void mnuSettings_Click(object sender, EventArgs e)
        {

            // Clear the results
            LoadedData = false;
            App.ViewModel.SoundcloudSearchResultsHot.Clear();
            App.ViewModel.SoundcloudSearchResultsNew.Clear();

            NavigationService.Navigate(new Uri("/Settings.xaml?tab=search", UriKind.Relative));
        }
    }
}