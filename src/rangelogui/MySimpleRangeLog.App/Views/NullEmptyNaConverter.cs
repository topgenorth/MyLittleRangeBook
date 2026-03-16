using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MySimpleRangeLog.Views
{
    public class NullEmptyNAConverter : IValueConverter
    {
        public static readonly NullEmptyNAConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return value is string s && !string.IsNullOrWhiteSpace(s) && s.Trim().ToUpperInvariant() != "N/A";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotSupportedException("One-way converter only.");
        }
    }
}
