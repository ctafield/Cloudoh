using ProtoBuf;

namespace Cloudoh.Common
{

    public class SettingsHelper
    {
        public CloudohSettings GetSettings()
        {
            var storageHelper = new StorageHelper();
            var contents = storageHelper.LoadContentsFromFile<CloudohSettingsStorage>(ApplicationConstants.CloudohSettingsFile);

            if (contents != null)
            {
                var settings = new CloudohSettings(contents);
                return settings;
            }

            var cloudohSettingsStorage = new CloudohSettingsStorage();            
            return new CloudohSettings(cloudohSettingsStorage);
            
        }

        public void SaveSettings(CloudohSettings settings)
        {
            var storageHelper = new StorageHelper();
            storageHelper.SaveContentsToFile(ApplicationConstants.CloudohSettingsFile, settings.CloudohSettingsStorage);
        }
    }

    public class CloudohSettings
    {
        private CloudohSettingsStorage _cloudohSettingsStorage;

        public CloudohSettingsStorage CloudohSettingsStorage
        {
            get { return _cloudohSettingsStorage; }
        }

        public bool ShowAlbumArtOnTile
        {
            get
            {
                if (!_cloudohSettingsStorage.ShowAlbumArtOnTile.HasValue)
                    return true;

                return _cloudohSettingsStorage.ShowAlbumArtOnTile.Value;
            }
            set
            {
                _cloudohSettingsStorage.ShowAlbumArtOnTile = value;
            }
        }

        public bool IncludeInMusicHub
        {
            get
            {
                if (!_cloudohSettingsStorage.IncludeInMusicHub.HasValue)
                    return true;

                return _cloudohSettingsStorage.IncludeInMusicHub.Value;
            }
            set
            {
                _cloudohSettingsStorage.IncludeInMusicHub = value;
            }
        }

        public bool TransparentSmallTile
        {
            get
            {
                if (! _cloudohSettingsStorage.TransparentSmallTile.HasValue)
                    return false;

                return _cloudohSettingsStorage.TransparentSmallTile.Value;                
            }
            set
            {
                _cloudohSettingsStorage.TransparentSmallTile = value;
            }
        }

        public CloudohSettings(CloudohSettingsStorage settings)
        {
            _cloudohSettingsStorage = settings;
        }

    }

    [ProtoContract]
    public class CloudohSettingsStorage
    {
        [ProtoMember(1)]
        public bool? ShowAlbumArtOnTile { get; set; }

        [ProtoMember(2)]
        public bool? IncludeInMusicHub { get; set; }

        [ProtoMember(3)]
        public bool? TransparentSmallTile { get; set; }    
    }

}
