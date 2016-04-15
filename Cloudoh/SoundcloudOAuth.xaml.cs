using System;
using System.Globalization;
using System.Windows;
using System.Windows.Navigation;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using Microsoft.Phone.Controls;

namespace Cloudoh
{
    public partial class SoundcloudOAuth : PhoneApplicationPage
    {

        public SoundcloudOAuth()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavigateToSoundcloudOAuth();
        }

        private void NavigateToSoundcloudOAuth()
        {
            UiHelper.ShowProgressBar("loading soundcloud...");
            var api = new SoundcloudApi();
            var url = new Uri(api.LoginUrl, UriKind.Absolute);
            webBrowser.Navigated += delegate
            {
                UiHelper.HideProgressBar();
            };
            webBrowser.Navigate(url);
        }

        private static string GetQueryParameter(string input, string parameterName)
        {

            input = input.Substring(input.IndexOf("?", StringComparison.Ordinal) + 1);

            foreach (string item in input.Split('&'))
            {
                var parts = item.Split('=');
                if (parts[0] == parameterName)
                {
                    return parts[1];
                }
            }
            return String.Empty;
        }

        private bool GotCode { get; set; }

        protected void webBrowser_Navigating(object sender, NavigatingEventArgs e)
        {

            var api = new SoundcloudApi();

            if (e.Uri.ToString().ToLower().StartsWith(api.CallbackUri))
            {
                // ok, stop it.
                //e.Cancel = true;

                if (GotCode)
                    return;

                var code = GetQueryParameter(e.Uri.ToString().Replace("#", "&"), "code");

                if (string.IsNullOrEmpty(code))
                {
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                    return;
                }

                GotCode = true;

                api.GetAuthTokenCompletedEvent += api_GetAuthTokenCompletedEvent;
                api.GetAuthToken(code);

            }

        }

        void api_GetAuthTokenCompletedEvent(object sender, EventArgs e)
        {
            var api = sender as SoundcloudApi;

            if (api == null || api.OAuthResponse == null || api.UserProfile == null || string.IsNullOrEmpty(api.OAuthResponse.access_token))
            {
                UiHelper.SafeDispatchSync(() =>
                {
                    MessageBox.Show("Error", "Sorry, there was an error authenticating with SoundCloud.\n\nPlease ensure the date and time are correct on your phone and try again.",
                        MessageBoxButton.OK);

                    if(NavigationService.CanGoBack)
                        NavigationService.GoBack();
                });
                
                return;
            }

            var soundcloudAccess = new SoundcloudAccess
            {
                Id = api.UserProfile.id.ToString(CultureInfo.InvariantCulture),
                AccessToken = api.OAuthResponse.access_token,
                Fullname = api.UserProfile.full_name,
                ProfileUrl = api.UserProfile.avatar_url,
                UserName = api.UserProfile.username
            };

            var sh = new StorageHelper();

            sh.SaveUser(soundcloudAccess);

            sh.SaveProfileImageCompletedEvent += new EventHandler(ImageSaveCompleted);

            sh.SaveProfileImage(soundcloudAccess.ProfileUrl,
                                             soundcloudAccess.Id,
                                             api.UserProfile.id);
        }

        private void ImageSaveCompleted(object sender, EventArgs e)
        {

            // remove handler
            Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/MainPage.xaml?clear=true", UriKind.Relative)));

        }

    }
}