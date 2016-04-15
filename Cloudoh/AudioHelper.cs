using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Cloudoh;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.ViewModels;
using Microsoft.Devices;
using Microsoft.Phone.BackgroundAudio;

namespace Mitter.Soundcloud
{

    internal class AudioHelper
    {

        public static void PlayTrack(SoundcloudViewModel track, List<SoundcloudViewModel> currentPlayList = null)
        {

            if (track == null)
            {
                return;
            }

            // can we stream?
            if (!track.IsStreamable || string.IsNullOrWhiteSpace(track.StreamingUrl))
            {
                MessageBox.Show("Soundcloud prohibits the streaming of this track from third party clients. Please use the website to listen instead, sorry!", "Streaming not allowed", MessageBoxButton.OK);
                return;
            }

            try
            {

                var streamingUrl = track.StreamingUrl;

                var downloadHelper = new DownloadHelper();
                var streamingUri = downloadHelper.GetRemoteOrCachedUrl(track.Id, streamingUrl);
                var albumArtUri = new Uri(track.ThumbnailImage, UriKind.RelativeOrAbsolute);

                // pass the url as the tag
                var playlistTrack = new AudioTrack(streamingUri, track.Title, track.UserName, string.Empty, albumArtUri, track.PlayerTag, EnabledPlayerControls.All);

                // we paused the current track?
                if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused)
                {
                    if (BackgroundAudioPlayer.Instance.Track.Source.ToString().ToLowerInvariant() == streamingUrl.ToLowerInvariant())
                    {
                        // yes, so just play and exit this
                        BackgroundAudioPlayer.Instance.Play();
                        return;
                    }
                }

                try
                {
                    if (string.IsNullOrEmpty(track.ThumbnailImage))
                    {
                        StreamResourceInfo sri = Application.GetResourceStream(new Uri("/Cloudoh;component/tile_music_resource.png", UriKind.Relative));
                        var stream = sri.Stream;

                        var mediaHistoryItem = new MediaHistoryItem
                        {
                            ImageStream = stream,
                            Source = "",
                            Title = track.Title
                        };

                        mediaHistoryItem.PlayerContext.Add("CloudohKey", track.Id.ToString());
                        MediaHistory.Instance.NowPlaying = mediaHistoryItem;

                        var sh = new SettingsHelper();
                        if (sh.GetSettings().IncludeInMusicHub)
                        {
                            // reuse the same image stream for recent item, need to reset it first however 
                            stream.Seek(0, SeekOrigin.Begin);
                            var recentItem = new MediaHistoryItem
                                             {
                                                 Title = track.Title,
                                                 ImageStream = stream,
                                                 Source = ""
                                             };
                            recentItem.ImageStream = stream;
                            recentItem.PlayerContext.Add("CloudohKey", track.Id.ToString()); // put an ID of the recent item to add                 
                            MediaHistory.Instance.WriteRecentPlay(recentItem);
                        }
                    }
                    else
                    {
                        using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            using (var newFile = storage.OpenFile(track.ThumbnailImage, FileMode.Open))
                            {
                                using (var reader = new StreamReader(newFile))
                                {
                                    var stream = reader.BaseStream;
                                    stream.Seek(0, SeekOrigin.Begin);

                                    var mediaHistoryItem = new MediaHistoryItem
                                                           {
                                                               ImageStream = stream,
                                                               Source = string.Empty,
                                                               Title = track.Title
                                                           };
                                    mediaHistoryItem.PlayerContext.Add("CloudohKey", track.Id.ToString());
                                    MediaHistory.Instance.NowPlaying = mediaHistoryItem;

                                    var sh = new SettingsHelper();
                                    if (sh.GetSettings().IncludeInMusicHub)
                                    {
                                        // reuse the same image stream for recent item, need to reset it first however                                                                                 
                                        stream.Seek(0, SeekOrigin.Begin);
                                        var recentItem = new MediaHistoryItem
                                                         {
                                                             Title = track.Title,
                                                             ImageStream = stream,
                                                             Source = string.Empty
                                                         };
                                        recentItem.ImageStream = stream;
                                        recentItem.PlayerContext.Add("CloudohKey", track.Id.ToString()); // put an ID of the recent item to add                 
                                        MediaHistory.Instance.WriteRecentPlay(recentItem);
                                    }
                                }

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // doesn't matter
                    CrittercismSDK.Crittercism.LogHandledException(e);
                }

                // save the playlist 
                var plHelper = new SoundcloudPlaylistHelper();
                if (track.StreamType == ApplicationConstants.SoundcloudTypeEnum.Favourites)
                {
                    plHelper.SaveCurrentSoundcloudPlaylist(App.ViewModel.SoundcloudFavourites.ToList(), track.Index);
                }
                else if (track.StreamType == ApplicationConstants.SoundcloudTypeEnum.Dashboard)
                {
                    plHelper.SaveCurrentSoundcloudPlaylist(App.ViewModel.SoundcloudDashboard.ToList(), track.Index);
                }
                else if (track.StreamType == ApplicationConstants.SoundcloudTypeEnum.SearchResultsHot)
                {
                    plHelper.SaveCurrentSoundcloudPlaylist(App.ViewModel.SoundcloudSearchResultsHot.ToList(), track.Index);
                }
                else if (track.StreamType == ApplicationConstants.SoundcloudTypeEnum.SearchResultsNew)
                {
                    plHelper.SaveCurrentSoundcloudPlaylist(App.ViewModel.SoundcloudSearchResultsNew.ToList(), track.Index);
                }
                else if (track.StreamType == ApplicationConstants.SoundcloudTypeEnum.PlaylistTrack)
                {
                    plHelper.SaveCurrentSoundcloudPlaylist(App.ViewModel.CurrentPlaylist.PlaylistTracks.ToList(), track.Index);
                }
                else if (track.StreamType == ApplicationConstants.SoundcloudTypeEnum.CustomPlaylistTrack)
                {
                    plHelper.SaveCurrentSoundcloudPlaylist(currentPlayList, track.Index);
                }
                else if (track.StreamType == ApplicationConstants.SoundcloudTypeEnum.DownloadPlaylist)
                {
                    plHelper.SaveCurrentSoundcloudPlaylist(currentPlayList, track.Index);
                }
                else if (track.StreamType == ApplicationConstants.SoundcloudTypeEnum.SoundcloudPlaylistTrack)
                {
                    plHelper.SaveCurrentSoundcloudPlaylist(currentPlayList, track.Index);
                }
                else if (currentPlayList != null)
                {
                    plHelper.SaveCurrentSoundcloudPlaylist(currentPlayList, track.Index);
                }

                plHelper.SaveRecentPlaylist(track);

                BackgroundAudioPlayer.Instance.Volume = 1;
                BackgroundAudioPlayer.Instance.Track = playlistTrack;
                BackgroundAudioPlayer.Instance.Play();

            }
            catch (Exception)
            {
                //BugSense.BugSenseHandler.Instance.LogException(ex, "unable to play track : " + track.StreamingUrl);
            }

        }

        private static Stream GetResizedImage(string fileName)
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var newFile = storage.OpenFile(fileName, FileMode.Open))
                {

                    var factory = BitmapFactory.New(200, 200);
                    factory.SetSource(newFile);
                    Stream memoryStream = new MemoryStream();
                    factory.SaveJpeg(memoryStream, 200, 200, 0, 80);
                    return memoryStream;
                }
            }
        }
    }

}