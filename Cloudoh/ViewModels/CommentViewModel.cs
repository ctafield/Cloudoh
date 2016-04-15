using System;
using Telerik.Windows.Controls.ToggleSwitch;

namespace Cloudoh.ViewModels
{
    public class CommentViewModel
    {
        public string AvatarUrl { get; set; }

        public string Comment { get; set; }
        public int? TimeStamp { get; set; }

        public string TimeMark
        {
            get
            {
                if (TimeStamp == null)
                    return string.Empty;
                if (TimeStamp.GetValueOrDefault() < 0)
                    TimeStamp = 0;
                var duration = TimeSpan.FromMilliseconds(TimeStamp.Value);
                return new DateTime(duration.Ticks).ToString(duration.Hours > 0 ? "HH:mm.ss" : "mm.ss");
            }
        }
    }
}