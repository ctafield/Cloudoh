using System;
using System.Windows.Navigation;
using Cloudoh.Common;

namespace Cloudoh.Classes
{
    public class CloudohUriMapper : UriMapperBase
    {

        public override Uri MapUri(Uri uri)
        {
            string _tempUri = System.Net.HttpUtility.UrlDecode(uri.ToString());

            // URI association launch for contoso.
            if (_tempUri.Contains("cloudoh://OpenPlaylist"))
            {

                int textIndex = _tempUri.IndexOf("Link=", StringComparison.Ordinal) + 5;
                string linkUrl = _tempUri.Substring(textIndex);

                return new Uri("/MainPage.xaml?SharedLink=" + Uri.EscapeDataString(linkUrl), UriKind.Relative);
            }
            if (_tempUri.ToLower().Contains("mainpage.xaml"))
            {
                if (App.ViewModel.CurrentUserViewModel == null)
                {
                    var sh = new StorageHelper();
                    if (sh.GetSoundcloudUser() == null)
                    {
                        var newUri = _tempUri.Replace("MainPage.xaml", "Welcome.xaml");
                        return new Uri(newUri, UriKind.RelativeOrAbsolute);
                    }
                }
            }

            // Otherwise perform normal launch.
            return uri;
        }

    }
}
