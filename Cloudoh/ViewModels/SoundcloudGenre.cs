using System;
using Cloudoh.Annotations;

namespace Cloudoh.ViewModels
{
    public class SoundcloudGenre
    {

        public Uri ImageSource { get; private set; }
        public string Title { get; private set; }
        public string Genre { get; private set; }

        public SoundcloudGenre(string title, string genre, Uri imageSource)
        {
            ImageSource = imageSource;
            Genre = genre;
            Title = title;
        }
    }
}
