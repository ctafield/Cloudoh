using ProtoBuf;

namespace Cloudoh
{
    [ProtoContract]
    public class DontAskAgain
    {
        [ProtoMember(1)]
        public bool DontAsk { get; set; }
    }
}