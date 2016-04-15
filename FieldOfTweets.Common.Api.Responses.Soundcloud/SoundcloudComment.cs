namespace FieldOfTweets.Common.Api.Responses.Soundcloud
{
    public class SoundcloudComment
    {
        public string kind { get; set; }
        public int id { get; set; }
        public string created_at { get; set; }
        public int user_id { get; set; }
        public int track_id { get; set; }
        public int? timestamp { get; set; }
        public string body { get; set; }
        public string uri { get; set; }
        public SoundcloudUser user { get; set; }
    }
}
