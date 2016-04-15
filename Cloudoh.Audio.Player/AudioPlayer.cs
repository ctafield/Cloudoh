using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.Common.ErrorLogging;
using Microsoft.Devices;
using Microsoft.Phone.BackgroundAudio;

namespace Cloudoh.Audio.Player
{
    public class AudioPlayer : AudioPlayerAgent
    {

        private static volatile bool _classInitialized;

        /// <remarks>
        /// AudioPlayer instances can share the same process. 
        /// Static fields can be used to share state between AudioPlayer instances
        /// or to communicate with the Audio Streaming agent.
        /// </remarks>
        public AudioPlayer()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;

                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += AudioPlayer_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void AudioPlayer_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }

            ErrorLogger.LogException("AudioPlayer_UnhandledException", e.ExceptionObject);
        }

        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback.
        /// 
        /// Notable playstate events: 
        /// (a) TrackEnded: invoked when the player has no current track. The agent can set the next track.
        /// (b) TrackReady: an audio track has been set and it is now ready for playack.
        /// 
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {

            Debug.WriteLine("OnPlayStateChanged." + playState);

            try
            {
                switch (playState)
                {
                    case PlayState.TrackEnded:
                        player.Track = GetNextTrack();
                        break;

                    case PlayState.TrackReady:
                        if (player.PlayerState != PlayState.Playing)
                        {
                            player.Play();
                        }
                        break;

                    case PlayState.Shutdown:
                        // TODO: Handle the shutdown state here (e.g. save state)
                        break;

                    case PlayState.Unknown:
                        break;

                    case PlayState.Stopped:
                        break;

                    case PlayState.Paused:
                        break;

                    case PlayState.Playing:
                        break;

                    case PlayState.BufferingStarted:
                        break;

                    case PlayState.BufferingStopped:
                        break;

                    case PlayState.Rewinding:
                        break;

                    case PlayState.FastForwarding:
                        break;

                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException("OnPlayStateChanged", ex);

                player = null;
            }
            finally
            {
#if DEBUG
                if (player != null)
                {
                    if (player.Track != null)
                    {
                        Debug.WriteLine("OnPlayStateChanged.Playing : " + player.Track.Title);
                    }
                }
#endif

                NotifyComplete();
            }


        }


        /// <summary>
        /// Called when the user requests an action using application/system provided UI
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported.
        /// 
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {


            Debug.WriteLine("OnUserAction." + action);

            try
            {
                switch (action)
                {
                    case UserAction.Play:
                        if (player.PlayerState != PlayState.Playing)
                        {
                            player.Play();
                        }
                        break;
                    case UserAction.Stop:
                        player.Stop();
                        break;
                    case UserAction.Pause:
                        if (player.PlayerState == PlayState.Playing)
                            player.Pause();
                        break;
                    case UserAction.FastForward:
                        player.FastForward();
                        break;
                    case UserAction.Rewind:
                        player.Rewind();
                        break;
                    case UserAction.Seek:
                        player.Position = (TimeSpan)param;
                        break;
                    case UserAction.SkipNext:
                        player.Track = GetNextTrack();
                        break;
                    case UserAction.SkipPrevious:
                        if (player.PlayerState == PlayState.Playing && !IsNearStart(player, track))
                        {
                            player.Position = new TimeSpan(0);
                        }
                        else
                        {
                            AudioTrack previousTrack = GetPreviousTrack();
                            if (previousTrack != null)
                            {
                                player.Track = previousTrack;
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                ErrorLogger.LogException("OnPlayStateChanged", ex);
                player.Track = null;
            }
            finally
            {
                // force it to stop and not get in a bad way
                NotifyComplete();
            }


        }

        private bool IsNearStart(BackgroundAudioPlayer player, AudioTrack track)
        {

            try
            {
                var currentSeconds = player.Position.TotalSeconds;
                return (currentSeconds < 5);
            }
            catch (Exception)
            {
            }

            return false;

        }

        /// <summary>
        /// Implements the logic to get the next AudioTrack instance.
        /// In a playlist, the source can be from a file, a web request, etc.
        /// </summary>
        /// <remarks>
        /// The AudioTrack URI determines the source, which can be:
        /// (a) Isolated-storage file (Relative URI, represents path in the isolated storage)
        /// (b) HTTP URL (absolute URI)
        /// (c) MediaStreamSource (null)
        /// </remarks>
        /// <returns>an instance of AudioTrack, or null if the playback is completed</returns>
        private AudioTrack GetNextTrack()
        {

            // Load the playlists
            var dsh = new StorageHelper();
            var currentPlaylist = dsh.LoadContentsFromFile<SoundcloudPlaylist>(ApplicationConstants.SoundcloudPlaylist);

            SoundcloudTrack track = null;
            string streamingUrl = string.Empty;

            while (string.IsNullOrEmpty(streamingUrl))
            {
                currentPlaylist.CurrentPosition += 1;
                if (currentPlaylist.CurrentPosition > currentPlaylist.Tracks.Count - 1)
                    currentPlaylist.CurrentPosition = 0;

                track = currentPlaylist.Tracks[currentPlaylist.CurrentPosition];
                streamingUrl = track.StreamingUrl;
            }

            if (track == null)
                return null;

            dsh.SaveContentsToFile(ApplicationConstants.SoundcloudPlaylist, currentPlaylist);

            return FormatTrack(currentPlaylist, track);

        }


        /// <summary>
        /// Implements the logic to get the previous AudioTrack instance.
        /// </summary>
        /// <remarks>
        /// The AudioTrack URI determines the source, which can be:
        /// (a) Isolated-storage file (Relative URI, represents path in the isolated storage)
        /// (b) HTTP URL (absolute URI)
        /// (c) MediaStreamSource (null)
        /// </remarks>
        /// <returns>an instance of AudioTrack, or null if previous track is not allowed</returns>
        private AudioTrack GetPreviousTrack()
        {
            // Load the playlists
            var dsh = new StorageHelper();
            var currentPlaylist = dsh.LoadContentsFromFile<SoundcloudPlaylist>(ApplicationConstants.SoundcloudPlaylist);

            SoundcloudTrack track = null;
            string streamingUrl = string.Empty;

            while (string.IsNullOrEmpty(streamingUrl))
            {
                currentPlaylist.CurrentPosition -= 1;
                if (currentPlaylist.CurrentPosition < 0)
                    currentPlaylist.CurrentPosition = currentPlaylist.Tracks.Count - 1;

                track = currentPlaylist.Tracks[currentPlaylist.CurrentPosition];
                streamingUrl = track.StreamingUrl;
            }

            if (track == null)
                return null;

            dsh.SaveContentsToFile(ApplicationConstants.SoundcloudPlaylist, currentPlaylist);

            return FormatTrack(currentPlaylist, track);

        }

        private Uri GetRemoteOrCachedUrl(long trackId, string streamingUrl)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(ApplicationConstants.DownloadFolder + "/" + trackId))
                {
                    return new Uri(ApplicationConstants.DownloadFolderNoSlash + "/" + trackId, UriKind.Relative);
                }
            }
            return new Uri(streamingUrl, UriKind.Absolute);
        }


        private AudioTrack FormatTrack(SoundcloudPlaylist currentPlaylist, SoundcloudTrack track)
        {

            var dsh = new StorageHelper();

            var streamingUri = GetRemoteOrCachedUrl(track.Id, track.StreamingUrl);
            var albumArtUri = string.IsNullOrEmpty(track.AlbumArt) ? null : new Uri(track.AlbumArt, UriKind.RelativeOrAbsolute);

            var nextTrack = new AudioTrack(streamingUri, track.Title, track.Artist, null, albumArtUri, track.PlayerTag, EnabledPlayerControls.All);

            try
            {
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (storage.FileExists(track.AlbumArt))
                    {
                        using (var newFile = storage.OpenFile(track.AlbumArt, FileMode.Open))
                        {
                            using (var reader = new StreamReader(newFile))
                            {
                                var stream = reader.BaseStream;

                                var mediaHistoryItem = new MediaHistoryItem
                                {
                                    ImageStream = stream,
                                    Source = "",
                                    Title = track.Title
                                };

                                mediaHistoryItem.PlayerContext.Add("CloudohKey", track.Id.ToString(CultureInfo.InvariantCulture));
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
                                    recentItem.PlayerContext.Add("CloudohKey", track.Id.ToString(CultureInfo.InvariantCulture)); // put an ID of the recent item to add                 
                                    MediaHistory.Instance.WriteRecentPlay(recentItem);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {
                // ignore, not critical
            }

            // serialise and save the playlist            
            dsh.SaveContentsToFile(ApplicationConstants.SoundcloudPlaylist, currentPlaylist);

            // specify the track
            return nextTrack;

        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent 
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {

            Debug.WriteLine("AudioPlayer.OnError:" + error);
            ErrorLogger.LogException("AudioPlayer.OnError", error);

            if (isFatal)
            {
                Abort();
            }
            else
            {
                NotifyComplete();
            }

        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        /// <remarks>
        /// Once the request is Cancelled, the agent gets 5 seconds to finish its work,
        /// by calling NotifyComplete()/Abort().
        /// </remarks>
        protected override void OnCancel()
        {
            Debug.WriteLine("AudioPlayer.OnCancel");

        }

    }
}
