using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Windows.Phone.Media.Devices;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.UserControls;
using Cloudoh.ViewModels;
using Cloudoh.ViewModels.Playlists;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Mitter.Soundcloud;
using Newtonsoft.Json;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Cloudoh
{

    public partial class SoundcloudCustomPlaylist : PhoneApplicationPage
    {

        protected CloudohPlaylist ViewModel { get; set; }
        private SoundcloudViewModel MenuTrack { get; set; }
        private bool DataLoaded { get; set; }
        private Guid Id { get; set; }

        public SoundcloudCustomPlaylist()
        {
            InitializeComponent();
            Loaded += SoundcloudCustomPlaylist_Loaded;
        }

        void SoundcloudCustomPlaylist_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataLoaded)
                return;

            RadContextMenu.SetFocusedElementType(this.lstTracks, typeof(RadDataBoundListBoxItem));

            SetMenuNormal();

            var id = NavigationContext.QueryString["Id"];
            Id = Guid.Parse(id);

            if (NavigationContext.QueryString.ContainsKey("PermaLink"))
                PermaLink = NavigationContext.QueryString["PermaLink"];

            LoadData();
        }

        public string PermaLink { get; set; }

        private void LoadData()
        {

            if (Id != Guid.Empty)
                ViewModel = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.Id == Id).Clone();
            else
                ViewModel = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.PermaLink == PermaLink).Clone();

            this.DataContext = ViewModel;
            AssignMenu();
            DataLoaded = true;
        }

        private void AssignMenu()
        {
            if (ViewModel.PlaylistType == CloudohPlaylistType.User)
            {
                ApplicationBar = Resources["menuNormal"] as ApplicationBar;
            }
            else
            {
                ApplicationBar = Resources["menuBuiltinPlaylist"] as ApplicationBar;
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e)
        {

            var result = MessageBox.Show("Are you sure you want to delete this playlist?", "delete playlist", MessageBoxButton.OKCancel);

            if (result != MessageBoxResult.OK)
                return;

            var model = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.Id == Id);
            App.ViewModel.CloudohPlaylists.Remove(model);

            var sh = new StorageHelper();
            sh.SaveCustomPlaylists(App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User));

            UiHelper.SafeDispatch(() =>
                                      {
                                          if (NavigationService.CanGoBack)
                                          {
                                              NavigationService.GoBack();
                                          }
                                      });
        }

        private void mnuPlay_Click(object sender, EventArgs e)
        {

            try
            {
                var model = DataContext as CloudohPlaylist;

                if (model == null || App.ViewModel == null)
                    return;

                var firstTrack = model.FirstTrack;

                App.ViewModel.CurrentSoundcloud = model.FirstTrack;

                AudioHelper.PlayTrack(firstTrack, model.Tracks.ToList());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        private bool isShareOpen;

        private async void mnuShare_Click(object sender, EventArgs e)
        {

            if (isShareOpen)
                return;


            var sh = new StorageHelper();
            var contents = sh.LoadContentsFromFile<DontAskAgain>("ConfirmPlaylist");

            if (contents == null)
            {

                isShareOpen = true;

                var result = await RadMessageBox.ShowAsync("share playlist", MessageBoxButtons.YesNo, "This will upload the playlist (note: not the tracks) to your OneDrive account, and then share a link to it.\n\nContinue?", "Don't ask me again.", false);

                isShareOpen = false;

                if (result.Result == DialogResult.Cancel)
                    return;

                if (result.IsCheckBoxChecked)
                {
                    var dontask = new DontAskAgain {DontAsk = true};
                    sh.SaveContentsToFile("ConfirmPlaylist", dontask);
                }
            }

            UiHelper.SafeDispatchSync(() =>
                                          {
                                              ApplicationBar.IsMenuEnabled = false;
                                              foreach (ApplicationBarIconButton item in ApplicationBar.Buttons)
                                              {
                                                  item.IsEnabled = false;
                                              }
                                          });

            var model = DataContext as CloudohPlaylist;
            if (model == null)
                return;

            UiHelper.ShowProgressBar("getting link from OneDrive");

            var s = new SkydriveHelper();
            var content = JsonConvert.SerializeObject(model);

            try
            {
                var shareLink = await s.SharePlaylist(content, model.Id.ToString());

                if (string.IsNullOrEmpty(shareLink))
                    return;

                var encodedLink = Convert.ToBase64String(Encoding.UTF8.GetBytes(shareLink));


                var shareTask = new EmailComposeTask
                {
                    Subject = "Playlist shared from Cloudoh"
                };

                shareTask.Body = "I have shared a playlist called '" + model.Title + "'. Click this link to open it in Cloudoh cloudoh://OpenPlaylist?Link="
                                 + HttpUtility.UrlEncode(encodedLink)
                                 + Environment.NewLine + Environment.NewLine + Environment.NewLine
                                 + "If you don't have Cloudoh installed then you can download it for free from the Windows Phone Store:"
                                 + Environment.NewLine
                                 + ApplicationConstants.CloudohAppStoreLink
                                 + Environment.NewLine + Environment.NewLine + Environment.NewLine
                                 + "Cloudoh is a SoundCloud client exclusive to Windows Phone 8.";

                shareTask.Show();
            }
            catch (Exception)
            {
            }
            finally
            {
                UiHelper.HideProgressBar();

                UiHelper.SafeDispatchSync(() =>
                {
                    try
                    {
                        ApplicationBar.IsMenuEnabled = true;
                        foreach (ApplicationBarIconButton item in ApplicationBar.Buttons)
                        {
                            item.IsEnabled = true;
                        }

                    }
                    catch (Exception)
                    {
                    }
                });
            }

        }

        private void mnuReorder_Click(object sender, EventArgs e)
        {

            SetMenuSave();
            InteractionEffectManager.SetIsInteractionEnabled(lstTracks, false);
            lstTracks.IsItemReorderEnabled = true;
            AnimateOnInstructions();

        }

        private static DoubleAnimation CreateAnimation(double to, double from, double duration,
                                               PropertyPath propertyPath, DependencyObject target)
        {
            var db = new DoubleAnimation
            {
                To = to,
                From = from,
                EasingFunction = new SineEase(),
                Duration = TimeSpan.FromSeconds(duration),
            };
            Storyboard.SetTarget(db, target);
            Storyboard.SetTargetProperty(db, propertyPath);
            return db;
        }


        private void FadeBorderPosition(double opacityTo)
        {

            if (borderInstructions.Opacity == opacityTo)
                return;

            // Now animate it
            var sb = new Storyboard
            {
                BeginTime = TimeSpan.FromSeconds(0)
            };

            const double duration = 0.3;

            sb.Children.Add(CreateAnimation(opacityTo, duration, new PropertyPath(OpacityProperty), borderInstructions));

            if (opacityTo > 0)
            {
                sb.Children.Add(CreateAnimation(0, 30, duration, new PropertyPath(TranslateTransform.XProperty), txtInstructions.RenderTransform));
            }
            else
            {
                sb.Children.Add(CreateAnimation(-30, 0, duration, new PropertyPath(TranslateTransform.XProperty), txtInstructions.RenderTransform));
            }

            sb.Duration = new Duration(TimeSpan.FromSeconds(duration));
            sb.Begin();

        }

        private static DoubleAnimation CreateAnimation(double to, double duration,
                                                       PropertyPath propertyPath, DependencyObject target)
        {
            var db = new DoubleAnimation
            {
                To = to,
                EasingFunction = new SineEase(),
                Duration = TimeSpan.FromSeconds(duration)
            };
            Storyboard.SetTarget(db, target);
            Storyboard.SetTargetProperty(db, propertyPath);
            return db;
        }


        private void AnimateOnInstructions()
        {
            FadeBorderPosition(1);
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {

            FadeBorderPosition(0);

            SaveUpdates();

            InteractionEffectManager.SetIsInteractionEnabled(lstTracks, true);
            lstTracks.IsItemReorderEnabled = false;

            SetMenuNormal();

        }

        protected bool Saved { get; set; }

        private void SetMenuNormal()
        {
            ApplicationBar = (ApplicationBar)Resources["menuNormal"];
            ApplicationBar.MatchOverriddenTheme();
        }

        private void SetMenuSave()
        {
            ApplicationBar = (ApplicationBar)Resources["menuSave"];
            ApplicationBar.MatchOverriddenTheme();
        }

        private void SetMenuDeleteSave()
        {
            ApplicationBar = (ApplicationBar)Resources["menuDeleteSave"];
            ApplicationBar.MatchOverriddenTheme();
        }

        private void SaveUpdates()
        {

            if (ViewModel.Tracks.Count == 0)
            {
                var result = MessageBox.Show("Are you sure you want to delete this playlist?", "delete playlist", MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                    return;

                DeletePlaylist();

                return;
            }

            Saved = true;

            // reorder
            int startPos = 0;

            foreach (var item in ViewModel.Tracks)
            {
                item.Index = startPos++;
            }

            var modelToRemove = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.Id == Id);
            var index = App.ViewModel.CloudohPlaylists.IndexOf(modelToRemove);

            App.ViewModel.CloudohPlaylists.Remove(modelToRemove);
            App.ViewModel.CloudohPlaylists.Insert(index, ViewModel.Clone());

            lstTracks.IsCheckModeActive = false;

            var sh = new StorageHelper();
            sh.SaveCustomPlaylists(App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User));

        }

        private void DeletePlaylist()
        {
            var model = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.Id == Id);

            App.ViewModel.CloudohPlaylists.Remove(model);

            var sh = new StorageHelper();
            sh.SaveCustomPlaylists(App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User));

            UiHelper.SafeDispatch(() =>
            {
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            });

        }

        private void mnuRemove_Click(object sender, EventArgs e)
        {
            foreach (var item in lstTracks.CheckedItems.ToList())
            {
                ViewModel.Tracks.Remove(item as SoundcloudViewModel);
            }
        }

        private void LstTracks_OnIsCheckModeActiveChanged(object sender, IsCheckModeActiveChangedEventArgs e)
        {
            if (e.CheckBoxesVisible)
            {
                Saved = false;
                SetMenuDeleteSave();
            }
            else
            {
                if (!Saved)
                {
                    lstTracks.CheckedItems.Clear();

                    // restore 
                    var existing = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.Id == Id);
                    foreach (var track in existing.Tracks.ToList())
                    {
                        if (ViewModel.Tracks.All(x => x.Id != track.Id))
                        {
                            ViewModel.Tracks.Add(track.Clone());
                        }
                    }
                }

                SetMenuNormal();
            }
        }


        private bool isRenaming;

        private async void mnuRename_Click(object sender, EventArgs e)
        {

            if (isRenaming)
                return;

            isRenaming = true;

            var template = Resources["RenameInputTemplate"] as ControlTemplate;

            InputPromptClosedEventArgs result = await RadInputPrompt.ShowAsync(template, "rename playlist", new List<object> { "OK", "Cancel" });

            if (result.ButtonIndex == 0)
            {
                ViewModel.Title = result.Text;

                // regrab the original one
                var existing = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.Id == Id);
                if (existing != null)
                {
                    existing.Title = result.Text;
                    var sh = new StorageHelper();
                    sh.SaveCustomPlaylists(App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User));
                }
            }

            isRenaming = false;

        }

        private void mnuRemove_Tap(object sender, GestureEventArgs e)
        {

            if (ViewModel.Tracks.Count == 1)
            {
                var result = MessageBox.Show("Are you sure you want to delete this playlist?", "delete playlist", MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                    return;

                DeletePlaylist();

                return;
            }

            var existingModel = App.ViewModel.CloudohPlaylists.FirstOrDefault(x => x.Id == Id);
            var existingTrack = existingModel.Tracks.SingleOrDefault(x => x.Id == MenuTrack.Id);
            if (existingTrack != null)
                existingModel.Tracks.Remove(existingTrack);

            ViewModel.Tracks.Remove(MenuTrack);
            MenuTrack = null;

            // save
            var sh = new StorageHelper();
            sh.SaveCustomPlaylists(App.ViewModel.CloudohPlaylists.Where(x => x.PlaylistType == CloudohPlaylistType.User));
        }

        private void MenuRemove_OnOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            RadDataBoundListBoxItem focusedItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (focusedItem == null)
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
                return;
            }

            // only user playlists
            if (ViewModel.PlaylistType != CloudohPlaylistType.User)
            {
                e.Cancel = true;
                return;
            }

            if (lstTracks.IsItemReorderEnabled)
            {
                e.Cancel = true;
                return;
            }

            MenuTrack = focusedItem.AssociatedDataItem.Value as SoundcloudViewModel;
        }

        private void mnuPinToStart_Click(object sender, EventArgs e)
        {

            var tile = new CloudohTile();
            tile.SetValues(ViewModel.FirstTrack);
            tile.UpdateLayout();

            var tileSmall = new CloudohTileSmall();
            tileSmall.SetValues(ViewModel.FirstTrack);
            tileSmall.UpdateLayout();

            var newTile = new RadFlipTileData()
            {
                SmallVisualElement = tileSmall,
                VisualElement = tile,
                Title = "",
                MeasureMode = MeasureMode.Element,
                IsTransparencySupported = false
            };

            LiveTileHelper.CreateOrUpdateTile(newTile, new Uri("/MainPage.xaml?ExternalCustomPlaylistId=" + ViewModel.Id + "&PermaLink=" + Uri.EscapeUriString(ViewModel.PermaLink), UriKind.Relative), false);

        }

    }
}