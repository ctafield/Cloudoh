using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Cloudoh.Common.ErrorLogging;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Shell;

namespace Cloudoh.Classes
{
    public static class UiHelper
    {

        public static void SafeDispatchSync(Action action)
        {
            if (Deployment.Current.Dispatcher.CheckAccess())
            { // do it now on this thread 
                action.Invoke();
            }
            else
            {
                EventWaitHandle wait = new AutoResetEvent(false);

                // do it on the UI thread 
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    action();
                    wait.Set();
                });
                wait.WaitOne();
            }
        }

        public static void SafeDispatch(Action action)
        {
            if (Deployment.Current.Dispatcher.CheckAccess())
            { // do it now on this thread 
                action.Invoke();
            }
            else
            {
                // do it on the UI thread 
                Deployment.Current.Dispatcher.BeginInvoke(action);
            }
        }

        public static void ShowProgressBar()
        {

            ShowProgressBar(string.Empty);

        }

        public static void ShowProgressBar(string text)
        {
            SafeDispatchSync(() =>
            {
                try
                {
                    if (SystemTray.ProgressIndicator == null)
                    {
                        SystemTray.ProgressIndicator = new ProgressIndicator()
                        {
                            IsIndeterminate = true,
                            IsVisible = true,
                            Text = text
                        };
                    }
                    else
                    {
                        SystemTray.ProgressIndicator.IsIndeterminate = true;
                        SystemTray.ProgressIndicator.IsVisible = true;
                        SystemTray.ProgressIndicator.Text = text;
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogException("ShowProgressBar", ex);
                }

            });
        }

        public static void HideProgressBar()
        {
            SafeDispatchSync(() =>
            {
                try
                {

                    if (SystemTray.ProgressIndicator != null)
                    {
                        SystemTray.ProgressIndicator.IsIndeterminate = false;
                        SystemTray.ProgressIndicator.IsVisible = false;
                        SystemTray.ProgressIndicator.Text = string.Empty;
                    }
                    else
                    {
                        SystemTray.ProgressIndicator = new ProgressIndicator
                        {
                            IsVisible = false,
                            IsIndeterminate = false
                        };
                    }

                }
                catch (Exception ex)
                {
                    ErrorLogger.LogException("HideProgressBar", ex);
                }

            });
        }

        public static Uri GetUnFavouriteImage()
        {
            //return UiHelper.GetCurrentTheme() == ThemeEnum.Dark ? new Uri("/Images/dark/appbar.favs.del.rest.png", UriKind.Relative) : new Uri("/Images/light/appbar.favs.del.rest.png", UriKind.Relative);
            return new Uri("/Images/76x76/dark/appbar.star.minus.png", UriKind.Relative);
        }

        public static Uri GetFavouriteImage()
        {
            //return UiHelper.GetCurrentTheme() == ThemeEnum.Dark ? new Uri("/Images/dark/appbar.favs.addto.rest.png", UriKind.Relative) : new Uri("/Images/light/appbar.favs.addto.rest.png", UriKind.Relative);
            return new Uri("/Images/76x76/dark/appbar.star.add.png", UriKind.Relative);
        }

        public static void ShowToast(string title, string message)
        {
            SafeDispatch(() =>
            {
                var newToast = new ToastPrompt()
                {
                    ImageSource = new BitmapImage(new Uri("/tile_toast.png", UriKind.Relative)),
                    Message = message,
                    Title = title,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                newToast.Show();
            });
        }

        public static void ShowToast(string message)
        {
            ShowToast("cloudoh", message);
        }

        public static DoubleAnimation CreateAnimation(double from, double to, double duration, PropertyPath targetProperty, DependencyObject target, TimeSpan? beginTime)
        {
            var db = new DoubleAnimation
            {
                To = to,
                From = from,
                EasingFunction = new SineEase(),
                Duration = TimeSpan.FromSeconds(duration),
                BeginTime = beginTime
            };
            Storyboard.SetTarget(db, target);
            Storyboard.SetTargetProperty(db, targetProperty);
            return db;
        }

        public static void NavigateTo(string targetPage)
        {
            var targetUri = new Uri(targetPage, UriKind.Relative);
            ((App)App.Current).RootFrame.Navigate(targetUri);
        }

        public static void ShowToastDelayed(string message, double delayInSeconds = 1.5)
        {

            SafeDispatch(() =>
            {
                var d = new DispatcherTimer
                            {
                                Interval = TimeSpan.FromSeconds(delayInSeconds)
                            };
                d.Tick += delegate(object sender, EventArgs args)
                              {
                                  d.Stop();
                                  ShowToast(message);
                              };
                d.Start();
            });
        }
    }

}
