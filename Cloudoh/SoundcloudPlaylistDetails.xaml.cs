using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.ExtensionMethods;
using Cloudoh.UserControls;
using Cloudoh.ViewModels;
using FieldOfTweets.Common.Api.Responses.Soundcloud;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mitter.Soundcloud;
using Telerik.Windows.Controls;

namespace Cloudoh
{
    public partial class SoundcloudPlaylistDetails : PhoneApplicationPage
    {

        private bool DataLoaded { get; set; }

        public SoundcloudPlaylistDetails()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (DataLoaded)
                return;

            LoadData(e);
        }

        private void LoadData(NavigationEventArgs navigationEventArgs)
        {

            try
            {
                foreach (ApplicationBarIconButton item in ApplicationBar.Buttons)
                {
                    item.IsEnabled = false;
                }
            }
            catch (Exception)
            {
            }

            try
            {

                if (navigationEventArgs.NavigationMode == NavigationMode.New)
                {
                    if (NavigationContext.QueryString.ContainsKey("ExternalPlaylistId"))
                    {
                        var trackId = int.Parse(NavigationContext.QueryString["ExternalPlaylistId"]);
                        var title = NavigationContext.QueryString["Title"];

                        if (!App.ViewModel.IsDataLoaded)
                        {
                            App.ViewModel.LoadData();
                        }

                        App.ViewModel.CurrentPlaylist = new SoundcloudViewModel()
                                                        {
                                                            Id = trackId,
                                                            Title = title
                                                        };
                    }
                }
                else
                {
                    App.ViewModel.CurrentPlaylist.PlaylistTracks = new ObservableCollection<SoundcloudViewModel>();

                    this.DataContext = App.ViewModel.CurrentPlaylist;

                    DataContext = App.ViewModel.CurrentPlaylist;
                }

                ThreadPool.QueueUserWorkItem(GetTrackList);

            }
            catch (Exception)
            {
            }

        }

        private async void GetTrackList(object state)
        {
            UiHelper.ShowProgressBar("getting tracks");

            var api = new SoundcloudApi();
            //api.GetTracksForPlaylistCompletedEvent += api_GetTracksForPlaylistCompletedEvent;
            var result = await api.GetTracksForPlaylist(App.ViewModel.CurrentPlaylist.Id);
            api_GetTracksForPlaylistCompletedEvent(result);

        }

        private void api_GetTracksForPlaylistCompletedEvent(ResponsePlaylist playlist)
        {
            if (playlist == null || playlist.tracks == null)
                return;

            int currentIndex = 0;

            UiHelper.SafeDispatch(() =>
                                      {
                                          try
                                          {

                                              if (DataContext == null)
                                              {
                                                  App.ViewModel.CurrentPlaylist.PlaylistTracks = new ObservableCollection<SoundcloudViewModel>();

                                                  App.ViewModel.CurrentPlaylist.Description = playlist.description;

                                                  App.ViewModel.CurrentPlaylist.AlbumArt = string.IsNullOrEmpty(playlist.artwork_url)
                                                      ? playlist.user.avatar_url
                                                      : playlist.artwork_url;

                                                  // rebind
                                                  DataContext = App.ViewModel.CurrentPlaylist;
                                              }

                                              foreach (var track in playlist.tracks)
                                              {
                                                  var model = track.AsViewModel(currentIndex++, ApplicationConstants.SoundcloudTypeEnum.PlaylistTrack);
                                                  App.ViewModel.CurrentPlaylist.PlaylistTracks.Add(model);
                                              }

                                              foreach (ApplicationBarIconButton item in ApplicationBar.Buttons)
                                              {
                                                  item.IsEnabled = true;
                                              }

                                              borderName.Visibility = Visibility.Visible;

                                          }
                                          catch (Exception)
                                          {

                                          }
                                          finally
                                          {
                                              UiHelper.HideProgressBar();
                                          }

                                      });

        }

        private void mnuPlay_Click(object sender, EventArgs e)
        {

            var firstItem = App.ViewModel.CurrentPlaylist.PlaylistTracks.FirstOrDefault();

            if (firstItem != null)
            {
                AudioHelper.PlayTrack(firstItem, App.ViewModel.CurrentPlaylist.PlaylistTracks.ToList());
            }

        }

        private void mnuAdd_Click(object sender, EventArgs e)
        {
            UiHelper.NavigateTo("/AddToPlaylist.xaml?mode=set");
        }

        private void mnuPinToStart_Click(object sender, EventArgs e)
        {

            SoundcloudViewModel firstTrack = null;

            try
            {
                firstTrack = App.ViewModel.CurrentPlaylist.PlaylistTracks.FirstOrDefault();
            }
            catch (Exception)
            {
            }

            if (firstTrack == null)
            {
                MessageBox.Show("Sorry! We can't pin this as there are no tracks in this set.", "Pin to start", MessageBoxButton.OK);
                return;
            }

            var tile = new CloudohTile();
            tile.SetValues(firstTrack);
            tile.UpdateLayout();

            var tileSmall = new CloudohTileSmall();
            tileSmall.SetValues(firstTrack);
            tileSmall.UpdateLayout();

            var newTile = new RadFlipTileData()
            {
                SmallVisualElement = tileSmall,
                VisualElement = tile,
                Title = "",
                MeasureMode = MeasureMode.Element,
                IsTransparencySupported = false
            };

            LiveTileHelper.CreateOrUpdateTile(newTile, new Uri("/MainPage.xaml?ExternalPlaylistId=" + App.ViewModel.CurrentPlaylist.Id + "&Title=" + Uri.EscapeUriString(App.ViewModel.CurrentPlaylist.Title), UriKind.Relative), false);

        }

    }

}