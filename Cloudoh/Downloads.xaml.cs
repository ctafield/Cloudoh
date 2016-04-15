using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Navigation;
using System.Windows.Threading;
using Cloudoh.Classes;
using Cloudoh.ExtensionMethods;
using Cloudoh.ViewModels;
using Cloudoh.ViewModels.Playlists;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.BackgroundTransfer;
using Microsoft.Phone.Controls;
using Mitter.Soundcloud;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Cloudoh
{

    public partial class Downloads : PhoneApplicationPage
    {
        private bool DataLoaded { get; set; }
        public DownloadsViewModel ViewModel { get; set; }
        public DownloadQueueViewModel MenuItem { get; set; }

        public Downloads()
        {            
            InitializeComponent();
            ViewModel = new DownloadsViewModel();
        }

        public DispatcherTimer UpdateTimer { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!DataLoaded)
                LoadData();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (UpdateTimer != null)
            {
                UpdateTimer.Stop();
            }
        }

        private void LoadData()
        {
            DataLoaded = true;

            var downloadHelper = new DownloadHelper();

            ViewModel.FinishedDownloadQueue = new ObservableCollection<DownloadQueueViewModel>();
            ViewModel.ActiveDownloadQueue = new ObservableCollection<DownloadQueueViewModel>();

            ViewModel.FinishedDownloadQueue.AddRange(downloadHelper.GetFinishedDownloads());
            ViewModel.ActiveDownloadQueue.AddRange(downloadHelper.GetActiveDownloadQueue());

            lstActiveDownloads.DataContext = ViewModel.ActiveDownloadQueue;
            lstCompletedDownloads.DataContext = ViewModel.FinishedDownloadQueue;

            // do this here for now
            App.ViewModel.UpdateDownloadPlaylist();

            StartRefreshTimer();
        }

        private void StartRefreshTimer()
        {
            UpdateTimer = new DispatcherTimer
                          {
                              Interval = TimeSpan.FromSeconds(3)
                          };
            
            UpdateTimer.Tick += delegate
                                {
                                    var downloadHelper = new DownloadHelper();
                                    var newValues = downloadHelper.GetAllDownloads();

                                    foreach (var value in newValues)
                                    {
                                        var thisItem = ViewModel.ActiveDownloadQueue.FirstOrDefault(x => x.RequestId == value.RequestId);
                                        if (thisItem != null)
                                        {
                                            thisItem.Completion = value.Completion;
                                            thisItem.Status = value.Status;

                                            if (value.Status == TransferStatus.Completed)
                                            {
                                                ViewModel.ActiveDownloadQueue.Remove(thisItem);
                                                ViewModel.FinishedDownloadQueue.Add(thisItem);

                                                App.ViewModel.UpdateDownloadPlaylist();
                                            }
                                        }
                                    }

                                };

            UpdateTimer.Start();
        }
        

        private void mnuDelete_Tap(object sender, GestureEventArgs e)
        {
            var dh = new DownloadHelper();
            dh.DeleteDownload(MenuItem.RequestId, MenuItem.Tag.Id);
            LoadData();
        }

        private void MenuRemove_OnOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            var focusedItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (focusedItem == null)
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
                return;
            }

            MenuItem = focusedItem.AssociatedDataItem.Value as DownloadQueueViewModel;
            
        }

        private void playButton_Tap(object sender, GestureEventArgs e)
        {

            e.Handled = true;

            var stack = sender as RoundButton;

            var model = stack.DataContext as DownloadQueueViewModel;

            var downloadPlaylist = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.PlaylistType == CloudohPlaylistType.Downloaded);

            AudioHelper.PlayTrack(model.Tag, downloadPlaylist.Tracks.ToList());
        }

    }

}