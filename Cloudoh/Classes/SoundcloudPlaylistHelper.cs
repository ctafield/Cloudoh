using System;
using System.Collections.Generic;
using System.Linq;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.ViewModels;
using Cloudoh.ViewModels.Playlists;

namespace Cloudoh.Classes
{
    public class SoundcloudPlaylistHelper
    {

        public void SaveCurrentSoundcloudPlaylist(List<SoundcloudViewModel> model, int currentPosition)
        {

            var playlist = new SoundcloudPlaylist
            {
                Tracks = new List<SoundcloudTrack>(),
                CurrentPosition = currentPosition
            };

            var currentIndex = 0;

            foreach (var item in model)
            {
                var track = new SoundcloudTrack()
                {
                    Artist = item.UserName,
                    Index = currentIndex++,
                    StreamingUrl = item.StreamingUrl,
                    Title = item.Title,
                    AlbumArt = item.ThumbnailImage,
                    AlbumArtRemote = item.AlbumArtLarge,
                    Id = item.Id
                };
                playlist.Tracks.Add(track);
            }

            var dsh = new StorageHelper();

            // ensure the tile is there
            dsh.WriteShellTileImage();

            dsh.SaveContentsToFile(ApplicationConstants.SoundcloudPlaylist, playlist);
        }

        /// <summary>
        /// Saves this track to the recently played auto generated playlist.
        /// </summary>
        /// <param name="track"></param>
        public void SaveRecentPlaylist(SoundcloudViewModel track)
        {

            CloudohPlaylist recentPlayList;

            recentPlayList = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.PlaylistType == CloudohPlaylistType.Recent);

            if (recentPlayList == null)
            {
                recentPlayList = new CloudohPlaylist
                {
                    Title = "Recently Played",
                    Id = Guid.NewGuid(),
                    PlaylistType = CloudohPlaylistType.Recent
                };

                recentPlayList.Tracks.Add(track);

                App.ViewModel.CloudohPlaylists.Add(recentPlayList);

            }
            else
            {
                var existingItem = recentPlayList.Tracks.FirstOrDefault(x => x.Id == track.Id);
                if (existingItem != null)
                    recentPlayList.Tracks.Remove(existingItem);

                recentPlayList.Tracks.Insert(0, track);                
            }
           
            var tracks = recentPlayList.Tracks.Take(15).ToList();
            recentPlayList.Tracks.Clear();

            foreach (var item in tracks)
                recentPlayList.Tracks.Add(item);

            var sh = new StorageHelper();
            sh.SaveContentsToFile(ApplicationConstants.SoundcloudRecentPlays, recentPlayList);

        }

    }

}
