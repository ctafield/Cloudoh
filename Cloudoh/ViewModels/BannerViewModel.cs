// *********************************************************************************************************
// <copyright file="BannerViewModel.cs" company="My Own Limited">
// Copyright (c) 2013 All Rights Reserved
// </copyright>
// <summary>Cloudoh</summary>
// *********************************************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using Cloudoh.Annotations;
using Cloudoh.Classes;
using Cloudoh.Common;
using Newtonsoft.Json;

namespace Cloudoh.ViewModels
{
    public class BannerViewModel : DependencyObject, INotifyPropertyChanged
    {
        #region Fields

        private bool _isExpanded;
        private string _nowPlayingText;
        private string _lastTitle;
        private string _userName;
        private string _cachedImageUri;
        private string _title;
        private string _artist;
        private string _albumArtUrl;

        #endregion

        #region Constructor

        public BannerViewModel()
        {
            // default to this
            RectWidth = 240;
            IsExpanded = false;
        }

        #endregion

        #region Properties

        [JsonIgnore]
        public Visibility PauseVisibility   
        {
            get { return _pauseVisibility; }
            set
            {
                if (value == _pauseVisibility) 
                    return;
                _pauseVisibility = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public Visibility PlayVisibility
        {
            get { return _playVisiblity; }
            set
            {
                if (value == _playVisiblity) 
                    return;
                _playVisiblity = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public bool IsPlaying
        {
            set
            {
                PauseVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                PlayVisibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        //[JsonIgnore]
        //public Uri AlbumArtImageSource
        //{
        //    get
        //    {
        //        if (_albumArtImageSource != null)
        //            return _albumArtImageSource;

        //        if (string.IsNullOrWhiteSpace(AlbumArtUrl))
        //            return null;

        //        ThreadPool.QueueUserWorkItem(delegate(object state)
        //        {
        //            var model = state as BannerViewModel;

        //            var urlParts = model.AlbumArtUrl.Split('/');
        //            var currentImage = urlParts[urlParts.Length - 1];

        //            if (currentImage.Contains("?"))
        //                currentImage = currentImage.Substring(0, currentImage.IndexOf('?'));

        //            const string userId = "thumb";

        //            if (ImageCacheHelper.IsProfileImageCached(userId, currentImage))
        //            {
        //                _albumArtImageSource = ImageCacheHelper.GetUriForCachedImage(userId, currentImage);
        //                OnPropertyChanged("AlbumArtUrl");
        //                OnPropertyChanged("AlbumArtImageSource");
        //            }
        //            else
        //            {
        //                ImageCacheHelper.CacheImage(model.AlbumArtUrl, userId, currentImage, () =>
        //                {
        //                    OnPropertyChanged("AlbumArtUrl");
        //                    OnPropertyChanged("AlbumArtImageSource");
        //                });
        //            }
        //        }, this);

        //        return null;
        //    }
        //}

        //public void ResetAlbumArt()
        //{
        //    _albumArtImageSource = null;
        //    OnPropertyChanged("AlbumArtImageSource");
        //}

        public string AlbumArtUrl
        {
            get { return _albumArtUrl; }
            set
            {
                if (value == _albumArtUrl) return;
                _albumArtUrl = value;
                OnPropertyChanged();
            }
        }

        public string Artist
        {
            get { return _artist; }
            set
            {
                if (value == _artist) return;
                _artist = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string LastTitle
        {
            get { return _lastTitle; }
            set
            {
                if (value == _lastTitle) return;
                _lastTitle = value;
                OnPropertyChanged();
            }
        }


        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value.Equals(_isExpanded)) return;
                _isExpanded = value;
                OnPropertyChanged();
            }
        }


        public string NowPlayingText
        {
            get { return _nowPlayingText; }
            set
            {
                if (value == _nowPlayingText) return;
                _nowPlayingText = value;
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty RectWidthProperty = DependencyProperty.Register("RectWidth", typeof(double), typeof(BannerViewModel), new PropertyMetadata(0.0));
        private Uri _albumArtImageSource;
        private Visibility _playVisiblity;
        private Visibility _pauseVisibility;

        public double RectWidth
        {
            get { return (double)GetValue(RectWidthProperty); }
            set { SetValue(RectWidthProperty, value); }
        }


        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == _userName) return;
                _userName = value;
                OnPropertyChanged();
            }
        }

        public string CachedImageUri
        {
            get { return _cachedImageUri; }
            set
            {
                if (value == _cachedImageUri) return;
                _cachedImageUri = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                UiHelper.SafeDispatch(() => handler(this, new PropertyChangedEventArgs(propertyName)));
            }
        }
    }
}