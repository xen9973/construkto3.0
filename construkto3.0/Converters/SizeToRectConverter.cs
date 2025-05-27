using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace construkto3._0.Converters
{
    public class SizeToRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double width && values[1] is double height)
            {
                return new Rect(0, 0, width, height);
            }
            return new Rect(0, 0, 90, 90); // Значение по умолчанию
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}