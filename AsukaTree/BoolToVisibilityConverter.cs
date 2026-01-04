using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AsukaTree
{
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        // parameter="Invert" で反転も可能（必要なら）
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = value is bool bb && bb;

            if (parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase))
                b = !b;

            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
