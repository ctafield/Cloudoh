using System;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.Common.ErrorLogging;
using Cloudoh.ViewModels;
using FieldOfTweets.Common.Api.Responses.Soundcloud;
using Newtonsoft.Json;

namespace Cloudoh.ExtensionMethods
{
    public static class SoundcloudViewModelExtension
    {

        public static SoundcloudViewModel AsViewModelFromPlaylist(this Collection track, int index)
        {
            var api = new SoundcloudApi();

            return new SoundcloudViewModel
            {
                Type = track.type,
                StreamType = ApplicationConstants.SoundcloudTypeEnum.Playlist,
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
                PlaylistUri = track.origin.tracks_uri,
                TrackCount = track.origin.track_count,
                Index = index
            };
        }

        public static SoundcloudUserViewModel AsViewModel(this SoundcloudUserProfile userProfile)
        {
            var model = new SoundcloudUserViewModel()
            {
                Biography = userProfile.description,
                Followers = userProfile.followers_count,
                Following = userProfile.followings_count,
                FullName = userProfile.full_name,
                ProfileImageUrl = userProfile.avatar_url,
                ScreenName = userProfile.username,
                WebSite = userProfile.website,
                Id = userProfile.id,
                City = userProfile.city,
                Country = userProfile.country
            };

            return model;
        }

        public static SoundcloudViewModel AsViewModel(this ResponseGetTrack track, int index, ApplicationConstants.SoundcloudTypeEnum streamType)
        {

            if (track == null)
                return null;
           
            try
            {

                var api = new SoundcloudApi(); 

                var model = new SoundcloudViewModel
                {
                    StreamType = streamType,
                    AlbumArt = string.IsNullOrEmpty(track.artwork_url) ? track.user.avatar_url : track.artwork_url,
                    Id = track.id,
                    StreamingUrl = api.GetTrackUrl(track.stream_url),
                    IsStreamable = track.streamable.GetValueOrDefault(false),
                    Title = track.title,
                    UserName = track.user.username,
                    UserId = (track.user != null) ? track.user.id : 0,
                    AvatarUrl = (track.user != null) ? track.user.avatar_url : null,
                    Description = track.description,
                    Genre = track.genre,
                    WaveformUrl = track.waveform_url,
                    PermalinkUrl = track.permalink_url,
                    Duration = track.duration,
                    Index = index,
                    LikeCount = track.favoritings_count,
                    PlayCount = track.playback_count,
                    CommentCount = track.comment_count,
                    DownloadCount = track.download_count,
                    TrackCount = track.track_count,
                    Downloadable = track.downloadable.GetValueOrDefault(false),
                    TrackCreatedAt = track.created_at,
                    DownloadUrl = track.download_url,
                };

                return model;

            }
            catch (Exception ex)
            {
                string trackInfo = "";

                try
                {
                    trackInfo = JsonConvert.SerializeObject(track);
                }
                catch (Exception)
                {
                }

                ErrorLogger.LogException("Extensions.AsViewModel", ex, trackInfo);
                CrittercismSDK.Crittercism.LeaveBreadcrumb("TrackInfo = " + trackInfo);
                CrittercismSDK.Crittercism.LogHandledException(ex);
                return null;
            }

        }

    }
}
