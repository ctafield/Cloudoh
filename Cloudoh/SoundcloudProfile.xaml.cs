using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.Common.API.Soundcloud;
using Cloudoh.Common.ErrorLogging;
using Cloudoh.ExtensionMethods;
using Cloudoh.ViewModels;
using FieldOfTweets.Common.Api.Responses.Soundcloud;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Cloudoh
{
    public partial class SoundcloudProfile : PhoneApplicationPage
    {

        protected bool DataLoaded { get; set; }
        protected long UserId { get; set; }
        protected SoundcloudUserViewModel Model { get; set; }
        public ObservableCollection<SoundcloudViewModel> UserLikes { get; set; }
        public ObservableCollection<SoundcloudViewModel> UserTracks { get; set; }
        protected bool DoesUserFollow { get; set; }

        public SoundcloudProfile()
        {
            InitializeComponent();


            Loaded += SoundcloudProfile_Loaded;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void SoundcloudProfile_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataLoaded)
                return;

            DataLoaded = true;

            string temp;
            if (!NavigationContext.QueryString.TryGetValue("userId", out temp))
            {
                if (App.ViewModel.CurrentSoundcloudProfile != null)
                {
                    SetModelFromState();
                }
                else
                {
                    NavigationService.GoBack();
                }

            }
            else
            {
                UserId = int.Parse(temp);
                LoadProfile();
            }

            ThreadPool.QueueUserWorkItem(GetFollowingStatus);
        }

        void timer_Tick(object sender, EventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void GetFollowingStatus(object state)
        {
            if (App.ViewModel.CurrentUserViewModel.Id == UserId.ToString(CultureInfo.InvariantCulture))
                return;

            var api = new SoundcloudApi();
            api.DoesUserFollowUserCompletedEvent += ApiOnDoesUserFollowUserCompletedEvent;
            api.DoesUserFollowUser(App.ViewModel.CurrentUserViewModel.Id, UserId);
        }

        private void ApiOnDoesUserFollowUserCompletedEvent(object sender, EventArgs eventArgs)
        {
            var api = sender as SoundcloudApi;

            DoesUserFollow = api.DoesUserFollow;

            UiHelper.SafeDispatch(() =>
                                      {
                                          try
                                          {
                                              var mnuFollowButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;

                                              if (DoesUserFollow)
                                              {
                                                  mnuFollowButton.IconUri = new Uri("/Images/76x76/light/appbar.minus.png", UriKind.Relative);
                                                  mnuFollowButton.Text = "un-follow";
                                              }
                                              else
                                              {
                                                  mnuFollowButton.IconUri = new Uri("/Images/76x76/light/appbar.add.png", UriKind.Relative);
                                                  mnuFollowButton.Text = "follow";
                                              }

                                              mnuFollowButton.IsEnabled = true;
                                          }
                                          catch (Exception)
                                          {
                                          }
                                          finally
                                          {
                                              UiHelper.HideProgressBar();
                                          }

                                      });
        }

        private void SetModelFromState()
        {
            Model = App.ViewModel.CurrentSoundcloudProfile.Clone();
            UserId = Model.Id;

            UiHelper.SafeDispatch(() =>
            {
                DataContext = Model;
                LayoutRoot.Visibility = Visibility.Visible;
            });

        }

        private async void LoadProfile()
        {
            UiHelper.ShowProgressBar("fetching user profile");

            var api = new SoundcloudApi();
            var profile = await api.GetUserProfile(UserId);
            GetUserProfileCompletedEvent(profile);

        }

        private void GetUserProfileCompletedEvent(SoundcloudUserProfile profile)
        {

            UiHelper.HideProgressBar();

            if (profile == null)
                return;

            try
            {

                Model = profile.AsViewModel();

                UiHelper.SafeDispatch(() =>
                {
                    DataContext = Model;
                    LayoutRoot.Visibility = Visibility.Visible;
                });

            }
            catch (Exception ex)
            {
                ErrorLogger.LogException("GetUserProfileCompletedEvent", ex);
            }

        }


        private void PivotMain_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (pivotMain.SelectedIndex)
            {
                case 0: // userprofile, dont care
                    break;

                case 1: // tracks
                    CheckLoadTracks();
                    break;

                case 2: // favourites
                    CheckLoadLikes();
                    break;

            }

        }

        private void CheckLoadTracks()
        {
            if (UserTracks == null)
                LoadTracks();
        }

        private void LoadTracks()
        {
            var api = new SoundcloudApi();
            api.GetUserTracksCompletedEvent += api_GetUserTracksCompletedEvent;
            api.GetUserTracks(UserId);
        }

        private void api_GetUserTracksCompletedEvent(object sender, EventArgs e)
        {

            var api = sender as SoundcloudApi;

            if (api == null || api.Tracks == null || api.Tracks.Count == 0)
            {
                UiHelper.SafeDispatch(() =>
                                           {
                                               lstTracks.Visibility = Visibility.Collapsed;
                                               noTracks.Visibility = Visibility.Visible;
                                           });
                return;
            }

            UserTracks = new ObservableCollection<SoundcloudViewModel>();

            UiHelper.SafeDispatch(() =>
            {

                lstTracks.DataContext = UserTracks;

                if (api.Tracks != null && api.Tracks.Count > 0)
                {
                    var index = 0;
                    foreach (var track in api.Tracks)                    
                    {                        
                        var model = track.AsViewModel(index++, ApplicationConstants.SoundcloudTypeEnum.UsersTracks);
                        UserTracks.Add(model);
                    }
                }

            });

        }

        private void CheckLoadLikes()
        {
            if (UserLikes == null)
                LoadLikes();
        }

        private async void LoadLikes()
        {
            var api = new SoundcloudApi();
            var result = await api.GetUserFavorites(UserId);
            api_GetUserFavoritesCompletedEvent(result);
        }

        void api_GetUserFavoritesCompletedEvent(List<ResponseGetTrack> favourites )
        {

            if (favourites == null || favourites.Count == 0)
            {
                UiHelper.SafeDispatch(() =>
                {
                    lstLikes.Visibility = Visibility.Collapsed;
                    noLikes.Visibility = Visibility.Visible;
                });

                return;
            }

            UserLikes = new ObservableCollection<SoundcloudViewModel>();

            UiHelper.SafeDispatch(() =>
            {

                lstLikes.DataContext = UserLikes;

                if (favourites.Count > 0)
                {
                    var index = 0;
                    foreach (var track in favourites)
                    {
                        var model = track.AsViewModel(index++, ApplicationConstants.SoundcloudTypeEnum.UsersLikes);
                        UserLikes.Add(model);
                    }
                }

            });

        }

        private void mnuFollow_Click(object sender, EventArgs e)
        {

            if (DoesUserFollow)
            {
                // unfollow
                UnFollowUser();
            }
            else
            {
                // follow
                FollowUser();
            }

        }

        private void DisableFollowButton()
        {
            UiHelper.SafeDispatch(() =>
            {
                var mnuFollowButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                mnuFollowButton.IsEnabled = false;
            });
        }

        private void EnableFollowButton()
        {
            UiHelper.SafeDispatch(() =>
            {
                var mnuFollowButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                mnuFollowButton.IsEnabled = true;
            });
        }

        private void FollowUser()
        {
            UiHelper.ShowProgressBar();
            DisableFollowButton();

            var api = new SoundcloudApi();
            api.FollowUserCompletedEvent += delegate
                                                {
                                                    EnableFollowButton();
                                                    ThreadPool.QueueUserWorkItem(GetFollowingStatus);
                                                };
            api.FollowUser(App.ViewModel.CurrentUserViewModel.Id, UserId);

        }

        private void UnFollowUser()
        {
            UiHelper.ShowProgressBar();
            DisableFollowButton();

            var api = new SoundcloudApi();
            api.UnFollowUserCompletedEvent += delegate
            {
                EnableFollowButton();
                ThreadPool.QueueUserWorkItem(GetFollowingStatus);
            };
            api.UnFollowUser(App.ViewModel.CurrentUserViewModel.Id, UserId);
        }

        private void followsTap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Follows.xaml?Page=following&UserId=" + UserId, UriKind.Relative));
        }

        private void followersTap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Follows.xaml?Page=followers&UserId=" + UserId, UriKind.Relative));
        }

        private void mnuFollowing_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Follows.xaml?Page=following&UserId=" + UserId, UriKind.Relative));
        }

        private void mnuFollowers_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Follows.xaml?Page=followers&UserId=" + UserId, UriKind.Relative));
        }

    }

}