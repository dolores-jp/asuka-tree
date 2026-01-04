using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AsukaTree
{
    public sealed class TypeToIconConverter : IValueConverter
    {
        // ここは「用意できたtypeだけ」追加していく
        private static readonly Dictionary<string, string> Map = new()
        {
            ["type:0"] = "Assets/0.png",
            ["type:1"] = "Assets/1.png",
            ["type:2"] = "Assets/2.png",
            ["type:3"] = "Assets/3.png",
            ["type:4"] = "Assets/4.png",
            ["type:5"] = "Assets/5.png",
            ["type:6"] = "Assets/6.png",
            ["type:7"] = "Assets/7.png",
            ["type:9"] = "Assets/9.png",
            ["type:10"] = "Assets/10.png",
            ["type:11"] = "Assets/11.png",
            ["type:12"] = "Assets/12.png",
            ["type:14"] = "Assets/14.png",
            ["type:4099"] = "Assets/4099.png",
        };

        private const string FallbackPath = "Assets/999.png";
        private static readonly ConcurrentDictionary<string, BitmapImage> Cache = new();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = value as string ?? "";
            var path = Map.TryGetValue(key, out var p) ? p : FallbackPath;

            return GetImageOrFallback(path);
        }

        private static BitmapImage GetImageOrFallback(string relativePath)
        {
            try
            {
                return Cache.GetOrAdd(relativePath, p =>
                {
                    var uri = new Uri($"pack://application:,,,/{p}", UriKind.Absolute);
                    return new BitmapImage(uri);
                });
            }
            catch
            {
                return Cache.GetOrAdd(FallbackPath, p =>
                {
                    var uri = new Uri($"pack://application:,,,/{p}", UriKind.Absolute);
                    return new BitmapImage(uri);
                });
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
