using System;
using System.Globalization;
using System.Windows.Data;

namespace AnalyzerStudio.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    class TextToDoubleConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToString(value, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value, culture);
        }
    }
}
