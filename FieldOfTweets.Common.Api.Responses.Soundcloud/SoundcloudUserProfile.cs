namespace FieldOfTweets.Common.Api.Responses.Soundcloud
{
    public class SoundcloudUserProfile
    {

        /*
         *  id	integer ID	123
            permalink	permalink of the resource	"sbahn-sounds"
            username	username	"Doctor Wilson"
            uri	API resource URL	http://api.soundcloud.com/comments/32562
            permalink_url	URL to the SoundCloud.com page	"http://soundcloud.com/bryan/sbahn-sounds"
            avatar_url	URL to a JPEG image	"http://i1.sndcdn.com/avatars-000011353294-n0axp1-large.jpg"
            country	country	"Germany"
            full_name	first and last name	"Tom Wilson"
            city	city	"Berlin"
            description	description	"Buskers playing in the S-Bahn station in Berlin"
            discogs-name	Discogs name	"myrandomband"
            myspace-name	MySpace name	"myrandomband"
            website	a URL to the website	"http://facebook.com/myrandomband"
            website-title	a custom title for the website	"myrandomband on Facebook"
            online	online status (boolean)	true
            track_count	number of public tracks	4
            playlist_count	number of public playlists	5
            followers_count	number of followers	54
            followings_count	number of followed users	75
            public_favorites_count	number of favorited public tracks	7
            avatar_data	binary data of user avatar	(only for uploading)
         */

        public long id { get; set; }
        public string kind { get; set; }
        public string permalink { get; set; }
        public string username { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
        public string avatar_url { get; set; }
        public string country { get; set; }
        public string full_name { get; set; }
        public string description { get; set; }
        public string city { get; set; }
        public string discogs_name { get; set; }
        public string myspace_name { get; set; }
        public string website { get; set; }
        public string website_title { get; set; }
        public bool? online { get; set; }
        public int? track_count { get; set; }
        public int? playlist_count { get; set; }
        public string plan { get; set; }
        public int? public_favorites_count { get; set; }
        public int? followers_count { get; set; }
        public int? followings_count { get; set; }
        public int? upload_seconds_left { get; set; }
        public SoundcloudQuota SoundcloudQuota { get; set; }
        public int? private_tracks_count { get; set; }
        public int? private_playlists_count { get; set; }
        public bool? primary_email_confirmed { get; set; }
    }

}

