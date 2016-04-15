namespace Cloudoh.Common
{
    public static class ApplicationConstants
    {

        //public const string CloudohAppStoreLink = "http://www.windowsphone.com/s?appid=d8ead07d-327c-46fb-8327-61e46eb9cccd";
        public const string CloudohAppStoreLink = "http://www.windowsphone.com/";

        public enum SoundcloudTypeEnum
        {
            Dashboard = 0,
            Favourites = 1,
            SearchResultsHot = 2,
            SearchResultsNew = 3,
            Playlist = 4,
            PlaylistTrack = 5,
            CustomPlaylistTrack = 6,
            DownloadPlaylist = 7,
            UsersTracks = 8,
            UsersLikes = 9,
            SoundcloudPlaylistTrack = 10
        }

        public static string UserCacheStorageFolder
        {
            get
            {
                // was C, now D as we store thumbnails too
                return "D"; // had to shorten this as it was too long for wp7 file system
            }
        }

        public static string UserStorageFolder
        {
            get { return "Users"; }
        }

        public static string ShellMediaFolder
        {
            get { return "Shared/Media"; }
        }

        public static string ShellContentFolder
        {
            get { return "Shared/ShellContent"; }
        }

        public static string SoundcloudPlaylist
        {
            get { return "Soundcloud_Playlist.json"; }
        }

        public static string SoundcloudLastFavourites
        {
            get { return "Last_Favourites.json"; }
        }

        public static string SoundcloudLastStream
        {
            get { return "Last_Stream.json"; }
        }

        public static string SoundcloudRecentSearches
        {
            get { return "RecentSearches.json"; }
        }

        public static string SoundcloudRecentPlays
        {
            get { return "RecentPlays.json"; }
        }

        /// <summary>
        /// This is the users custom playlists
        /// </summary>
        public static string SoundcloudPlaylists
        {
            get { return "CustomPlaylists.json"; }
        }

        /// <summary>
        /// this is the IAP key
        /// </summary>
        public static string CloudohPremiumLicence
        {
            get { return "premium"; }
        }

        public static string CloudohSettingsFile
        {
            get
            {
                return "Settings.json";
            }
        }

        public static string SearchSettingsFile
        {
            get
            {
                return "SearchSettings.json";
            }
        }

        /// <summary>
        /// Folder for cached track downloads
        /// </summary>
        public static string DownloadFolder
        {
            get
            {
                return "/shared/transfers";
            }
        }

        public static string DownloadFolderNoSlash
        {
            get
            {
                return "shared/transfers";
            }
        }

    }

}
