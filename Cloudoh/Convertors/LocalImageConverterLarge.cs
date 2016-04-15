using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Cloudoh.Convertors
{

    public class LocalImageConverterLarge : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null) 
                return null;

            if (DesignerProperties.IsInDesignTool)
                return value;

            string stringUri;

            var uri = value as Uri;
            if (uri != null)
                stringUri = uri.ToString();
            else if (value is string)
                stringUri = (string)value;
            else
                return null;

            if (stringUri.StartsWith("http"))
                return value;

            if (stringUri.EndsWith("/"))
                return value;

            try
            {
                var newImage = new BitmapImage()
                                   {
                                       CreateOptions = BitmapCreateOptions.BackgroundCreation
                                   };

                using (var myStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var stream = myStore.OpenFile(stringUri, FileMode.Open))
                    {
                        newImage.SetSource(stream);
                    }
                }

                return newImage;

            }
            catch (Exception)
            {
                return value;
            }

        }

        //
        // This was for NOKIA IMAGING
        //
        //private void BlurImage(IsolatedStorageFileStream stream)
        //{
        //    // Create a source to read the image from PhotoResult stream
        //    using (var source = new StreamImageSource(stream))
        //    {
        //        // Create effect collection with the source stream
        //        using (var filters = new FilterEffect(source))
        //        {
        //            // Initialize the filter 
        //            var sampleFilter = new BlurFilter(1);

        //            // Add the filter to the FilterEffect collection
        //            filters.Filters = new IFilter[] { sampleFilter };

        //            // Create a target where the filtered image will be rendered to
        //            var target = new WriteableBitmap(300, 300);

        //            // Create a new renderer which outputs WriteableBitmaps
        //            using (var renderer = new WriteableBitmapRenderer(filters, target))
        //            {
        //                EventWaitHandle wait = new AutoResetEvent(false);

        //                // Render the image with the filter(s)
        //                Task.Run(async () =>
        //                {
        //                    await renderer.RenderAsync();
        //                    wait.Set();
        //                });

        //                wait.WaitOne();

        //                using (var myStore = IsolatedStorageFile.GetUserStoreForApplication())
        //                {
        //                    using (var newStream = myStore.OpenFile("dummy_blur.jpg", FileMode.Create, FileAccess.Write))
        //                    {
        //                        target.SaveJpeg(newStream, 300, 300, 0, 100);
        //                    }
        //                }
        //            }

        //        }
        //    }
        //}

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }


        
    }
}