using System;
using Cloudoh.Common;
using ProtoBuf;

namespace Cloudoh.Classes
{
    
    [ProtoContract]
    public class SearchSettings
    {
        [ProtoMember(1)]
        public bool? DownloadableOnly { get; set; }

        [ProtoMember(2)]
        public bool? FilterOnBpm { get; set; }

        [ProtoMember(3)]
        public bool? FilterOnDuration { get; set; }

        [ProtoMember(4)]
        public short? BpmMinimum { get; set; }

        [ProtoMember(5)]
        public short? BpmMaximum { get; set; }

        [ProtoMember(6)]
        public TimeSpan? MinDuration { get; set; }

        [ProtoMember(7)]
        public TimeSpan? MaxDuration { get; set; }

        public bool IsFiltered
        {
            get
            {
                return (DownloadableOnly.GetValueOrDefault() ||
                        FilterOnBpm.GetValueOrDefault() ||
                        FilterOnDuration.GetValueOrDefault());
            }
        }

        public void Save()
        {
            var storageHelper = new StorageHelper();
            storageHelper.SaveContentsToFile(ApplicationConstants.SearchSettingsFile, this);
        }
    }

}
