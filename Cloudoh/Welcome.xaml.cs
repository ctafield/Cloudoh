using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Cloudoh.Classes;
using FieldOfTweets.Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Cloudoh
{
    public partial class Welcome : PhoneApplicationPage
    {
        public Welcome()
        {
            InitializeComponent();
            InteractionEffectManager.SetIsInteractionEnabled(btnConnect, true);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            UiHelper.SafeDispatch(() => NavigationService.Navigate(new Uri("/SoundcloudOAuth.xaml", UriKind.Relative)));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
                AnimateBoxesOn();

            txtVersion.Text = VersionInfo.FullVersion();
        }

        public void AnimateBoxesOn()
        {

            var timer = new DispatcherTimer();
            timer.Tick += delegate(object sender, EventArgs args)
            {

                var senderTimer = sender as DispatcherTimer;
                senderTimer.Stop();

                var sb = new Storyboard();

                sb.Children.Add(UiHelper.CreateAnimation(0, 1, 1, new PropertyPath(OpacityProperty), box1, TimeSpan.FromSeconds(0)));
                sb.Children.Add(UiHelper.CreateAnimation(100, 0, 1, new PropertyPath(CompositeTransform.TranslateXProperty), box1.RenderTransform, TimeSpan.FromSeconds(0)));

                sb.Children.Add(UiHelper.CreateAnimation(0, 1, 1, new PropertyPath(OpacityProperty), box2, TimeSpan.FromSeconds(0.1)));
                sb.Children.Add(UiHelper.CreateAnimation(-100, 0, 1, new PropertyPath(CompositeTransform.TranslateXProperty), box2.RenderTransform, TimeSpan.FromSeconds(0.1)));

                sb.Children.Add(UiHelper.CreateAnimation(0, 1, 1, new PropertyPath(OpacityProperty), box3, TimeSpan.FromSeconds(0.1)));
                sb.Children.Add(UiHelper.CreateAnimation(100, 0, 1, new PropertyPath(CompositeTransform.TranslateXProperty), box3.RenderTransform, TimeSpan.FromSeconds(0.2)));

                sb.Children.Add(UiHelper.CreateAnimation(0, 1, 0.6, new PropertyPath(OpacityProperty), box4, TimeSpan.FromSeconds(0.9)));

                sb.Duration = new Duration(TimeSpan.FromSeconds(2));
                sb.Begin();

            };
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();

        }

    }

}