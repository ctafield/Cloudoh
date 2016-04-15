// *********************************************************************************************************
// <copyright file="Banner.xaml.cs" company="My Own Limited">
// Copyright (c) 2013 All Rights Reserved
// </copyright>
// <summary>Cloudoh</summary>
// *********************************************************************************************************

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.ViewModels;
using Microsoft.Phone.BackgroundAudio;
using Newtonsoft.Json;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Cloudoh
{

    public partial class Banner : UserControl
    {

        #region Constructor

        public Banner()
        {
            InitializeComponent();

            ViewModel = new BannerViewModel()
                        {
                            IsPlaying = false
                        };

            Loaded += Banner_Loaded;
        }

        #endregion

        #region Properties

        private bool IsAnimating { get; set; }

        public BannerViewModel ViewModel { get; set; }

        private Storyboard firstNowPlayingStoryboard { get; set; }
        private Storyboard secondNowPlayingStoryboard { get; set; }
        private Storyboard thirdNowPlayingStoryboard { get; set; }

        private Storyboard firstLogoStoryboard { get; set; }
        private Storyboard secondLogoStoryboard { get; set; }


        private SoundcloudNowPlayingDetails CurrentTag { get; set; }

        #endregion

        private void Banner_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsAnimating)
                return;

            IsAnimating = true;

            DataContext = ViewModel;

            CreateLogoStoryboards();
            CreateAnimation();
            CreateNowPlaying();
        }

        private void CreateNowPlaying()
        {
            var timer = new DispatcherTimer();

            timer.Tick += delegate
                              {
                                  try
                                  {

                                      ViewModel.IsPlaying = (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing);

                                      if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing || BackgroundAudioPlayer.Instance.PlayerState == PlayState.Paused)
                                      {
                                          var currentTrack = BackgroundAudioPlayer.Instance.Track;

                                          var newTitle = string.Format("\x266b {0}", currentTrack.Title);

                                          if (newTitle != ViewModel.LastTitle)
                                          {
                                              ViewModel.LastTitle = newTitle;

                                              CurrentTag = GetModelFromTag(currentTrack.Tag);

                                              UiHelper.SafeDispatch(() =>
                                                                        {
                                                                            ViewModel.NowPlayingText = newTitle;

                                                                            UpdateSubInfo(currentTrack);

                                                                            ResetAnimations();

                                                                            if (ViewModel.IsExpanded)
                                                                                StartAnimatingNowPlaying();
                                                                            else
                                                                                ExpandAndStartAnimatingNowPlaying();
                                                                        });
                                          }
                                      }
                                      else
                                      {

                                          ViewModel.NowPlayingText = "";

                                          if (ViewModel.IsExpanded)
                                              CollapseNowPlaying();
                                      }
                                  }
                                  catch (Exception)
                                  {
                                      //throw;
                                  }
                              };

            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void ResetAnimations()
        {

            if (firstNowPlayingStoryboard != null)
                firstNowPlayingStoryboard.Stop();

            if (secondNowPlayingStoryboard != null)
                secondNowPlayingStoryboard.Stop();

            if (thirdNowPlayingStoryboard != null)
                thirdNowPlayingStoryboard.Stop();

            Canvas.SetLeft(txtNowPlaying, 0);
        }

        private void ExpandAndStartAnimatingNowPlaying()
        {

            var sb = new Storyboard
                         {
                             Duration = new Duration(TimeSpan.FromSeconds(1))
                         };

            sb.Children.Add(UiHelper.CreateAnimation(240, 435, 1, new PropertyPath(BannerViewModel.RectWidthProperty), ViewModel, TimeSpan.FromSeconds(0)));

            sb.Children.Add(UiHelper.CreateAnimation(0, 240, 1, new PropertyPath(TranslateTransform.XProperty), grid1.RenderTransform, TimeSpan.FromSeconds(0)));
            sb.Children.Add(UiHelper.CreateAnimation(0, 240, 1, new PropertyPath(TranslateTransform.XProperty), grid2.RenderTransform, TimeSpan.FromSeconds(0)));

            sb.Completed += delegate
                                {
                                    ViewModel.IsExpanded = true;
                                    StartAnimatingNowPlaying();
                                };

            sb.Begin();

        }

        private void CollapseNowPlaying()
        {
            // im not sure this can ever happen?
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


        private void CreateNowPlayingStoryBoards()
        {

            // first animation
            firstNowPlayingStoryboard = new Storyboard
                                  {
                                      Duration = new Duration(TimeSpan.FromSeconds(26))
                                  };

            var end = 0 - (txtNowPlaying.ActualWidth - 420);

            if (end > 0)
                end = 0;

            firstNowPlayingStoryboard.Children.Add(UiHelper.CreateAnimation(0, end, 25, new PropertyPath(Canvas.LeftProperty), txtNowPlaying, TimeSpan.FromSeconds(5)));

            firstNowPlayingStoryboard.Completed += delegate
                                             {
                                                 secondNowPlayingStoryboard.Begin();
                                             };

            // second animation
            secondNowPlayingStoryboard = new Storyboard
                                   {
                                       Duration = new Duration(TimeSpan.FromSeconds(1))
                                   };

            secondNowPlayingStoryboard.Children.Add(UiHelper.CreateAnimation(1, 0, 1, new PropertyPath(OpacityProperty), txtNowPlaying, TimeSpan.FromSeconds(0)));

            secondNowPlayingStoryboard.Completed += delegate
                                                        {
                                                            Canvas.SetLeft(txtNowPlaying, 0);
                                                            thirdNowPlayingStoryboard.Begin();
                                                        };

            // third animation
            thirdNowPlayingStoryboard = new Storyboard
                                            {
                                                Duration = new Duration(TimeSpan.FromSeconds(1))
                                            };

            thirdNowPlayingStoryboard.Children.Add(UiHelper.CreateAnimation(0, 1, 1, new PropertyPath(OpacityProperty), txtNowPlaying, TimeSpan.FromSeconds(0)));

            thirdNowPlayingStoryboard.Completed += delegate
                                                        {
                                                            firstNowPlayingStoryboard.Begin();
                                                        };

        }

        private void StartAnimatingNowPlaying()
        {
            CreateNowPlayingStoryBoards();
            firstNowPlayingStoryboard.Begin();
        }

        private void CreateAnimation()
        {
            var timer = new DispatcherTimer();
            timer.Tick += delegate(object sender, EventArgs args)
                              {
                                  if (App.ViewModel.CurrentUserViewModel != null)
                                  {
                                      ViewModel.UserName = App.ViewModel.CurrentUserViewModel.UserName;
                                      ViewModel.CachedImageUri = App.ViewModel.CurrentUserViewModel.CachedImageUri;

                                      var senderTimer = sender as DispatcherTimer;
                                      senderTimer.Stop();

                                      StartAnimatingLogo();
                                  }
                              };
            timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            timer.Start();
        }

        private void StartAnimatingLogo()
        {
            firstLogoStoryboard.Begin();
        }

        private void CreateLogoStoryboards()
        {
            firstLogoStoryboard = new Storyboard
                                      {
                                          Duration = new Duration(TimeSpan.FromSeconds(5))
                                      };

            firstLogoStoryboard.Children.Add(UiHelper.CreateAnimation(1, 0, 0.5, new PropertyPath(OpacityProperty), grid1, TimeSpan.FromSeconds(4)));
            firstLogoStoryboard.Children.Add(UiHelper.CreateAnimation(0, 1, 0.5, new PropertyPath(OpacityProperty), grid2, TimeSpan.FromSeconds(4.5)));

            firstLogoStoryboard.Completed += (sender, args) =>
            {
                secondLogoStoryboard.Begin();
            };

            secondLogoStoryboard = new Storyboard
                                       {
                                           Duration = new Duration(TimeSpan.FromSeconds(5))
                                       };

            secondLogoStoryboard.Children.Add(UiHelper.CreateAnimation(1, 0, 0.5, new PropertyPath(OpacityProperty), grid2, TimeSpan.FromSeconds(4)));
            secondLogoStoryboard.Children.Add(UiHelper.CreateAnimation(0, 1, 0.5, new PropertyPath(OpacityProperty), grid1, TimeSpan.FromSeconds(4.5)));

            secondLogoStoryboard.Completed += (sender, args) =>
            {
                if (!ViewModel.IsExpanded)
                    firstLogoStoryboard.Begin();
            };


        }

        private void nowPlayingTapped(object sender, GestureEventArgs e)
        {

            try
            {
                if (CurrentTag == null || BackgroundAudioPlayer.Instance.Track == null)
                    return;

                var openingAnimation = new RadFadeAnimation() { Duration = new Duration(TimeSpan.FromSeconds(0.45)) };

                openingAnimation.Ended += delegate(object o, AnimationEndedEventArgs args)
                {
                    WindowOpen = true;
                };

                window.OpenAnimation = openingAnimation;

                var closingAnimation = openingAnimation.CreateOpposite();

                closingAnimation.Ended += delegate(object o, AnimationEndedEventArgs args)
                {
                    WindowOpen = false;
                };

                window.CloseAnimation = closingAnimation;

                window.IsOpen = true;

                window.VerticalOffset = 50;

                var currentTrack = BackgroundAudioPlayer.Instance.Track;

                UpdateSubInfo(currentTrack);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

        }

        private void UpdateSubInfo(AudioTrack currentTrack)
        {

            ViewModel.Artist = currentTrack.Artist;
            ViewModel.Title = currentTrack.Title;

            if (CurrentTag != null)
                ViewModel.AlbumArtUrl = CurrentTag.AlbumArtRemote;

            AnimateStackWindowOn();
        }

        private void AnimateStackWindowOn()
        {

            if (!WindowOpen)
                return;

            UiHelper.SafeDispatch(() =>
            {
                stackDetailsWindow.Opacity = 0;

                var animatestack = new Storyboard
                {
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    BeginTime = new TimeSpan(0)
                };

                animatestack.Children.Add(UiHelper.CreateAnimation(0, 1, 0.5, new PropertyPath(OpacityProperty), stackDetailsWindow, TimeSpan.FromSeconds(0)));
                animatestack.Children.Add(UiHelper.CreateAnimation(30, 0, 0.5, new PropertyPath(TranslateTransform.XProperty), stackDetailsWindow.RenderTransform, TimeSpan.FromSeconds(0)));

                animatestack.Begin();
            });
        }

        private void backgroundBanner_Tap(object sender, GestureEventArgs e)
        {
            window.IsOpen = false;
        }

        public bool WindowOpen { get; set; }

        private void bannerText_Tap(object sender, GestureEventArgs e)
        {
            RedirectToDetailsPage();
        }

        private void RedirectToDetailsPage()
        {
            window.IsOpen = false;

            App.ViewModel.CurrentSoundcloud = new SoundcloudViewModel
            {
                Id = CurrentTag.Id,
                Type = "favoriting"
            };

            var newUri = new Uri("/SoundcloudDetails.xaml?random=" + DateTime.Now.Ticks, UriKind.Relative);

            (Application.Current as App).RootFrame.Navigate(newUri);
        }

        private void previousButton_Tap(object sender, GestureEventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipPrevious();
        }

        private void nextButton_Tap(object sender, GestureEventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipNext();
        }

        private void AlbumArt_Tap(object sender, GestureEventArgs e)
        {
            RedirectToDetailsPage();
        }

        private void Pause_Tap(object sender, GestureEventArgs e)
        {
            if (BackgroundAudioPlayer.Instance.CanPause && BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing)
                BackgroundAudioPlayer.Instance.Pause();
        }

        private void Play_Tap(object sender, GestureEventArgs e)
        {
            BackgroundAudioPlayer.Instance.Play();
        }

    }

}