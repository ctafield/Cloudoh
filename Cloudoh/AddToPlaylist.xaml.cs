using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.ViewModels.Playlists;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;

namespace Cloudoh
{
    public partial class AddToPlaylist : PhoneApplicationPage
    {

        /// <summary>
        /// if true then we are working with a playlist/set, rather than one track
        /// Details come from -> App.ViewModel.CurrentPlaylist.PlaylistTracks
        /// </summary>
        private bool IsSetMode { get; set; }

        public AddToPlaylist()
        {
            InitializeComponent();

            DataContext = App.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                if (NavigationContext.QueryString.ContainsKey("mode"))
                {
                    if (NavigationContext.QueryString["mode"] == "set")
                    {
                        IsSetMode = true;
                    }
                }
            }

        }

        private async void btnNew_Click(object sender, EventArgs e)
        {
            if (LicenceHelper.IsLicensedForPremium())
                PromptForNewPlaylist();
            else
            {
                await LicenceHelper.PromptPurchase();
            }
        }

        private bool _showingNewPlaylistPrompt;

        private async void PromptForNewPlaylist()
        {
            if (_showingNewPlaylistPrompt)
                return;

            var template = Resources["AddPlaylistTemplate"] as ControlTemplate;

            string addMessage;

            if (IsSetMode)
            {
                addMessage = "add tracks to this playlist";
            }
            else
            {
                addMessage = "add track to this playlist";
            }

            InputPromptClosedEventArgs result;

            try
            {
                _showingNewPlaylistPrompt = true;

                result = await RadInputPrompt.ShowAsync(template, 
                    "new playlist", 
                    new[] { "OK", "Cancel" }, 
                    "", 
                    InputMode.Text, 
                    null, 
                    addMessage, 
                    true);
            }
            finally
            {
                _showingNewPlaylistPrompt = false;
            }

            if (result.Result == DialogResult.Cancel)
                return;

            CreateNewPlaylist(result.Text, result.IsCheckBoxChecked);
        }

        private void CreateNewPlaylist(string newTracklistName, bool addTrackToPlaylist)
        {
            var newPlaylist = new CloudohPlaylist
                                              {
                                                  Title = newTracklistName,
                                                  Id = Guid.NewGuid(),
                                                  PlaylistType = CloudohPlaylistType.User
                                              };

            if (addTrackToPlaylist)
            {

                if (IsSetMode)
                {
                    short newIndex = 0;

                    foreach (var track in App.ViewModel.CurrentPlaylist.PlaylistTracks)
                    {
                        var thisTrack = track.Clone();
                        thisTrack.StreamType = ApplicationConstants.SoundcloudTypeEnum.CustomPlaylistTrack;
                        thisTrack.Index = newIndex++;
                        newPlaylist.Tracks.Add(thisTrack);
                    }
                }
                else
                {
                    var newItem = App.ViewModel.CurrentTrackToAdd.Clone();
                    newItem.StreamType = ApplicationConstants.SoundcloudTypeEnum.CustomPlaylistTrack;
                    newItem.Index = 0;

                    newPlaylist.Tracks.Add(newItem);

                    App.ViewModel.CurrentTrackToAdd = null;
                }
            }

            App.ViewModel.CloudohPlaylists.Add(newPlaylist);

            if (addTrackToPlaylist)
            {
                ThreadPool.QueueUserWorkItem(delegate
                                             {
                                                 var sh = new StorageHelper();
                                                 sh.SaveCustomPlaylists(App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User));

                                                 if (IsSetMode)
                                                     UiHelper.ShowToastDelayed("tracks added to " + newTracklistName);
                                                 else
                                                     UiHelper.ShowToastDelayed("track added to " + newTracklistName);
                                             });
                NavigationService.GoBack();
            }

        }

        private void playlistTap(object sender, ListBoxItemTapEventArgs e)
        {

            // 1. add item to the playlist
            var playlist = e.Item.DataContext as CloudohPlaylist;

            if (playlist == null)
                return;

            if (playlist.PlaylistType != CloudohPlaylistType.User)
            {
                if (playlist.PlaylistType == CloudohPlaylistType.SoundCloud)
                    MessageBox.Show("Sorry, but you can only add tracks to custom Cloudoh playlists. This playlist is stored on SoundCloud.\n\nHopefully we will be able to add the ability to add to SoundCloud playlists soon.", "add to playlist", MessageBoxButton.OK);
                else
                    MessageBox.Show("Sorry, but you can only add tracks to custom Cloudoh playlists. This is a special automatically generated playlist.", "add to playlist", MessageBoxButton.OK);
                return;
            }

            if (IsSetMode)
            {

                foreach (var track in App.ViewModel.CurrentPlaylist.PlaylistTracks)
                {
                    var newItem = track.Clone();
                    newItem.StreamType = ApplicationConstants.SoundcloudTypeEnum.CustomPlaylistTrack;

                    if (playlist.Tracks.All(x => x.Id != newItem.Id))
                    {
                        newItem.Index = playlist.Tracks.Count;
                        playlist.Tracks.Add(newItem);
                    }
                }

            }
            else
            {
                var newItem = App.ViewModel.CurrentTrackToAdd.Clone();
                newItem.StreamType = ApplicationConstants.SoundcloudTypeEnum.CustomPlaylistTrack;

                if (playlist.Tracks.All(x => x.Id != newItem.Id))
                {
                    newItem.Index = playlist.Tracks.Count;
                    playlist.Tracks.Add(newItem);
                }

                App.ViewModel.CurrentTrackToAdd = null;
            }

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                var sh = new StorageHelper();
                sh.SaveCustomPlaylists(App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User));

                // 2. notify
                if (IsSetMode)
                    UiHelper.ShowToastDelayed("tracks added to " + playlist.Title);
                else
                    UiHelper.ShowToastDelayed("track added to " + playlist.Title);
            });

            // 3. return to the details page
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();

        }

    }

}