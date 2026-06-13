using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace MyLittleRangeBook.Persistence
{
    /// <summary>
    /// Custom JSON converter for Avalonia Color objects.
    /// Handles serialization and deserialization of Color values as strings.
    /// Converts between Color objects and their string representations for JSON storage.
    /// </summary>
    public class JsonColorConverter : JsonConverter<Color>
    {
        /// <summary>
        /// Deserializes a string value from JSON into a Color object.
        /// Returns the default color if parsing fails, ensuring robust error handling.
        /// </summary>
        /// <param name="reader">The JSON reader to read from</param>
        /// <param name="typeToConvert">The type being converted (Color)</param>
        /// <param name="options">Serialization options</param>
        /// <returns>The deserialized Color object or default if parsing fails</returns>
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Color.TryParse(reader.GetString(), out Color value) ? value : default;
        }

        /// <summary>
        /// Serializes a Color object into a string for JSON output.
        /// Uses Color's built-in ToString() method for consistent formatting.
        /// </summary>
        /// <param name="writer">The JSON writer to write to</param>
        /// <param name="value">The Color object to serialize</param>
        /// <param name="options">Serialization options</param>
        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}