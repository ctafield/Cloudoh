using System;
using System.Windows;
using Cloudoh.Common;
using ProtoBuf;

namespace Cloudoh.ViewModels
{

    [ProtoContract]
    public class SoundcloudUserViewModel
    {
        [ProtoMember(1)]
        public long AccountId { get; set; }

        private string _fullName;

        [ProtoMember(2)]
        public string ScreenName { get; set; }

        [ProtoMember(3)]
        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(_fullName))
                    return _fullName;
                return ScreenName;
            }
            set { _fullName = value; }
        }

        [ProtoMember(4)]
        public int? Followers { get; set; }

        [ProtoMember(5)]
        public int? Following { get; set; }

        [ProtoMember(6)]
        public string WebSite { get; set; }

        public string WebSiteNice
        {
            get
            {
                if (string.IsNullOrEmpty(WebSite))
                    return string.Empty;
                var webSite = WebSite.Replace("http://", "").Replace("https://", "");
                webSite = webSite.Replace("www.", "");
                return webSite;
            }
        }

        public string BiographyHtml
        {
            get
            {
                return "<div>" + Biography + "</div>";
            }
        }

        [ProtoMember(7)]
        public string Biography { get; set; }

        [ProtoMember(8)]
        public string ProfileImageUrl { get; set; }

        public string ProfileImageLargeUrl
        {
            get
            {
                return ProfileImageUrl.Replace("-large", "-original");
            }
        }

        //public string ArtistImageBackground
        //{
        //    get
        //    {
        //        var nokiaMusic = new NokiaMusicConstants();
        //        return nokiaMusic.GetArtistImage(this.FullName);
        //    }
        //}

        public Uri ImageSource
        {
            get
            {
                return new Uri(ProfileImageUrl, UriKind.Absolute);
            }
        }

        // todo: this
        public Visibility IsProtectedVisibility { get { return Visibility.Collapsed; } }

        [ProtoMember(9)]
        public long Id { get; set; }

        public string Location
        {
            get
            {
                string location = City;

                if (!string.IsNullOrEmpty(Country))
                {
                    if (!string.IsNullOrEmpty(location))
                    {
                        location += ", ";
                    }
                    location += Country;
                }

                return location;
            }
        }

        [ProtoMember(10)]
        public string Country { get; set; }

        [ProtoMember(11)]
        public string City { get; set; }


        public Visibility LocationVisibility
        {
            get { return string.IsNullOrWhiteSpace(Location) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility WebsiteVisibility
        {
            get { return string.IsNullOrWhiteSpace(WebSite) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility AboutVisibility
        {
            get { return string.IsNullOrWhiteSpace(Biography) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public SoundcloudUserViewModel Clone()
        {
            return (SoundcloudUserViewModel)MemberwiseClone();
        }
    }

}
