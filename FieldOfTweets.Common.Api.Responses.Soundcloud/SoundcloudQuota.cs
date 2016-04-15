namespace FieldOfTweets.Common.Api.Responses.Soundcloud
{
    public class SoundcloudQuota
    {
        public bool unlimited_upload_quota { get; set; }
        public int upload_seconds_used { get; set; }
        public int upload_seconds_left { get; set; }
    }
}