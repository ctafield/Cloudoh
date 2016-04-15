using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Cloudoh
{
    public class RectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            double width = (double)value;

            return new Rect(0, 0, width, 50);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}