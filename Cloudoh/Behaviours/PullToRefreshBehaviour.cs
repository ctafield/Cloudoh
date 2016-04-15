using Telerik.Windows.Controls;

namespace System.Windows.Interactivity
{
    public class PullToRefreshBehavior : Behavior<RadDataBoundListBox>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the RefreshLabel in the PullToRefreshLoading indicator should be updated.
        /// </summary>
        public bool UpdateRefreshLabel { get; set; }

        /// <summary>
        /// Identifies the IsRunning dependency property.
        /// </summary>
        public static DependencyProperty IsRunningProperty =
            DependencyProperty.Register(
                "IsRunning",
                typeof(bool),
                typeof(PullToRefreshBehavior),
                new PropertyMetadata(true, OnIsRunningChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the PullToRefreshLoading indicator should stop.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return (bool)GetValue(PullToRefreshBehavior.IsRunningProperty);
            }
            set { SetValue(PullToRefreshBehavior.IsRunningProperty, value); }
        }

        /// <summary>
        /// IsRunningProperty property changed handler.
        /// </summary>
        /// <param name="d">PullToRefreshBehavior that changed its IsRunning property.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PullToRefreshBehavior)d).OnIsRunningChanged(e);
        }

        /// <summary>
        /// IsBusyProperty property changed handler.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected void OnIsRunningChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!IsRunning)
            {
                AssociatedObject.StopPullToRefreshLoading(UpdateRefreshLabel);
            }
        }

    }
}