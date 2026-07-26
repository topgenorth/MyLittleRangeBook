using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyLittleRangeBook.Models
{
    /// <summary>
    ///     A JSON converter for serializing and deserializing <see cref="MlrbId" /> instances.
    /// </summary>
    public class MlrbIdJsonConverter : JsonConverter<MlrbId>
    {
        public override MlrbId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string ulidString = reader.GetString() ?? throw new JsonException("Expected a string value for MlrbId.");

            return MlrbId.FromString(ulidString);
        }

        public override void Write(Utf8JsonWriter writer, MlrbId value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
    }
}