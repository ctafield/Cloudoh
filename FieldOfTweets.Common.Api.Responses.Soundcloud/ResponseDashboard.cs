using System.Collections.Generic;

namespace FieldOfTweets.Common.Api.Responses.Soundcloud
{
    public class SharingNote
    {
        public string text { get; set; }
        public string created_at { get; set; }
    }

    public class Track
    {
        public int id { get; set; }
        public string permalink { get; set; }
        public string title { get; set; }
        public int user_id { get; set; }
        public string kind { get; set; }
        public string uri { get; set; }
        public string user_uri { get; set; }
        public string permalink_url { get; set; }
        public string stream_url { get; set; }
        public string artwork_url { get; set; }
    }

    public class Label
    {
        public int id { get; set; }
        public string kind { get; set; }
        public string permalink { get; set; }
        public string username { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
        public string avatar_url { get; set; }
    }

    public class Origin
    {
        public string tracks_uri;
        public int? track_count;
        public string kind { get; set; }
        public int id { get; set; }
        public string created_at { get; set; }
        public int user_id { get; set; }
        public long duration { get; set; }
        public bool commentable { get; set; }
        public string state { get; set; }
        public int original_content_size { get; set; }
        public string sharing { get; set; }
        public string tag_list { get; set; }
        public string permalink { get; set; }
        public string description { get; set; }
        public bool? streamable { get; set; }
        public bool? downloadable { get; set; }
        public string genre { get; set; }
        public string release { get; set; }
        public string purchase_url { get; set; }
        public string purchase_title { get; set; }
        public int? label_id { get; set; }
        public string label_name { get; set; }
        public string isrc { get; set; }
        public object video_url { get; set; }
        public string track_type { get; set; }
        public string key_signature { get; set; }
        public double? bpm { get; set; }
        public string title { get; set; }
        public int? release_year { get; set; }
        public int? release_month { get; set; }
        public int? release_day { get; set; }
        public string original_format { get; set; }
        public string license { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
        public string artwork_url { get; set; }
        public string waveform_url { get; set; }
        public SoundcloudUser user { get; set; }
        public string stream_url { get; set; }
        public string download_url { get; set; }
        public int user_playback_count { get; set; }
        public bool user_favorite { get; set; }
        public int? playback_count { get; set; }
        public int? download_count { get; set; }
        public int? favoritings_count { get; set; }
        public int? comment_count { get; set; }
        public string attachments_uri { get; set; }
        public SharingNote sharing_note { get; set; }
        public Track track { get; set; }
        public Label label { get; set; }
    }

    public class Collection
    {
        public string type { get; set; }
        public string created_at { get; set; }
        public Origin origin { get; set; }
        public string tags { get; set; }
    }

    public class ResponseDashboard
    {
        public List<Collection> collection { get; set; }
        public string next_href { get; set; }
        public string future_href { get; set; }
    }

    public class ResponsePlaylist
    {
        public string kind { get; set; }
        public int id { get; set; }
        public string created_at { get; set; }
        public int user_id { get; set; }
        public int? duration { get; set; }
        public string sharing { get; set; }
        public string tag_list { get; set; }
        public string permalink { get; set; }
        public int? track_count { get; set; }
        public bool streamable { get; set; }
        public bool downloadable { get; set; }
        public string embeddable_by { get; set; }
        public string purchase_url { get; set; }
        public object label_id { get; set; }
        public string type { get; set; }
        public string playlist_type { get; set; }
        public string ean { get; set; }
        public string description { get; set; }
        public string genre { get; set; }
        public string release { get; set; }
        public string purchase_title { get; set; }
        public string label_name { get; set; }
        public string title { get; set; }
        public int? release_year { get; set; }
        public int? release_month { get; set; }
        public int? release_day { get; set; }
        public string license { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
        public string artwork_url { get; set; }
        public SoundcloudUser user { get; set; }
        public List<ResponseGetTrack> tracks { get; set; }
    }

}
