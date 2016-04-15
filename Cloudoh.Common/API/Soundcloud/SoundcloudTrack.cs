using System;
using Newtonsoft.Json;
using ProtoBuf;

namespace Cloudoh.Common.API.Soundcloud
{
    [ProtoContract]
    public class SoundcloudTrack
    {
        [ProtoMember(1)]
        public string Title { get; set; }
        [ProtoMember(2)]
        public string StreamingUrl { get; set; }
        [ProtoMember(3)]
        public string Artist { get; set; }
        [ProtoMember(4)]
        public int Index { get; set; }
        [ProtoMember(5)]
        public string AlbumArt { get; set; }
        [ProtoMember(6)]
        public string AlbumArtRemote { get; set; }
        [ProtoMember(7)]
        public long Id { get; set; }

        public string PlayerTag
        {
            get
            {
                try
                {
                    var tag = new SoundcloudNowPlayingDetails()
                    {
                        Marker = "CLOUDOHSC",
                        Id = Id,
                        AlbumArtUri = AlbumArt,
                        AlbumArtRemote = AlbumArtRemote
                    };

                    return JsonConvert.SerializeObject(tag);
                }
                catch (Exception)
                {
                    return null;
                }                
            }
        }
    }
}