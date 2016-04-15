using System.Collections.Generic;
using ProtoBuf;

namespace Cloudoh.Common.API.Soundcloud
{
    [ProtoContract]
    public class SoundcloudPlaylist
    {
        [ProtoMember(1)]
        public List<SoundcloudTrack> Tracks { get; set; }
        [ProtoMember(2)]
        public int CurrentPosition { get; set; }
    }
}
