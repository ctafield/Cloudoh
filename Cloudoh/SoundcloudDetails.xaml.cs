using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Security;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Clarity.Phone.Extensions;
using Cloudoh;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.Common.ErrorLogging;
using Cloudoh.ExtensionMethods;
using Cloudoh.UserControls;
using Cloudoh.ViewModels;
using Coding4Fun.Toolkit.Controls;
using FieldOfTweets.Common.Api.Responses.Soundcloud;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Mitter.Soundcloud
{

    public partial class SoundcloudDetails : PhoneApplicationPage
    {

        private SoundcloudViewModel ThisTrack { get; set; }

        protected string AutoPlayId { get; set; }
        protected bool AutoPlayWhenLoaded { get; set; }

        private bool? IsFavourite { get; set; }

        protected bool IsPlaying { get; set; }

        protected bool DataLoaded { get; set; }

        public SoundcloudDetails()
        {
            InitializeComponent();

            Loaded += SoundcloudDetails_Loaded;

            var timer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(0.75)
                        };
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void SoundcloudDetails_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataLoaded)
                return;

            // remove any previous SoundcloudDetails.xaml pages
            if (NavigationService.CanGoBack)
            {
                bool done = false;
                while (!done)
                {
                    var last = NavigationService.BackStack.FirstOrDefault();
                    if (last != null)
                    {
                        if (last.Source.OriginalString.ToLower().Contains("soundclouddetails.xaml"))
                        {
                            NavigationService.RemoveBackEntry();
                        }
                        else
                        {
                            done = true;
                        }
                    }
                    else
                    {
                        done = true;
                    }
                }
            }

            if (App.ViewModel.CurrentSoundcloud == null || App.ViewModel.CurrentSoundcloud.Type == "favoriting")
            {

                DataLoaded = true;

                if (NavigationContext.QueryString.ContainsKey("autoPlay"))
                {
                    AutoPlayId = NavigationContext.QueryString["autoPlayId"];
                    AutoPlayWhenLoaded = true;
                }
                else if (NavigationContext.QueryString.ContainsKey("ExternalTrackId"))
                {
                    var trackId = int.Parse(NavigationContext.QueryString["ExternalTrackId"]);

                    if (!App.ViewModel.IsDataLoaded)
                    {
                        App.ViewModel.LoadData();
                    }

                    App.ViewModel.CurrentSoundcloud = new SoundcloudViewModel
                    {
                        Id = trackId
                    };
                }

                LoadDetails();

            }
            else
            {

                ThisTrack = App.ViewModel.CurrentSoundcloud.Clone();

                DataContext = ThisTrack;
                
                AssignMenu();

                DataLoaded = true;

                ShowDuration();

                LayoutRoot.Visibility = Visibility.Visible;
                borderWave.Visibility = Visibility.Visible;

                StartGetIsFavourite();
                GetComments();
            }  
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

          

        }

        private void AssignMenu()
        {
            if (ThisTrack == null)
                return;

            if (!string.IsNullOrEmpty(ThisTrack.DownloadUrl) && ThisTrack.Downloadable)
            {
                ApplicationBar = Resources["menuDownload"] as ApplicationBar;
                CheckIfAlreadyDownloaded();
            }
            else
            {
                ApplicationBar = Resources["menuNormal"] as ApplicationBar;
            }
        }

        private void CheckIfAlreadyDownloaded()
        {
            var dh = new DownloadHelper();
            var allDownloads = dh.GetAllDownloads();
            if (allDownloads.Any(x => x.Tag.Id == ThisTrack.Id))
            {
                (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = false;
            }
        }


        void timer_Tick(object sender, EventArgs e)
        {

            IsPlaying = false;

            try
            {
                if (ThisTrack != null)
                {
                    if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
                    {
                        var currentTag = BackgroundAudioPlayer.Instance.Track.Tag;
                        if (!string.IsNullOrEmpty(currentTag))
                        {
                            var tagObject = JsonConvert.DeserializeObject<SoundcloudTrack>(currentTag);
                            if (tagObject != null)
                            {
                                if (tagObject.Id == ThisTrack.Id)
                                {
                                    IsPlaying = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                IsPlaying = false;
            }

            if (IsPlaying)
            {
                try
                {
                    var track = BackgroundAudioPlayer.Instance.Track;

                    // set the time and the duration

                    if (track.Duration.Hours > 0)
                    {
                        txtPosition.Text = new DateTime(BackgroundAudioPlayer.Instance.Position.Ticks).ToString("HH.mm.ss");
                    }
                    else
                    {
                        txtPosition.Text = new DateTime(BackgroundAudioPlayer.Instance.Position.Ticks).ToString("mm.ss");
                    }

                    CurrentPosition = BackgroundAudioPlayer.Instance.Position;

                    var newPos = (480 / track.Duration.TotalSeconds) * CurrentPosition.Value.TotalSeconds;

                    linePosition.Width = newPos;

                    FadeBorderPosition(1);
                }
                catch (Exception ex)
                {
                    txtPosition.Text = string.Empty;
                    linePosition.Width = 0;
                    FadeBorderPosition(0);

                    ErrorLogger.LogException("timer_Tick", ex);
                }

            }
            else
            {
                txtPosition.Text = string.Empty;
                linePosition.Width = 0;
                CurrentPosition = null;
                FadeBorderPosition(0);
            }

            try
            {
                if (ApplicationBar != null)
                {
                    var mnu = (ApplicationBarIconButton)ApplicationBar.Buttons[0];

                    if (IsPlaying)
                    {
                        mnu.IconUri = new Uri("/Images/76x76/dark/appbar.control.pause.png", UriKind.Relative);
                        mnu.Text = "pause";
                    }
                    else
                    {
                        mnu.IconUri = new Uri("/Images/76x76/dark/appbar.control.play.png", UriKind.Relative);
                        mnu.Text = "play";
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        public TimeSpan? CurrentPosition { get; set; }

        private void FadeBorderPosition(double opacityTo)
        {

            if (borderPosition.Opacity == opacityTo)
                return;

            // Now animate it
            var sb = new Storyboard
            {
                BeginTime = TimeSpan.FromSeconds(0)
            };

            const double duration = 0.3;

            sb.Children.Add(CreateAnimation(opacityTo, duration, new PropertyPath(OpacityProperty), borderPosition));

            if (opacityTo > 0)
            {
                sb.Children.Add(CreateAnimation(0, -20, duration, new PropertyPath(TranslateTransform.XProperty), txtPosition.RenderTransform));
            }
            else
            {
                sb.Children.Add(CreateAnimation(-20, 0, duration, new PropertyPath(TranslateTransform.XProperty), txtPosition.RenderTransform));
            }

            sb.Duration = new Duration(TimeSpan.FromSeconds(duration));
            sb.Begin();

        }

        private static DoubleAnimation CreateAnimation(double to, double from, double duration, PropertyPath propertyPath, DependencyObject target)
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


        private static DoubleAnimation CreateAnimation(double to, double duration, PropertyPath propertyPath, DependencyObject target)
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

        private void ShowDuration()
        {
            var duration = App.ViewModel.CurrentSoundcloud.DurationTimeSpan;

            if (duration.Hours > 0)
            {
                txtDuration.Text = new DateTime(duration.Ticks).ToString("HH.mm.ss");
            }
            else
            {
                txtDuration.Text = new DateTime(duration.Ticks).ToString("mm.ss");
            }

            txtDuration.Visibility = Visibility.Visible;
        }

        private void LoadDetails()
        {

            UiHelper.ShowProgressBar("getting track details");

            ThreadPool.QueueUserWorkItem(delegate
                                         {
                                             if (App.ViewModel == null || App.ViewModel.CurrentSoundcloud == null)
                                             {
                                                 UiHelper.HideProgressBar();
                                                 return;
                                             }

                                             var trackId = App.ViewModel.CurrentSoundcloud.Id;

                                             var soundcloudApi = new SoundcloudApi();
                                             soundcloudApi.GetTrackCompletedEvent += new EventHandler(soundcloudApi_GetTrackCompletedEvent);
                                             soundcloudApi.GetTrack(trackId.ToString());
                                         });

        }

        private async void GetComments()
        {
            var api = new SoundcloudApi();
            var comments = await api.GetComments(ThisTrack.Id);
            api_GetCommentsCompletedEvent(comments);
        }

        private void api_GetCommentsCompletedEvent(List<SoundcloudComment> comments )
        {
            
            if (comments == null || !comments.Any())
            {
                UiHelper.SafeDispatchSync(() =>
                                          {
                                              noComments.Visibility = Visibility.Visible;
                                              lstComments.Visibility = Visibility.Collapsed;
                                          });
                return;
            }

            var commentViewModels = new List<CommentViewModel>();

            foreach (var comment in comments.OrderBy(x => x.timestamp))
            {
                var model = new CommentViewModel()
                                {
                                    AvatarUrl = comment.user.avatar_url,
                                    Comment = comment.body,
                                    TimeStamp = comment.timestamp
                                };

                commentViewModels.Add(model);
            }

            UiHelper.SafeDispatch(() =>
                                      {
                                          lstComments.DataContext = commentViewModels;

                                          noComments.Visibility = Visibility.Collapsed;
                                          lstComments.Visibility = Visibility.Visible;
                                      });

        }

        private async void StartGetIsFavourite()
        {
            var api = new SoundcloudApi();
            //api.GetIsFavouriteCompletedEvent += api_GetIsFavouriteCompletedEvent;
            var result = await api.GetIsFavourite(App.ViewModel.CurrentSoundcloud.Id);
            api_GetIsFavouriteCompletedEvent(result);
        }

        void api_GetIsFavouriteCompletedEvent(bool isFavourite)
        {

            // Change the favourite button
            UiHelper.SafeDispatch(() =>
                                      {
                                          try
                                          {
                                              if (ApplicationBar != null)
                                              {
                                                  if (ApplicationBar.Buttons[1] as ApplicationBarIconButton != null)
                                                  {
                                                      (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;

                                                      IsFavourite = isFavourite;

                                                      if (IsFavourite.Value)
                                                          (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IconUri = UiHelper.GetUnFavouriteImage();
                                                      else
                                                          (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IconUri = UiHelper.GetFavouriteImage();
                                                  }
                                              }
                                          }

                                          catch (Exception ex)
                                          {

                                          }
                                      });
        }

        void soundcloudApi_GetTrackCompletedEvent(object sender, EventArgs e)
        {

            var api = sender as SoundcloudApi;

            UiHelper.SafeDispatch(() =>
                                  {

                                      var thisTrack = api.TrackDetails.AsViewModel(0, ApplicationConstants.SoundcloudTypeEnum.Dashboard);

                                      if (thisTrack == null)
                                      {
                                          UiHelper.ShowToast("sorry! error getting track details");
                                          return;
                                      }

                                      App.ViewModel.CurrentSoundcloud = thisTrack;

                                      ThisTrack = App.ViewModel.CurrentSoundcloud.Clone();

                                      AssignMenu();

                                      var duration = App.ViewModel.CurrentSoundcloud.DurationTimeSpan;

                                      if (duration.Hours > 0)
                                      {
                                          txtDuration.Text = new DateTime(duration.Ticks).ToString("HH.mm.ss");
                                      }
                                      else
                                      {
                                          txtDuration.Text = new DateTime(duration.Ticks).ToString("mm.ss");
                                      }

                                      txtDuration.Visibility = Visibility.Visible;

                                      DataContext = App.ViewModel.CurrentSoundcloud;
                                      borderWave.Visibility = Visibility.Visible;

                                      DataLoaded = true;

                                      GetComments();

                                      LayoutRoot.Visibility = Visibility.Visible;

                                      if (AutoPlayWhenLoaded)
                                      {

                                          bool thisTrackIsPlaying = false;

                                          try
                                          {
                                              var currentTrack = BackgroundAudioPlayer.Instance.Track;
                                              if (currentTrack != null)
                                              {
                                                  var track = GetModelFromTag(currentTrack.Tag);

                                                  if (track.Id.ToString(CultureInfo.InvariantCulture) == AutoPlayId)
                                                      thisTrackIsPlaying = true;
                                              }
                                          }
                                          catch
                                          {
                                              thisTrackIsPlaying = false;
                                          }

                                          // Only play if not already playing.
                                          if (!thisTrackIsPlaying)
                                              mnuPlay_Click(this, null);

                                      }

                                  });


            StartGetIsFavourite();

            UiHelper.HideProgressBar();
        }

        private void mnuPlay_Click(object sender, EventArgs e)
        {

            try
            {
                if (IsPlaying && BackgroundAudioPlayer.Instance.CanPause)
                {
                    BackgroundAudioPlayer.Instance.Pause();
                    return;
                }

                AudioHelper.PlayTrack(ThisTrack);
            }
            catch (Exception)
            {
            }


        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {

            if (ShareBarPrompt != null)
            {
                if (ShareBarPrompt.IsOpen)
                {
                    ShareBarPrompt.Hide();
                    e.Cancel = true;
                    return;
                }
            }

            if (ImagePopup != null)
            {
                HideImagePopup();
                e.Cancel = true;
                return;
            }


            base.OnBackKeyPress(e);

        }

        private AppBarPrompt ShareBarPrompt { get; set; }

        private void mnuTweet_Click(object sender, EventArgs e)
        {
            var track = App.ViewModel.CurrentSoundcloud;

            string desc = string.Format("{0} - {1} {2}", track.Title, track.UserName, track.PermalinkUrl);

            var actions = new List<AppBarPromptAction>();

            actions.Add(new AppBarPromptAction("share via email", () =>
            {
                var emailComposeTask = new EmailComposeTask
                {
                    Subject = "Soundcloud track shared from Cloudoh"
                };

                emailComposeTask.Body = desc;
                emailComposeTask.Show();
            }));

            actions.Add(new AppBarPromptAction("share via mehdoh", async () =>
                                                                        {
                                                                            string text = HttpUtility.UrlEncode(desc) + HttpUtility.UrlEncode(track.PermalinkUrl);
                                                                            var uri = "mehdoh:TwitterPost?Text=" + text;
                                                                            await Windows.System.Launcher.LaunchUriAsync(new System.Uri("mehdoh:TwitterPost?Text=" + desc));
                                                                        }));

            actions.Add(new AppBarPromptAction("share via social network", () =>
            {
                var shareTask = new ShareStatusTask()
                                    {
                                        Status = desc
                                    };
                shareTask.Show();
            }));

            ShareBarPrompt = new AppBarPrompt(actions.ToArray())
                                 {
                                     Foreground = new SolidColorBrush(Colors.Black)
                                 };

            ShareBarPrompt.Show();

        }

        private void mnuFavourite_Click(object sender, EventArgs e)
        {
            if (!IsFavourite.HasValue)
                return;

            var api = new SoundcloudApi();
            if (IsFavourite.Value)
            {
                // remove from favourites    
                api.RemoveFromFavouritesCompletedEvent += new EventHandler(api_RemoveFromFavouritesCompletedEvent);
                api.RemoveFromFavourites(App.ViewModel.CurrentSoundcloud.Id);
            }
            else
            {
                api.AddToFavouritesCompletedEvent += new EventHandler(api_AddToFavouritesCompletedEvent);
                api.AddToFavourites(App.ViewModel.CurrentSoundcloud.Id);
            }

        }

        void api_AddToFavouritesCompletedEvent(object sender, EventArgs e)
        {
            UiHelper.ShowToast("track added to favourites");
            UiHelper.SafeDispatch(() =>
            {
                var button = (ApplicationBar.Buttons[1] as ApplicationBarIconButton);
                if (button != null)
                    button.IconUri = UiHelper.GetUnFavouriteImage();
            });
        }

        void api_RemoveFromFavouritesCompletedEvent(object sender, EventArgs e)
        {
            UiHelper.ShowToast("track removed from favourites");
            UiHelper.SafeDispatch(() =>
            {
                var button = (ApplicationBar.Buttons[1] as ApplicationBarIconButton);
                if (button != null)
                    button.IconUri = UiHelper.GetFavouriteImage();
            });
        }

        private void mnuViewProfile_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
                return;

            if (App.ViewModel.CurrentSoundcloud == null)
                return;

            var userId = App.ViewModel.CurrentSoundcloud.UserId;

            string uri = "/SoundcloudProfile.xaml?userId=" + userId;
            NavigationService.Navigate(new Uri(uri, UriKind.Relative));

        }

        private void waveForm_Up(object sender, MouseButtonEventArgs e)
        {

            if (!IsPlaying)
                return;

            try
            {
                if (stackSkip.Opacity > 0)
                {

                    var position = e.GetPosition(canvasWave);
                    // skip the track

                    // 100 / 480 * position.X
                    var pct = ((double)100 / 480) * position.X;
                    var newPos = ((double)(BackgroundAudioPlayer.Instance.Track.Duration.TotalSeconds / 100)) * pct;
                    BackgroundAudioPlayer.Instance.Position = TimeSpan.FromSeconds(newPos);

                    // hide the track
                    stackSkip.Opacity = 0;

                }

                e.Handled = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

        }

        private void waveForm_Down(object sender, MouseButtonEventArgs e)
        {

            try
            {
                if (!IsPlaying)
                    return;

                var position = e.GetPosition(canvasWave);

                SetSkipTime(position);

                stackSkip.Opacity = 1;

                e.Handled = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SetSkipTime(Point position)
        {

            try
            {
                if (position.X > 240) // 480 / 2
                {
                    Canvas.SetLeft(stackSkip, 10);
                }
                else
                {
                    Canvas.SetLeft(stackSkip, 380);
                }

                var newPos = (BackgroundAudioPlayer.Instance.Track.Duration.TotalSeconds / 480) * position.X;

                var newPosition = TimeSpan.FromSeconds(newPos);

                if (newPosition.Hours > 0)
                {
                    txtSkipTime.Text = new DateTime(newPosition.Ticks).ToString("HH.mm.ss");
                }
                else
                {
                    txtSkipTime.Text = new DateTime(newPosition.Ticks).ToString("mm.ss");
                }
            }
            catch (Exception)
            {
            }

        }

        private void waveForm_Move(object sender, MouseEventArgs e)
        {

            if (!IsPlaying)
                return;

            var position = e.GetPosition(canvasWave);

            // update the time
            SetSkipTime(position);

        }

        private SoundcloudNowPlayingDetails GetModelFromTag(string tag)
        {

            try
            {
                if (string.IsNullOrEmpty(tag))
                    return null;

                var model = JsonConvert.DeserializeObject<SoundcloudNowPlayingDetails>(tag);

                return model;
            }
            catch (Exception)
            {
                return null;
            }

        }

        private void mnuAddPlaylist_Click(object sender, EventArgs e)
        {
            var currentItem = App.ViewModel.CurrentSoundcloud;

            App.ViewModel.CurrentTrackToAdd = currentItem.Clone();

            UiHelper.NavigateTo("/AddToPlaylist.xaml");
        }

        private DialogService ImagePopup { get; set; }

        private void ShowImagePopup(object sender, GestureEventArgs e)
        {

            UiHelper.SafeDispatch(() =>
            {
                var imageSource = sender as Image;

                if (imageSource == null || imageSource.Source == null)
                    return;

                var panImage = new PanScanImage()
                {
                    Source = imageSource.Source
                };

                ImagePopup = new DialogService
                {
                    AnimationType = DialogService.AnimationTypes.Fade,
                    Child = panImage
                };

                ImagePopup.Show();

            });
        }

        private void HideImagePopup()
        {
            UiHelper.SafeDispatch(() =>
            {
                if (ImagePopup != null)
                {
                    EventHandler selectAccountPopupClosed = (sender, args) =>
                    {
                        ImagePopup = null;
                        ApplicationBar.IsVisible = true;
                    };
                    ImagePopup.Closed += selectAccountPopupClosed;
                    ImagePopup.Hide();
                }
            });
        }


        private void mnuSaveToLibrary_Tap(object sender, GestureEventArgs e)
        {

            var newFileName = "cloudoh_" + Guid.NewGuid().ToString().Replace("-", "") + ".jpg";

            var source = imageArt.Source as BitmapImage;
            var tempJpeg = ConvertToBytes(source);
            var library = new MediaLibrary();
            library.SavePicture(newFileName, tempJpeg);

            UiHelper.ShowToast("album art saved to photo album");
        }

        public static byte[] ConvertToBytes(BitmapImage bitmapImage)
        {
            using (var ms = new MemoryStream())
            {
                var btmMap = new WriteableBitmap(bitmapImage);

                // write an image into the stream
                btmMap.SaveJpeg(ms, bitmapImage.PixelWidth, bitmapImage.PixelHeight, 0, 100);

                return ms.ToArray();
            }
        }


        private bool showingDownloadMessage;

        private async void mnuDownload_Click(object sender, EventArgs e)
        {

            if (showingDownloadMessage)
                return;

            var dh = new DownloadHelper();

            var message = dh.AddDownloadLink(ThisTrack);

            if (!string.IsNullOrEmpty(message))
            {
                showingDownloadMessage = true;
                MessageBox.Show("Sorry, we you can't download this track at the current time:\n\n" + message, "download", MessageBoxButton.OK);
                showingDownloadMessage = false;
                return;
            }

            var result = await RadMessageBox.ShowAsync(new string[] { "OK", "Show Queue" }, "download", "Track has been added to the download queue.");

            if (result.ButtonIndex == 1)
            {
                NavigationService.Navigate(new Uri("/Downloads.xaml", UriKind.Relative));
            }
            else
            {
                CheckIfAlreadyDownloaded();
            }
        }

        private void mnuComment_Click(object sender, EventArgs e)
        {
            var args = string.Format("time={0}&trackId={1}&maxTime={2}", GetCurrentTime(), ThisTrack.Id, GetMaxTime());
            NavigationService.Navigate(new Uri("/AddComment.xaml?" + args, UriKind.Relative));
        }

        private double GetMaxTime()
        {
            return ThisTrack.DurationTimeSpan.TotalSeconds;
        }

        private double GetCurrentTime()
        {
            if (CurrentPosition == null)
                return 0;

            return CurrentPosition.Value.TotalSeconds;
        }

        private void mnuPin_Click(object sender, EventArgs e)
        {

            try
            {
                var tile = new CloudohTile();
                tile.SetValues(ThisTrack);
                tile.UpdateLayout();

                var tileSmall = new CloudohTileSmall();
                tileSmall.SetValues(ThisTrack);
                tileSmall.UpdateLayout();

                var newTile = new RadFlipTileData()
                {
                    SmallVisualElement = tileSmall,
                    VisualElement = tile,
                    Title = "",
                    MeasureMode = MeasureMode.Element,
                    IsTransparencySupported = false
                };

                LiveTileHelper.CreateOrUpdateTile(newTile, new Uri("/MainPage.xaml?ExternalTrackId=" + ThisTrack.Id, UriKind.Relative), false);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Sorry! Unable to pin track at the current time.\n\nPlease try again in a short while.", "Pin To Start", MessageBoxButton.OK);
            }

        }

    }

}