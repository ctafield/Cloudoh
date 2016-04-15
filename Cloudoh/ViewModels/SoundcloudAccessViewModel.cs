using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cloudoh.Annotations;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using ProtoBuf;

namespace Cloudoh.ViewModels
{

    [ProtoContract]
    public class SoundcloudAccessViewModel : INotifyPropertyChanged
    {

        private string _accessToken;
        private string _fullname;
        private string _profileUrl;
        private string _userName;
        private string _id;

        public SoundcloudAccessViewModel()
        {
            
        }

        public SoundcloudAccessViewModel(SoundcloudAccess model)
        {
            AccessToken = model.AccessToken;
            Fullname = model.Fullname;
            Id = model.Id;
            ProfileUrl = model.ProfileUrl;
            UserName = model.UserName;
        }


        [ProtoMember(1)]
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == _userName) return;
                _userName = value;
                OnPropertyChanged();
            }
        }

        [ProtoMember(2)]
        public string ProfileUrl
        {
            get { return _profileUrl; }
            set
            {
                if (value == _profileUrl) return;
                _profileUrl = value;
                OnPropertyChanged();
            }
        }

        [ProtoMember(3)]
        public string Id
        {
            get { return _id; }
            set
            {
                if (value == _id) return;
                _id = value;
                OnPropertyChanged();
                OnPropertyChanged("CachedImageUri");
            }
        }

        [ProtoMember(4)]
        public string Fullname
        {
            get { return _fullname; }
            set
            {
                if (value == _fullname) return;
                _fullname = value;
                OnPropertyChanged();
            }
        }

        [ProtoMember(5)]
        public string AccessToken
        {
            get { return _accessToken; }
            set
            {
                if (value == _accessToken) return;
                _accessToken = value;
                OnPropertyChanged();
            }
        }

        public string CachedImageUri
        {
            get
            {
                var sh = new StorageHelper();
                return sh.CachedImageUri(Id);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
