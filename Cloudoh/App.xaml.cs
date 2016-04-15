using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Cloudoh.Classes;
using Cloudoh.Common;
using Cloudoh.ViewModels.Playlists;
using Coding4Fun.Toolkit.Controls;
using CrittercismSDK;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mitter.Soundcloud;
using Telerik.Windows.Controls;
using Cloudoh.ViewModels;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Cloudoh
{

    public partial class App : Application
    {

        private static MainViewModel viewModel = null;

        private bool reset;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
            set { viewModel = value; }
        }

        /// <summary>
        /// Component used to raise a notification to the end users to rate the application on the marketplace.
        /// </summary>
        public RadRateApplicationReminder rateReminder;

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            ThemeManager.OverrideOptions = ThemeManagerOverrideOptions.None;
            ThemeManager.ToLightTheme();
            ThemeManager.SetAccentColor(Color.FromArgb(255, 255, 102, 0));
            
            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //// Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                //PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;            
            }

            // Crittercism
            try
            {
                Crittercism.Init("52760020a7928a037e000001");
            }
            catch (Exception)
            {
            }
            
            //Creates a new instance of the RadRateApplicationReminder component.
            rateReminder = new RadRateApplicationReminder
                               {
                                   RecurrencePerUsageCount = 5,
                                   AllowUsersToSkipFurtherReminders = true
                               };

            //Sets how often the rate reminder is displayed.
        }


        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            //Before using any of the ApplicationBuildingBlocks, this class should be initialized with the version of the application.
            ApplicationUsageHelper.Init("1.0");
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (!e.IsApplicationInstancePreserved)
            {
                //This will ensure that the ApplicationUsageHelper is initialized again if the application has been in Tombstoned state.
                ApplicationUsageHelper.OnApplicationActivated();
            }

            // Ensure that application state is restored appropriately
            if (!App.ViewModel.IsDataLoaded)
            {
                //App.ViewModel.LoadDataAfterTombstoning();
                var th = new TombstoneHelper();
                th.LoadState();
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.

            // Save ViewModelState
            var th = new TombstoneHelper();
            th.SaveState();

        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {

            if (reset && e.IsCancelable && e.Uri.OriginalString == "/MainPage.xaml")
            {
                e.Cancel = true;
                reset = false;
            }

        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new RadPhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            RootFrame.UriMapper = new CloudohUriMapper();

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;
            RootFrame.Navigating += RootFrame_Navigating;
            RootFrame.Navigated += RootFrame_Navigated;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            reset = e.NavigationMode == NavigationMode.Reset;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        private void itemTapped(object sender, GestureEventArgs e)
        {

            e.Handled = true;

            try
            {
                var stack = sender as StackPanel;

                var model = stack.DataContext as SoundcloudViewModel;
                if (model != null)
                {
                    if (model.StreamType == ApplicationConstants.SoundcloudTypeEnum.Playlist)
                    {
                        ViewModel.CurrentPlaylist = model;

                        var newUri = new Uri("/SoundcloudPlaylistDetails.xaml", UriKind.Relative);
                        RootFrame.Dispatcher.BeginInvoke(() => RootFrame.Navigate(newUri));
                    }
                    else
                    {
                        ViewModel.CurrentSoundcloud = model;

                        var newUri = new Uri("/SoundcloudDetails.xaml", UriKind.Relative);
                        RootFrame.Dispatcher.BeginInvoke(() => RootFrame.Navigate(newUri));
                    }

                    return;
                }

                var playlist = stack.DataContext as CloudohPlaylist;
                if (playlist != null)
                {
                    var newUri = new Uri("/SoundcloudCustomPlaylist.xaml?Id=" + Uri.EscapeUriString(playlist.Id.ToString() + "&PermaLink=" + Uri.EscapeUriString(playlist.PermaLink)), UriKind.Relative);
                    RootFrame.Dispatcher.BeginInvoke(() => RootFrame.Navigate(newUri));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

        }

        private void playButton_Tap(object sender, GestureEventArgs e)
        {

            e.Handled = true;

            var stack = sender as RoundButton;

            if (stack.DataContext is SoundcloudViewModel)
            {
                var model = stack.DataContext as SoundcloudViewModel;

                if (model.StreamType == ApplicationConstants.SoundcloudTypeEnum.Playlist)
                {
                    App.ViewModel.CurrentPlaylist = model;

                    var newUri = new Uri("/SoundcloudPlaylistDetails.xaml", UriKind.Relative);

                    RootFrame.Dispatcher.BeginInvoke(() => RootFrame.Navigate(newUri));
                }
                else
                {
                    App.ViewModel.CurrentSoundcloud = model;

                    if (model.StreamType == ApplicationConstants.SoundcloudTypeEnum.CustomPlaylistTrack ||
                        model.StreamType == ApplicationConstants.SoundcloudTypeEnum.DownloadPlaylist || 
                        model.StreamType == ApplicationConstants.SoundcloudTypeEnum.SoundcloudPlaylistTrack)
                    {
                        var currentPage = ((App) Application.Current).RootFrame.Content as PhoneApplicationPage;
                        if (currentPage != null)
                        {
                            var dataContext = currentPage.DataContext as CloudohPlaylist;
                            if (dataContext != null)
                            {
                                AudioHelper.PlayTrack(model, dataContext.Tracks.ToList());
                            }
                            else
                            {
                                AudioHelper.PlayTrack(model);
                            }
                        }
                        else
                        {
                            AudioHelper.PlayTrack(model);
                        }
                    }
                    else if (model.StreamType == ApplicationConstants.SoundcloudTypeEnum.UsersTracks)
                    {
                        var currentPage = ((App) Application.Current).RootFrame.Content as SoundcloudProfile;
                        if (currentPage != null)
                        {
                            var dataContext = currentPage.UserTracks;
                            if (dataContext != null)
                            {
                                AudioHelper.PlayTrack(model, dataContext.ToList());   
                            }
                        }                        
                    }
                    else if (model.StreamType == ApplicationConstants.SoundcloudTypeEnum.UsersLikes)
                    {
                        var currentPage = ((App)Application.Current).RootFrame.Content as SoundcloudProfile;
                        if (currentPage != null)
                        {
                            var dataContext = currentPage.UserLikes;
                            if (dataContext != null)
                            {
                                AudioHelper.PlayTrack(model, dataContext.ToList());
                            }
                        }
                    }
                    else
                    {
                        AudioHelper.PlayTrack(model);                        
                    }
                }
            }
            else if (stack.DataContext is CloudohPlaylist)
            {

                var model = stack.DataContext as CloudohPlaylist;

                var firstTrack = model.FirstTrack;

                ViewModel.CurrentSoundcloud = model.FirstTrack;

                AudioHelper.PlayTrack(firstTrack, model.Tracks.ToList());
            }

        }

    }

}
