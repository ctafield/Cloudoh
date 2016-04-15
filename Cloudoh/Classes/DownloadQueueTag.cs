using System;
using Newtonsoft.Json;

namespace Cloudoh.Classes
{

    [Obsolete]
    public class DownloadQueueTag
    {
        public long Id { get; set; }
        public string AlbumArtUrl { get; set; }

        [JsonIgnore]
        public Uri AlbumArtUri
        {
            get
            {
                return new Uri(AlbumArtUrl, UriKind.Absolute);
            }
        }

        public string Title { get; set; }
        public string UserName { get; set; }
        public long Duration { get; set; }

        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}