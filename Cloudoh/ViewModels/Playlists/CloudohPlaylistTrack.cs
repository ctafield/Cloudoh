using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Cloudoh.Classes;
using Cloudoh.Common.Annotations;
using Newtonsoft.Json;

namespace Cloudoh.ViewModels.Playlists
{

    [Obsolete]
    public class CloudohPlaylistTrack : INotifyPropertyChanged
    {
        public string SoundcloudUrl { get; set; }

        public string TrackName { get; set; }

        public long TrackId { get; set; }

        public string ArtistName { get; set; }

        public long Duration { get; set; }

        private Uri _albumArtImageSource;

        [JsonIgnore]
        public Uri AlbumArtImageSource
        {
            get
            {
                if (_albumArtImageSource != null)
                    return _albumArtImageSource;

                if (string.IsNullOrWhiteSpace(AlbumArtLarge))
                    return null;

                ThreadPool.QueueUserWorkItem(delegate(object state)
                {
                    var model = state as CloudohPlaylistTrack;

                    var urlParts = model.AlbumArtLarge.Split('/');
                    var currentImage = urlParts[urlParts.Length - 1];

                    if (currentImage.Contains("?"))
                        currentImage = currentImage.Substring(0, currentImage.IndexOf('?'));

                    var userId = urlParts[urlParts.Length - 2];

                    if (ImageCacheHelper.IsProfileImageCached(userId, currentImage))
                    {
                        model._albumArtImageSource = ImageCacheHelper.GetUriForCachedImage(userId, currentImage);
                        OnPropertyChanged("AlbumArtImageSource");
                        OnPropertyChanged("AlbumArtLarge");
                    }
                    else
                    {
                        ImageCacheHelper.CacheImage(model.AlbumArtLarge, userId, currentImage, () =>
                        {
                            OnPropertyChanged("AlbumArtImageSource");
                            OnPropertyChanged("AlbumArtLarge");
                        });
                    }
                }, this);

                return null;
            }
        }

        private string _albumArt;
        public string AlbumArt
        {
            get
            {
                return _albumArt;
            }
            set
            {
                _albumArt = value;
            }
        }

        [JsonIgnore]
        public string AlbumArtLarge
        {
            get
            {
                if (_albumArt.Contains("-large"))
                    return _albumArt.Replace("-large", "-t300x300");
                return _albumArt;
            }
        }


        [NotNull]
        public CloudohPlaylistTrack Clone()
        {
            return (CloudohPlaylistTrack) this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}