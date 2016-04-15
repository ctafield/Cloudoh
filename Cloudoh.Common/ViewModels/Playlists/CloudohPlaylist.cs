using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cloudoh.Common.Annotations;
using Newtonsoft.Json;

namespace Cloudoh.Common.Playlists
{

    public sealed class CloudohPlaylist : INotifyPropertyChanged, IComparable<CloudohPlaylist>
    {

        private string _description;
        private CloudohPlaylistType _playlistType;
        private int _playCount;
        private ObservableCollection<CloudohPlaylistTrack> _tracks;

        public CloudohPlaylist()
        {
            Tracks = new ObservableCollection<CloudohPlaylistTrack>();
        }

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

        public string Description
        {
            get { return _description; }
            set
            {
                if (value == _description) return;
                _description = value;
                OnPropertyChanged();
            }
        }

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
        public int TrackCount
        {
            get
            {                
                return Tracks.Count;
            }
        }

        public ObservableCollection<CloudohPlaylistTrack> Tracks
        {
            get { return _tracks; }
            set
            {
                if (Equals(value, _tracks)) return;
                _tracks = value;
                OnPropertyChanged();
                OnPropertyChanged("TrackCount");
            }
        }

        public int CompareTo(CloudohPlaylist other)
        {
            return String.CompareOrdinal(other.Description, Description);
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

