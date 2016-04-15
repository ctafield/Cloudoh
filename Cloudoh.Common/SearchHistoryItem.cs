using System.Windows.Media;
using Newtonsoft.Json;
using ProtoBuf;

namespace Cloudoh.Common
{

    [ProtoContract]
    public class SearchHistoryItem
    {

        [ProtoMember(1)]
        public string Query { get; set; }

        [ProtoMember(2)]
        public SearchTypeEnum SearchType { get; set; }

        [JsonIgnore]
        public Brush QueryColour
        {
            get
            {
                switch (SearchType)
                {
                    case SearchTypeEnum.Track:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 102, 0));
                    case SearchTypeEnum.Genre:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 39, 13));
                    case SearchTypeEnum.User:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 172, 13));
                }
                return null;
            }
        }
    }

    public enum SearchTypeEnum
    {
        Genre,
        Track,
        User
    }

}
