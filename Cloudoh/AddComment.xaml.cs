using System;
using System.Windows;
using System.Windows.Navigation;
using Cloudoh.Classes;
using Cloudoh.Common.API.Soundcloud;
using Microsoft.Phone.Controls;

namespace Cloudoh
{
    public partial class AddComment : PhoneApplicationPage
    {

        private int TrackId { get; set; }

        public AddComment()
        {
            InitializeComponent();

            Loaded += AddComment_Loaded;
        }

        void AddComment_Loaded(object sender, RoutedEventArgs e)
        {
            txtComment.Focus();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.ContainsKey("trackId"))
            {
                int trackId;
                if (!int.TryParse(NavigationContext.QueryString["trackId"], out trackId)) 
                    NavigationService.GoBack();
                TrackId = trackId;
            }
            else
            {
                NavigationService.GoBack();
            }

            if (NavigationContext.QueryString.ContainsKey("maxTime"))
            {
                double maxTime;
                if (double.TryParse(NavigationContext.QueryString["maxTime"], out maxTime))
                {
                    timePicker.MaxValue = TimeSpan.FromSeconds((int)maxTime);
                }
                else
                {
                    NavigationService.GoBack();
                }
            }

            if (NavigationContext.QueryString.ContainsKey("time"))
            {
                double time;
                if (double.TryParse(NavigationContext.QueryString["time"], out time))
                {
                    timePicker.Value = TimeSpan.FromSeconds((int)time);
                }                
            }

        }


        private void mnuComment_Click(object sender, EventArgs e)
        {
            if (!timePicker.Value.HasValue)
                return;

            var comment = txtComment.Text;

            if (string.IsNullOrWhiteSpace(comment.Trim()))
                return;

            var api = new SoundcloudApi();
            var timestamp = (int)timePicker.Value.Value.TotalMilliseconds;

            UiHelper.ShowProgressBar("posting comment");
            api.PostCommentCompletedEvent += ApiOnPostCommentCompletedEvent;
            api.PostComment(TrackId, comment, timestamp);
        }

        private void ApiOnPostCommentCompletedEvent(object sender, EventArgs eventArgs)
        { 

            UiHelper.HideProgressBar();

            UiHelper.SafeDispatch(() => {
                UiHelper.ShowToastDelayed("comment posted!");

                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            });

        }

    }

}