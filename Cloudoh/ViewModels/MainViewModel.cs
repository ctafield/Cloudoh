using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cloudoh.Annotations;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.ExtensionMethods;
using Cloudoh.ViewModels.Playlists;
using FieldOfTweets.Common.Api.Responses.Soundcloud;
using Microsoft.Phone.Shell;

namespace Cloudoh.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public SearchSettings SearchSettings { get; set; }

        public ObservableCollection<SearchHistoryItem> RecentSearches { get; set; }

        public SortedObservableCollection<SoundcloudViewModel> SoundcloudDashboard { get; set; }
        public ObservableCollection<SoundcloudViewModel> SoundcloudFavourites { get; set; }
        public ObservableCollection<SoundcloudViewModel> SoundcloudSearchResultsHot { get; set; }
        public ObservableCollection<SoundcloudViewModel> SoundcloudSearchResultsNew { get; set; }

        public ObservableCollection<CloudohPlaylist> CloudohPlaylists { get; set; }

        public List<SoundcloudGenre> Genres { get; private set; }

        public SoundcloudViewModel CurrentPlaylist { get; set; }

        // This is the current user
        public SoundcloudAccessViewModel CurrentUserViewModel { get; set; }

        // Used for passing between screens
        public SoundcloudUserViewModel CurrentSoundcloudProfile { get; set; }

        public SoundcloudViewModel CurrentSoundcloud { get; set; }
        public SoundcloudViewModel CurrentTrackToAdd { get; set; }

        public int? NewDashboardCount { get; set; }
        public int? NewFavouritesCount { get; set; }

        public MainViewModel()
        {
            SoundcloudDashboard = new SortedObservableCollection<SoundcloudViewModel>();
            SoundcloudFavourites = new ObservableCollection<SoundcloudViewModel>();
            RecentSearches = new ObservableCollection<SearchHistoryItem>();

            CloudohPlaylists = new ObservableCollection<CloudohPlaylist>();

            SearchSettings = new SearchSettings();
        }

        private List<SoundcloudGenre> CreateGenres()
        {
            return new List<SoundcloudGenre>
                       {
                           new SoundcloudGenre("Urban", "rap,hiphop,grime,r&b,soul,funk", new Uri("/Images/Genres/hiphop.jpg", UriKind.Relative)),
                           new SoundcloudGenre("Drum & Bass", "dnb,drum & bass,drumbass,neurofunk,liquid", new Uri("/Images/Genres/dnb.png", UriKind.Relative)),
                           new SoundcloudGenre("Electronic", "trance,goa,electronic,house,techno", new Uri("/Images/Genres/trance.jpg", UriKind.Relative)),
                           new SoundcloudGenre("Pop", "pop,80s,electronic pop", new Uri("/Images/Genres/pop.jpg", UriKind.Relative)),
                           new SoundcloudGenre("Rock", "rock,hard rock,grunge", new Uri("/Images/Genres/rock.jpg", UriKind.Relative)),
                           new SoundcloudGenre("Metal", "metal,metalcore,thash metal,death metal", new Uri("/Images/Genres/metal.jpg", UriKind.Relative)),
                           new SoundcloudGenre("Reggae", "reggae,dancehall,reggaeton,riddim,dub", new Uri("/Images/Genres/reggae.jpg", UriKind.Relative)),
                           new SoundcloudGenre("Country", "country", new Uri("/Images/Genres/country.jpg", UriKind.Relative)),
                           new SoundcloudGenre("Classical", "classical,piano,opera,choir,classic", new Uri("/Images/Genres/classical.jpg", UriKind.Relative)),
                           new SoundcloudGenre("Jazz & Blues", "jazz,blues", new Uri("/Images/Genres/jazz.jpg", UriKind.Relative))
                       };
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            IsFirstLoad = true;

            var task = Task.Run(async () =>
            {
                await LoadDataThreaded();

                IsDataLoaded = true;
            });

            task.Wait();

        }

        private bool IsFirstLoad;

        private async Task LoadDataThreaded()
        {

            // Check there is a user
            var sh = new StorageHelper();
            var user = sh.GetSoundcloudUser();

            if (user == null)
                return;

            CurrentUserViewModel = new SoundcloudAccessViewModel(user);

            // these two need to be on the money, right now
            var streamObject = sh.LoadContentsFromFile<ObservableCollection<SoundcloudViewModel>>(ApplicationConstants.SoundcloudLastStream);
            var favouritesObject = sh.LoadContentsFromFile<ObservableCollection<SoundcloudViewModel>>(ApplicationConstants.SoundcloudLastFavourites);

            //UiHelper.SafeDispatchSync(() =>
            //                          {

            if (streamObject != null)
            {
                foreach (var item in streamObject)
                    App.ViewModel.SoundcloudDashboard.Add(item);
            }

            if (favouritesObject != null)
            {
                foreach (var soundcloudViewModel in favouritesObject)
                {
                    App.ViewModel.SoundcloudFavourites.Add(soundcloudViewModel);
                }
            }


            //ThreadPool.QueueUserWorkItem(delegate(object state1)
            //                                 {
            RefreshStream();
            RefreshFavourites();
            RefreshPlaylists();
            //});

            //});

            //ThreadPool.QueueUserWorkItem(delegate(object state2)
            //                                 {
            LoadOtherItems();
            //});

        }


        private async Task LoadOtherItems()
        {

            var sh = new StorageHelper();

            Genres = CreateGenres();

            SearchSettings = sh.LoadContentsFromFile<SearchSettings>(ApplicationConstants.SearchSettingsFile);

            if (SearchSettings == null)
                SearchSettings = new SearchSettings();

            var searchesObject = sh.LoadContentsFromFile<ObservableCollection<SearchHistoryItem>>(ApplicationConstants.SoundcloudRecentSearches);
            var playlistsObject = sh.LoadContentsFromFile<ObservableCollection<CloudohPlaylist>>(ApplicationConstants.SoundcloudPlaylists);

            //UiHelper.SafeDispatch(() =>
            //{

            if (searchesObject != null)
            {
                foreach (var searchHistoryItem in searchesObject)
                {
                    App.ViewModel.RecentSearches.Add(searchHistoryItem);
                }
            }

            AddAutoGeneratedPlaylists();

            if (playlistsObject != null)
            {
                foreach (var cloudohPlaylist in playlistsObject)
                {
                    App.ViewModel.CloudohPlaylists.Add(cloudohPlaylist);
                }
            }

            // });

        }

        private void AddAutoGeneratedPlaylists()
        {
            // Downloads
            AddDownloadPlaylist();
        }

        private void AddDownloadPlaylist()
        {
            var downloadHelper = new DownloadHelper();
            var finishedDownloadQueue = downloadHelper.GetFinishedDownloads();

            var downloadPlaylist = new CloudohPlaylist()
                                        {
                                            Title = "Downloads",
                                            PlaylistType = CloudohPlaylistType.Downloaded,
                                            PermaLink = "zzz_cloudoh_downloads_playlist"
                                        };

            int index = 0;

            foreach (var finished in finishedDownloadQueue)
            {
                var item = finished.Tag.Clone();
                item.Index = index++;
                item.StreamType = ApplicationConstants.SoundcloudTypeEnum.DownloadPlaylist;
                downloadPlaylist.Tracks.Add(item);
            }

            if (downloadPlaylist.Tracks.Any())
                App.ViewModel.CloudohPlaylists.Add(downloadPlaylist);

            var ds = new StorageHelper();
            var temp = ds.LoadContentsFromFile<CloudohPlaylist>(ApplicationConstants.SoundcloudRecentPlays);
            
            if (temp != null)
                App.ViewModel.CloudohPlaylists.Add(temp);


        }

        public void UpdateDownloadPlaylist()
        {
            var downloadPlaylist = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.PlaylistType == CloudohPlaylistType.Downloaded);
            if (downloadPlaylist != default(CloudohPlaylist))
            {
                App.ViewModel.CloudohPlaylists.Remove(downloadPlaylist);
            }
            AddDownloadPlaylist();
        }

        public event EventHandler RefreshPlaylistsCompletedEvent;

        private async Task RefreshPlaylists()
        {
            var api = new SoundcloudApi();
            var result = await api.GetUsersPlaylists(CurrentUserViewModel.Id);

            api_GetUsersPlaylistsCompletedEvent(result);

            if (RefreshPlaylistsCompletedEvent != null)
                RefreshPlaylistsCompletedEvent(this, null);

        }

        public event EventHandler RefreshFavouritesCompletedEvent;

        public async Task RefreshFavourites()
        {
            var api = new SoundcloudApi();

            var results = await api.GetFavourites();

            api_GetFavouritesCompletedEvent(results, api.FetchingMoreFavourites);

            if (RefreshFavouritesCompletedEvent != null)
                RefreshFavouritesCompletedEvent(this, null);
        }

        public event EventHandler RefreshDashboardCompletedEvent;

        public async Task RefreshStream()
        {
            var api = new SoundcloudApi();

            var result = await api.GetDashboard();

            api_GetDashboardCompletedEvent(result, api.FetchingMoreDashboard);

            if (RefreshDashboardCompletedEvent != null)
                RefreshDashboardCompletedEvent(this, null);
        }


        private void api_GetUsersPlaylistsCompletedEvent(IEnumerable<ResponsePlaylist> usersPlaylists)
        {

            if (usersPlaylists == null)
                return;

            UiHelper.SafeDispatch(() =>
            {

                foreach (var playlist in usersPlaylists)
                {
                    var currentIndex = 0;

                    var thisPlaylist = new CloudohPlaylist()
                    {
                        Title = playlist.title,
                        PlaylistType = CloudohPlaylistType.SoundCloud,
                        PermaLink = playlist.permalink
                    };

                    if (playlist.tracks != null)
                    {
                        foreach (var track in playlist.tracks)
                        {
                            var model = track.AsViewModel(currentIndex++, ApplicationConstants.SoundcloudTypeEnum.PlaylistTrack);
                            model.StreamType = ApplicationConstants.SoundcloudTypeEnum.SoundcloudPlaylistTrack;
                            thisPlaylist.Tracks.Add(model);
                        }
                    }

                    App.ViewModel.CloudohPlaylists.Add(thisPlaylist);
                }

            });

        }

        void api_GetDashboardCompletedEvent(ResponseDashboard dashboard, bool fetchingMoreDashboard)
        {

            if (dashboard == null || dashboard.collection == null || dashboard.collection.Count == 0)
                return;

            var currentIndex = 0;

            UiHelper.SafeDispatchSync(() =>
            {

                var api = new SoundcloudApi();

                try
                {

                    var newItems = new List<SoundcloudViewModel>();

                    foreach (var track in dashboard.collection.Where(x => x.origin != null && !string.IsNullOrEmpty(x.type)))
                    {

                        if (track.type == "track")
                        {

                            var trackModel = new SoundcloudViewModel
                                                 {
                                                     Type = track.type,
                                                     StreamType = ApplicationConstants.SoundcloudTypeEnum.Dashboard,
                                                     AlbumArt = string.IsNullOrEmpty(track.origin.artwork_url)
                                                                    ? track.origin.user.avatar_url
                                                                    : track.origin.artwork_url,
                                                     Id = track.origin.id,
                                                     StreamingUrl = api.GetTrackUrl(track.origin.stream_url),
                                                     IsStreamable = (track.origin.streamable.HasValue && track.origin.streamable.Value),
                                                     Title = track.origin.title,
                                                     UserName = track.origin.user.username,
                                                     UserId = track.origin.user.id,
                                                     AvatarUrl = track.origin.user.avatar_url,
                                                     Description = track.origin.description,
                                                     Genre = track.origin.genre,
                                                     WaveformUrl = track.origin.waveform_url,
                                                     PermalinkUrl = track.origin.permalink_url,
                                                     Duration = track.origin.duration,
                                                     LikeCount = track.origin.favoritings_count,
                                                     PlayCount = track.origin.playback_count,
                                                     CommentCount = track.origin.comment_count,
                                                     DownloadCount = track.origin.download_count,
                                                     TrackCreatedAt = track.created_at,
                                                     Index = currentIndex,
                                                     DownloadUrl = track.origin.download_url,
                                                     Downloadable = track.origin.downloadable.GetValueOrDefault()
                                                 };

                            currentIndex++;

                            if (SoundcloudDashboard.All(x => x.Id != trackModel.Id))
                            {
                                App.ViewModel.SoundcloudDashboard.Add(trackModel);
                            }

                            newItems.Add(trackModel);

                        }
                        else if (track.type == "playlist")
                        {
                            var trackModel = track.AsViewModelFromPlaylist(currentIndex);

                            currentIndex++;

                            if (SoundcloudDashboard.All(x => x.Id != trackModel.Id))
                            {
                                App.ViewModel.SoundcloudDashboard.Add(trackModel);
                            }

                            newItems.Add(trackModel);

                        }

                    }

                    if (!fetchingMoreDashboard)
                    {
                        ThreadPool.QueueUserWorkItem(StoreLastStream, newItems);
                        ThreadPool.QueueUserWorkItem(UpdateLiveTile, App.ViewModel.SoundcloudDashboard.ToList());
                    }

                }
                catch (Exception)
                {
                    //blah
                }

            });

        }

        internal void UpdateLiveTile(object state)
        {
            var items = state as List<SoundcloudViewModel>;

            if (items == null)
                return;

            Uri[] images;

            try
            {
                images = items.Select(x => x.AlbumArtImageSource)
                              .Where(x => x != null)
                              .Distinct()
                              .Take(9)
                              .ToArray();
                if (!images.Any())
                    return;

            }
            catch (Exception)
            {
                return;
            }

            try
            {
                var sh = new StorageHelper();
                Uri[] newImageLocations = sh.CopyAlbumArtToShellFolder(images);

                var cycleTile = new CycleTileData()
                {
                    Title = "Cloudoh",
                    Count = 0,
                    SmallBackgroundImage = GetSmallTile(),
                    CycleImages = newImageLocations
                };

                ShellTile tileToFind = ShellTile.ActiveTiles.FirstOrDefault();
                if (tileToFind != default(ShellTile))
                    tileToFind.Update(cycleTile);
            }
            catch (Exception)
            {
            }

        }

        private Uri GetSmallTile()
        {
            var sh = new SettingsHelper();
            var settings = sh.GetSettings();
            return new Uri(settings.TransparentSmallTile ? "/tile_small_transparent.png" : "/tile_small.png", UriKind.Relative);            
        }

        private void api_GetFavouritesCompletedEvent(List<ResponseGetTrack> favourites, bool fetchingMoreFavourites)
        {

            UiHelper.SafeDispatchSync(() =>
                                      {

                                          try
                                          {
                                              if (favourites == null || favourites.Count == 0)
                                                  return;

                                              if (IsFirstLoad)
                                              {
                                                  App.ViewModel.SoundcloudFavourites.Clear();
                                                  IsFirstLoad = false;
                                              }

                                              var currentIndex = 0;

                                              // only insert if its a clean refresh, not getting more
                                              bool insert = !fetchingMoreFavourites && App.ViewModel.SoundcloudFavourites.Count > 0;

                                              var newItems = new List<SoundcloudViewModel>();

                                              foreach (var track in favourites)
                                              {
                                                  var model = track.AsViewModel(currentIndex++, ApplicationConstants.SoundcloudTypeEnum.Favourites);
                                                  if (App.ViewModel.SoundcloudFavourites.All(x => x.Id != model.Id))
                                                      if (insert)
                                                          App.ViewModel.SoundcloudFavourites.Insert(0, model);
                                                      else
                                                          App.ViewModel.SoundcloudFavourites.Add(model);

                                                  newItems.Add(model);
                                              }

                                              // dont want to save the items if weve just fetched more at the end of the list
                                              if (!fetchingMoreFavourites)
                                                  ThreadPool.QueueUserWorkItem(StoreLastFavourites, newItems);
                                          }
                                          catch (Exception)
                                          {
                                          }

                                      });

        }

        private object _storeLastStreamLock = new object();
        private void StoreLastStream(object state)
        {
            lock (_storeLastStreamLock)
            {
                var newItems = state as List<SoundcloudViewModel>;
                var sh = new StorageHelper();
                sh.SaveContentsToFile(ApplicationConstants.SoundcloudLastStream, newItems);
            }
        }

        private object _storeLastFavouritesLock = new object();
        private void StoreLastFavourites(object state)
        {
            lock (_storeLastFavouritesLock)
            {
                var newItems = state as List<SoundcloudViewModel>;
                var sh = new StorageHelper();
                sh.SaveContentsToFile(ApplicationConstants.SoundcloudLastFavourites, newItems);
            }
        }

        private object _storeRecentSearchesLock = new object();
        public void SaveRecentSearches()
        {
            try
            {
                lock (_storeRecentSearchesLock)
                {
                    var sh = new StorageHelper();
                    sh.SaveContentsToFile(ApplicationConstants.SoundcloudRecentSearches, RecentSearches);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadMoreFavourites()
        {
            var api = new SoundcloudApi();

            var offset = this.SoundcloudFavourites.Count;

            var results = await api.GetFavourites(offset);

            var previousCount = SoundcloudFavourites.Count;

            api_GetFavouritesCompletedEvent(results, api.FetchingMoreFavourites);

            var currentCount = SoundcloudFavourites.Count;

            if (api.FetchingMoreFavourites)
                NewFavouritesCount = currentCount - previousCount;

        }


        public async Task LoadMoreDashboard()
        {
            var api = new SoundcloudApi();
            var offset = this.SoundcloudDashboard.Count;

            var result = await api.GetDashboard(offset);

            var previousCount = SoundcloudDashboard.Count;

            api_GetDashboardCompletedEvent(result, api.FetchingMoreDashboard);

            var currentCount = SoundcloudDashboard.Count;

            if (api.FetchingMoreDashboard)
                NewDashboardCount = currentCount - previousCount;

        }

    }

}
