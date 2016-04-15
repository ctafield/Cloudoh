using ProtoBuf;

namespace Cloudoh.Common
{
    [ProtoContract]
    public class SoundcloudNowPlayingDetails
    {
        [ProtoMember(1)]
        public string AlbumArtUri { get; set; }
        [ProtoMember(2)]
        public string AlbumArtRemote { get; set; }
        [ProtoMember(3)]
        public string Marker { get; set; }
        [ProtoMember(4)]
        public long Id { get; set; }
        
    }
}
