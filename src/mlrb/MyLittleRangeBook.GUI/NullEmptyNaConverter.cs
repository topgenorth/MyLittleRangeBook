using System.Globalization;
using Avalonia.Data.Converters;

namespace MyLittleRangeBook.GUI
{
    /// <summary>
    ///     Converts null or empty values to "N/A" and vice versa for Avalonia.
    /// </summary>
    public class NullEmptyNaConverter : IValueConverter
    {
        public static readonly NullEmptyNaConverter Instance = new();

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
