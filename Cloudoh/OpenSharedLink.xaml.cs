using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.ViewModels.Playlists;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Telerik.Windows.Controls;

namespace Cloudoh
{

    public partial class OpenSharedLink : PhoneApplicationPage
    {

        private CloudohPlaylist Playlist { get; set; }
        private string SharedLink { get; set; }

        public OpenSharedLink()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            if (e.NavigationMode == NavigationMode.New)
            {

                if (string.IsNullOrEmpty(SharedLink))
                {

                    if (!NavigationContext.QueryString.ContainsKey("SharedLink"))
                    {
                        ShowFailed();
                        return;
                    }

                    var tempLink = Uri.UnescapeDataString(NavigationContext.QueryString["SharedLink"]);

                    if (string.IsNullOrEmpty(tempLink))
                    {
                        ShowFailed();
                        return;
                    }

                    var encodedArray = Convert.FromBase64String(tempLink);

                    var decodedLink = Encoding.UTF8.GetString(encodedArray, 0, encodedArray.Length);

                    var sh = new SkydriveHelper();
                    SharedLink = sh.GetDownloadLink(decodedLink);

                    var wc = new WebClient();
                    wc.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs args)
                                                      {
                                                          var contents = args.Result;

                                                          try
                                                          {
                                                              if (string.IsNullOrWhiteSpace(contents))
                                                              {
                                                                  ShowFailed();
                                                              }
                                                              else
                                                              {
                                                                  Playlist = JsonConvert.DeserializeObject<CloudohPlaylist>(contents);

                                                                  // overide whatever was there, should be user.
                                                                  Playlist.PlaylistType = CloudohPlaylistType.User;
                                                              }

                                                              UiHelper.SafeDispatch(() =>
                                                                                        {
                                                                                            DataContext = Playlist;

                                                                                            LoadingPlaylist.Visibility = Visibility.Collapsed;
                                                                                            PlaylistPanel.Visibility = Visibility.Visible;
                                                                                            ErrorLoading.Visibility = Visibility.Collapsed;

                                                                                            foreach (ApplicationBarIconButton item in ApplicationBar.Buttons)
                                                                                            {
                                                                                                item.IsEnabled = true;
                                                                                            }
                                                                                        });
                                                          }
                                                          catch (Exception)
                                                          {
                                                              ShowFailed();
                                                          }

                                                      };

                    wc.DownloadStringAsync(new Uri(SharedLink, UriKind.Absolute));

                }

            }

        }

        private void ShowFailed()
        {
            UiHelper.SafeDispatch(() =>
                                      {
                                          foreach (ApplicationBarIconButton item in ApplicationBar.Buttons)
                                          {
                                              item.IsEnabled = false;
                                          }                                          
                                          ErrorLoading.Visibility = Visibility.Visible;
                                          LoadingPlaylist.Visibility = Visibility.Collapsed;
                                          PlaylistPanel.Visibility = Visibility.Collapsed;
                                      });
        }

        private async void mnuSave_Click(object sender, EventArgs e)
        {

            if (LicenceHelper.IsLicensedForPremium())
                SavePlaylist();
            else
            {
                await LicenceHelper.PromptPurchase();
            }

        }

        private void SavePlaylist()
        {

            // give it a new ID, just in case
            Playlist.Id = new Guid();

            App.ViewModel.CloudohPlaylists.Add(Playlist);

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                var sh = new StorageHelper();
                sh.SaveCustomPlaylists(App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User));

                // 2. notify
                UiHelper.ShowToastDelayed(Playlist.Title + " saved to playlists");
            });

            // 3. return to the details page
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();

        }

    }

}