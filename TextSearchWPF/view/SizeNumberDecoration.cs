using System;
using System.Globalization;
using System.Windows.Data;

namespace com.hideakin.textsearch.view
{
    internal class SizeNumberDecoration : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return string.Format("{0:#,0}", intValue);
            }
            else if (value is int longValue)
            {
                return string.Format("{0:#,0}", longValue);
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
