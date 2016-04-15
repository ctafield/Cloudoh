using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Cloudoh.Classes;
using Cloudoh.Common.Annotations;
using Newtonsoft.Json;
using ProtoBuf;

namespace Cloudoh.ViewModels.Playlists
{

    [ProtoContract]
    public sealed class CloudohPlaylist : INotifyPropertyChanged, IComparable<CloudohPlaylist>
    {
        
        private string _title;
        private int _playCount;
        private CloudohPlaylistType _playlistType;
        private ObservableCollection<SoundcloudViewModel> _tracks;
        private SoundcloudViewModel _firstTrack;
        private string _permaLink;

        public CloudohPlaylist()
        {
            Tracks = new ObservableCollection<SoundcloudViewModel>();

            Tracks.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs args)
                                        {

                                            _firstTrack = null;
                                            OnPropertyChanged("FirstTrack");
                                            OnPropertyChanged("AlbumArtImageSource");
                                            OnPropertyChanged("TrackCount");
                                            OnPropertyChanged("TrackCountFormatted");
                                            OnPropertyChanged("DurationFormatted");
                                            OnPropertyChanged("AlbumArtImageSource");
                                        };
        }

        [ProtoMember(1)]
        public Guid Id { get; set; }

        [ProtoMember(2)]
        public CloudohPlaylistType PlaylistType
        {
            get { return _playlistType; }
            set
            {
                if (value == _playlistType) return;
                _playlistType = value;
                OnPropertyChanged();
            }
        }

        public void ResetFirstTrack()
        {
            _firstTrack = null;
            OnPropertyChanged("FirstTrack");
        }

        [JsonIgnore]
        public SoundcloudViewModel FirstTrack
        {
            get
            {
                if (_firstTrack != null)
                    return _firstTrack;

                _firstTrack = Tracks.FirstOrDefault();

                if (_firstTrack != null)
                    _firstTrack.PropertyChanged += (sender, args) => UiHelper.SafeDispatch(() =>
                                                                                           {
                                                                                               OnPropertyChanged(args.PropertyName);
                                                                                           });

                return _firstTrack;
            }
        }

        [JsonIgnore]
        public Uri AlbumArtImageSource
        {
            get
            {
                if (FirstTrack == null)
                    return null;
                return FirstTrack.AlbumArtImageSource;
            }
        }

        [ProtoMember(3)]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        [ProtoMember(4)]
        public int PlayCount
        {
            get { return _playCount; }
            set
            {
                if (value == _playCount) return;
                _playCount = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string DurationFormatted
        {
            get
            {
                var totalDuration = Tracks.Sum(x => x.Duration);
                var duration = TimeSpan.FromMilliseconds(totalDuration);
                return new DateTime(duration.Ticks).ToString(duration.Hours > 0 ? "HH:mm.ss" : "mm.ss");
            }
        }

        [JsonIgnore]
        public int TrackCount
        {
            get
            {
                return Tracks.Count;
            }
        }

        [JsonIgnore]
        public string TrackCountFormatted
        {
            get
            {
                return TrackCount.ToString(CultureInfo.InvariantCulture) + " track" + ((TrackCount == 1) ? "" : "s");
            }
        }

        [ProtoMember(5)]
        public ObservableCollection<SoundcloudViewModel> Tracks
        {
            get { return _tracks; }
            set
            {
                if (Equals(value, _tracks)) 
                    return;
                _tracks = value;
                OnPropertyChanged();
                OnPropertyChanged("TrackCount");

                _firstTrack = null;
                OnPropertyChanged("FirstTrack");
                OnPropertyChanged("AlbumArtImageSource");
            }
        }

        [ProtoMember(6)]
        public string PermaLink
        {
            get { return _permaLink ?? string.Empty; }
            set { _permaLink = value; }
        }

        public int CompareTo(CloudohPlaylist other)
        {
            return String.CompareOrdinal(other.Title, Title);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public CloudohPlaylist Clone()
        {
            var clone = (CloudohPlaylist)this.MemberwiseClone();
            clone.Tracks = new ObservableCollection<SoundcloudViewModel>();
            foreach (var track in this.Tracks)
                clone.Tracks.Add(track.Clone());
            return clone;
        }

    }


}

