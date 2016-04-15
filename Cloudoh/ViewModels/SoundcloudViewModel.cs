using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Windows;
using Cloudoh.Classes;
using Cloudoh.Common;
using Newtonsoft.Json;
using ProtoBuf;

namespace Cloudoh.ViewModels
{

    [ProtoContract]
    public class SoundcloudViewModel : INotifyPropertyChanged, IComparable<SoundcloudViewModel>
    {

        [ProtoMember(1)]
        public ApplicationConstants.SoundcloudTypeEnum StreamType { get; set; }

        [ProtoMember(2)]
        public ObservableCollection<SoundcloudViewModel> PlaylistTracks { get; set; }

        [ProtoMember(3)]
        public long UserId { get; set; }
        
        [ProtoMember(4)]
        public string UserName { get; set; }

        [ProtoMember(5)]
        public string Title { get; set; }

        [ProtoMember(6)]
        public string StreamingUrl { get; set; }

        [ProtoMember(7)]
        public long Id { get; set; }


        [JsonIgnore]
        public string DescriptionDecoded
        {
            get { return HttpUtility.HtmlDecode(Description); }
        }

        [ProtoMember(8)]
        public string Description
        {
            get
            {                
                return _description;
            }
            set
            {
                if (value != _description)
                {
                    _description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        [JsonIgnore]
        public Visibility DescriptionVisibility
        {
            get
            {
                if (string.IsNullOrEmpty(Description)) return Visibility.Collapsed;
                return (string.IsNullOrEmpty(Description.Trim())) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        [ProtoMember(9)]
        public string WaveformUrl { get; set; }

        [ProtoMember(10)]
        public string Genre { get; set; }

        [ProtoMember(11)]
        public int Index { get; set; }

        [ProtoMember(12)]
        public string PermalinkUrl { get; set; }

        [ProtoMember(13)]
        public bool IsStreamable { get; set; }

        [ProtoMember(14)]
        public Uri AlbumArtImageCached { get; set; }

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
                    var model = state as SoundcloudViewModel;

                    var currentImage = GetFileNameFromFile(model.AlbumArtLarge);

                    //var userId = urlParts[urlParts.Length - 2];
                    const string userId = "thumb";

                    if (ImageCacheHelper.IsProfileImageCached(userId, currentImage))
                    {
                        model._albumArtImageSource = ImageCacheHelper.GetUriForCachedImage(userId, currentImage);
                        model.NotifyPropertyChanged("AlbumArtImageSource");
                        model.NotifyPropertyChanged("AlbumArtLarge");
                    }
                    else
                    {
                        ImageCacheHelper.CacheImage(model.AlbumArtLarge, userId, currentImage, () =>
                                                                                              {
                                                                                                  model.NotifyPropertyChanged("AlbumArtImageSource");
                                                                                                  model.NotifyPropertyChanged("AlbumArtLarge");
                                                                                              });
                    }
                }, this);

                return null;
            }
        }

        private string GetFileNameFromFile(string fileName)
        {
            var urlParts = fileName.Split('/');
            var currentImage = urlParts[urlParts.Length - 1];

            if (currentImage.Contains("?"))
                currentImage = currentImage.Substring(0, currentImage.IndexOf('?'));

            return currentImage;
        }

        [ProtoMember(15)]
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

        [JsonIgnore]
        public string AlbumArtOriginal
        {
            get
            {
                if (_albumArt.Contains("-large"))
                    return _albumArt.Replace("-large", "-t500x500");
                return _albumArt;
            }
        }

        private string _avatarUrl;
        private string _description;

        [ProtoMember(16)]
        public string AvatarUrl
        {
            get
            {
                return _avatarUrl;
            }
            set
            {
                _avatarUrl = value;
            }
        }

        [ProtoMember(17)]
        public string Type { get; set; }

        [ProtoMember(18)]
        public long Duration { get; set; }

        [JsonIgnore]
        public TimeSpan DurationTimeSpan
        {
            get { return TimeSpan.FromMilliseconds(Duration); }
        }

        [JsonIgnore]
        public string DurationFormatted
        {
            get
            {
                var duration = DurationTimeSpan;
                return new DateTime(duration.Ticks).ToString(duration.Hours > 0 ? "HH:mm.ss" : "mm.ss");
            }
        }

        [JsonIgnore]
        public string PlayerTag
        {
            get
            {
                var tag = new SoundcloudNowPlayingDetails()
                {
                    Marker = "CLOUDOHSC",
                    Id = Id,
                    AlbumArtUri = AlbumArtOriginal,
                    AlbumArtRemote = _albumArt
                };

                return JsonConvert.SerializeObject(tag);
            }
        }

        [ProtoMember(19)]
        public int? PlayCount { get; set; }

        [ProtoMember(20)]
        public int? LikeCount { get; set; }

        [ProtoMember(21)]
        public int? CommentCount { get; set; }

        [ProtoMember(22)]
        public int? DownloadCount { get; set; }

        [JsonIgnore]
        public string Age
        {
            get
            {
                var diff = DateTime.Now.Subtract(TrackCreated);
                int roundedDays = (int)Math.Floor(diff.TotalDays);

                if (roundedDays == 0)
                {
                    int hours = (int)Math.Floor(diff.TotalHours);
                    return hours + " hour" + ((hours != 1) ? "s" : "");
                }
                else
                    return roundedDays + " day" + ((roundedDays != 1) ? "s" : "");
            }
        }

        [ProtoMember(23)]
        public string TrackCreatedAt { get; set; }

        [JsonIgnore]
        public DateTime TrackCreated
        {
            get
            {
                return DateTime.Parse(TrackCreatedAt);
            }
        }

        [JsonIgnore]
        public Visibility PlayCountVisibility
        {
            get { return (PlayCount.HasValue) ? Visibility.Visible : Visibility.Collapsed; }
        }

        [JsonIgnore]
        public Visibility TrackCountVisiblity
        {
            get { return (TrackCount.HasValue) ? Visibility.Visible : Visibility.Collapsed; }
        }

        [ProtoMember(24)]
        public int? TrackCount { get; set; }

        [JsonIgnore]
        public string TrackCountFormatted
        {
            get
            {
                if (!TrackCount.HasValue)
                    return string.Empty;
                return TrackCount.Value.ToString(CultureInfo.InvariantCulture) + " track" + ((TrackCount.Value == 1) ? "" : "s");
            }
        }

        [ProtoMember(25)]
        public string PlaylistUri { get; set; }

        /// <summary>
        /// Is this track downloadable?
        /// </summary>
        [ProtoMember(26)]
        public bool Downloadable
        {
            get; 
            set;
        }

        /// <summary>
        /// Url to download from
        /// </summary>
        [ProtoMember(27)]
        public string DownloadUrl
        {
            get; 
            set;
        }

        [JsonIgnore]
        public string ThumbnailImage {
            get
            {
                return @"D\thumb\" + GetFileNameFromFile(AlbumArtLarge).Replace(".jpg", "_thumb.jpg");

                //if (_albumArtImageSource != null)
                //{
                //    return _albumArtImageSource.OriginalString.Replace(".jpg", "_thumb.jpg");
                //}
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            UiHelper.SafeDispatch(() =>
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        public SoundcloudViewModel Clone()
        {
            return this.MemberwiseClone() as SoundcloudViewModel;
        }

        public int CompareTo(SoundcloudViewModel other)
        {

            //if (StreamType == ApplicationConstants.SoundcloudTypeEnum.Favourites)
            //    return other.Id.CompareTo(Id);

            return other.TrackCreated.CompareTo(TrackCreated);
        }
        
        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

}
