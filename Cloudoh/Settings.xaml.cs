using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.ViewModels.Playlists;
using CrittercismSDK;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Telerik.Windows.Controls;

namespace Cloudoh
{
    public partial class Settings : PhoneApplicationPage
    {

        private bool DataLoaded { get; set; }

        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!DataLoaded)
            {
                if (NavigationContext.QueryString.ContainsKey("tab") &&
                    NavigationContext.QueryString["tab"] == "search")
                {
                    pivotMain.SelectedIndex = 1;
                }

                LoadSettings();
                DataLoaded = true;
            }
        }

        private void LoadSettings()
        {
            var sh = new SettingsHelper();
            var settings = sh.GetSettings();

            try
            {
                // album art
                toggleAlbumArt.IsChecked = settings.ShowAlbumArtOnTile;
                toggleTrackHistory.IsChecked = settings.IncludeInMusicHub;
                toggleTransparentTile.IsChecked = settings.TransparentSmallTile;
                toggleSendDiagnostics.IsChecked = !Crittercism.GetOptOutStatus();

                // search settings
                toggleDownloadOnly.IsChecked = App.ViewModel.SearchSettings.DownloadableOnly.HasValue && App.ViewModel.SearchSettings.DownloadableOnly.Value;
                trackLengthMin.Value = App.ViewModel.SearchSettings.MinDuration;
                trackLengthMax.Value = App.ViewModel.SearchSettings.MaxDuration;
                bpmMax.Value = App.ViewModel.SearchSettings.BpmMaximum.GetValueOrDefault();
                bpmMax.Value = App.ViewModel.SearchSettings.BpmMinimum.GetValueOrDefault();

                stackBeats.Visibility = App.ViewModel.SearchSettings.FilterOnBpm.GetValueOrDefault() ? Visibility.Visible : Visibility.Collapsed;
                filterOnBpm.IsChecked = App.ViewModel.SearchSettings.FilterOnBpm.GetValueOrDefault();

                stackDuration.Visibility = App.ViewModel.SearchSettings.FilterOnDuration.GetValueOrDefault() ? Visibility.Visible : Visibility.Collapsed;
                filterOnDuration.IsChecked = App.ViewModel.SearchSettings.FilterOnDuration.GetValueOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            ThreadPool.QueueUserWorkItem(GetCacheImagesSizes);
            ThreadPool.QueueUserWorkItem(GetCacheDownloadsSizes);

        }

        private void GetCacheImagesSizes(object state)
        {
            var size = ImageCacheHelper.GetSizeOfCachedImages(ApplicationConstants.UserCacheStorageFolder) / 1024 / 1024;
            double x = Math.Truncate((double)size * 100) / 100;
            var s = string.Format("{0:N2}mb", x);

            UiHelper.SafeDispatch(() =>
            {
                txtCachedImage.Text = s;
                btnClearImages.IsEnabled = true;
            });

        }

        private void GetCacheDownloadsSizes(object state)
        {

            var size = ImageCacheHelper.GetSizeOfCachedImages(ApplicationConstants.DownloadFolder) / 1024 / 1024;
            double x = Math.Truncate((double)size * 100) / 100;
            var s = string.Format("{0:N2}mb", x);

            UiHelper.SafeDispatch(() =>
            {
                txtCachedTracks.Text = s;
                btnClearDownloads.IsEnabled = true;
            });

        }


        private void btnClearRecent_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.RecentSearches.Clear();
            App.ViewModel.SaveRecentSearches();
            UiHelper.ShowToast("recent searches cleared");
        }

        private async void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (LicenceHelper.IsLicensedForPremium())
            {
                await BackupPlaylists();
            }
            else
            {
                await LicenceHelper.PromptPurchase();
            }
        }

        private async Task BackupPlaylists()
        {

            var s = new SkydriveHelper();
            var sh = new StorageHelper();
            var contents = sh.SerialiseResponseObject(App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User));

            try
            {
                UiHelper.ShowProgressBar("uploading to OneDrive");
                btnBackup.IsEnabled = false;
                btnRestore.IsEnabled = false;
                await s.UploadPlaylists(contents);

            }
            catch (Exception)
            {
                MessageBox.Show("Unable to backup playlists to OneDrive.\n\nBe sure to allow Cloudoh access to write the backup.");
                return;
            }
            finally
            {
                btnBackup.IsEnabled = true;
                btnRestore.IsEnabled = true;
                UiHelper.HideProgressBar();
            }

        }

        private async void btnRestore_Click(object sender, RoutedEventArgs e)
        {

            if (LicenceHelper.IsLicensedForPremium())
            {
                await RestorePlaylists();
            }
            else
            {
                await LicenceHelper.PromptPurchase();
            }

        }

        private async Task RestorePlaylists()
        {

            var s = new SkydriveHelper();

            string result;

            try
            {
                UiHelper.ShowProgressBar("downloading from OneDrive");
                btnBackup.IsEnabled = false;
                btnRestore.IsEnabled = false;
                result = await s.DownloadPlaylists();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to restore playlists from OneDrive.\n\nBe sure to allow Cloudoh access to read the backup.");
                return;
            }
            finally
            {
                btnBackup.IsEnabled = true;
                btnRestore.IsEnabled = true;
                UiHelper.HideProgressBar();
            }


            if (string.IsNullOrEmpty(result))
            {
                MessageBox.Show("There were no backups found on SkyDrive.");
                return;
            }

            try
            {
                var playlists = JsonConvert.DeserializeObject<ObservableCollection<CloudohPlaylist>>(result);

                var plural = (playlists.Count != 1) ? "s" : "";

                var confirm = MessageBox.Show("Found " + playlists.Count + " playlist" + plural + ".\n\nAre you sure you want to replace your local playlists?", "restore playlists", MessageBoxButton.OKCancel);

                if (confirm == MessageBoxResult.OK)
                {

                    var itemsToRemove = App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User).ToList();

                    foreach (var item in itemsToRemove)
                    {
                        App.ViewModel.CloudohPlaylists.Remove(item);
                    }

                    foreach (var playlist in playlists)
                    {
                        App.ViewModel.CloudohPlaylists.Add(playlist);

                        var sh = new StorageHelper();
                        sh.SaveCustomPlaylists(App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User));

                        UiHelper.ShowToast("playlists restored");
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There were no valid backups found on OneDrive.");
            }

        }

        private void toggleDownloadOnly_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {

            if (!DataLoaded)
                return;

            App.ViewModel.SearchSettings.DownloadableOnly = toggleDownloadOnly.IsChecked;

            // save
            App.ViewModel.SearchSettings.Save();

        }

        private void ToggleTransparentSmallTile_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!DataLoaded)
                return;

            var sh = new SettingsHelper();
            var settings = sh.GetSettings();

            // album art
            settings.TransparentSmallTile = toggleTransparentTile.IsChecked;

            sh.SaveSettings(settings);

            // refresh
            ThreadPool.QueueUserWorkItem(App.ViewModel.UpdateLiveTile, App.ViewModel.SoundcloudDashboard.ToList());

        }

   
        private void ToggleAlbumArt_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {

            if (!DataLoaded)
                return;

            var sh = new SettingsHelper();
            var settings = sh.GetSettings();

            // album art
            settings.ShowAlbumArtOnTile = toggleAlbumArt.IsChecked;

            sh.SaveSettings(settings);

            // refresh
            ThreadPool.QueueUserWorkItem(App.ViewModel.UpdateLiveTile, App.ViewModel.SoundcloudDashboard.ToList());
        }

        private void ToggleTrackHistory_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {

            if (!DataLoaded)
                return;

            var sh = new SettingsHelper();
            var settings = sh.GetSettings();

            // album art
            settings.IncludeInMusicHub = toggleTrackHistory.IsChecked;

            sh.SaveSettings(settings);

        }

        private void BtnClearDownloads_OnClick(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show("This will delete all downloaded tracks and clear the download playlist. Are you sure?", "clear downloads", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            btnClearDownloads.IsEnabled = false;

            ThreadPool.QueueUserWorkItem(delegate
                                         {
                                             try
                                             {
                                                 var dh = new DownloadHelper();
                                                 dh.DeleteAllDownloads();
                                             }
                                             catch (Exception)
                                             {
                                             }
                                             ThreadPool.QueueUserWorkItem(GetCacheDownloadsSizes);
                                         });

        }

        private void BtnClearImages_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will delete all cached images. Are you sure?", "clear images", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            btnClearImages.IsEnabled = false;

            ThreadPool.QueueUserWorkItem(delegate(object state)
                                         {
                                             ImageCacheHelper.ClearCache();
                                             ThreadPool.QueueUserWorkItem(GetCacheImagesSizes);
                                         });


        }

        private void btnRestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            DataLoaded = false;

            trackLengthMin.Value = null;
            trackLengthMax.Value = null;
            toggleDownloadOnly.IsChecked = false;

            filterOnBpm.IsChecked = false;
            stackBeats.Visibility = Visibility.Collapsed;

            filterOnDuration.IsChecked = false;
            stackDuration.Visibility = Visibility.Collapsed;

            DataLoaded = true;

            App.ViewModel.SearchSettings.Save();
        }

        private void toggleFilterOnBpm_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!DataLoaded)
                return;

            App.ViewModel.SearchSettings.FilterOnBpm = filterOnBpm.IsChecked;

            stackBeats.Visibility = filterOnBpm.IsChecked ? Visibility.Visible : Visibility.Collapsed;

            App.ViewModel.SearchSettings.Save();
        }

        private void toggleFilterOnDuration_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {

            if (!DataLoaded)
                return;

            App.ViewModel.SearchSettings.FilterOnDuration = filterOnDuration.IsChecked;

            stackDuration.Visibility = filterOnDuration.IsChecked ? Visibility.Visible : Visibility.Collapsed;

            App.ViewModel.SearchSettings.Save();
        }

        private void numericMaxChanged(object sender, ValueChangedEventArgs<double> e)
        {
            if (!DataLoaded)
                return;

            App.ViewModel.SearchSettings.BpmMaximum = (short)e.NewValue;
            App.ViewModel.SearchSettings.Save();
        }

        private void numericMinChanged(object sender, ValueChangedEventArgs<double> e)
        {
            if (!DataLoaded)
                return;

            App.ViewModel.SearchSettings.BpmMinimum = (short)e.NewValue;
            App.ViewModel.SearchSettings.Save();
        }

        private void minDurationChanged(object sender, ValueChangedEventArgs<object> args)
        {
            if (!DataLoaded)
                return;

            App.ViewModel.SearchSettings.MinDuration = (TimeSpan)args.NewValue;
            App.ViewModel.SearchSettings.Save();
        }

        private void maxDurationChanged(object sender, ValueChangedEventArgs<object> args)
        {
            if (!DataLoaded)
                return;

            App.ViewModel.SearchSettings.MaxDuration = (TimeSpan)args.NewValue;
            App.ViewModel.SearchSettings.Save();
        }

        private void toggleSendDiagnostics_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!DataLoaded)
                return;

            CrittercismSDK.Crittercism.SetOptOutStatus(!toggleSendDiagnostics.IsChecked);
        }
    }

}