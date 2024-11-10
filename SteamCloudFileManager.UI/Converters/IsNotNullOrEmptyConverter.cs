using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SteamCloudFileManager.UI.Converters;

public class IsNotNullOrEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string stringValue && !string.IsNullOrEmpty(stringValue);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => throw new NotSupportedException();
}