using System.Windows;
using FieldOfTweets.Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace Cloudoh
{
    public partial class About : PhoneApplicationPage
    {

        public About()
        {
            InitializeComponent();
            txtVersion.Text = VersionInfo.FullVersion();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var emailTask = new EmailComposeTask
            {
                To = "cloudoh@myownltd.co.uk",
                Subject = "Cloudoh"
            };
            emailTask.Show();
        }


        private void rateApp(object sender, RoutedEventArgs e)
        {
            var a = new MarketplaceReviewTask();
            a.Show();            
        }

    }

}
