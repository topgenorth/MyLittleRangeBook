using System.Text.Json.Serialization;
using MyLittleRangeBook.Models;
using static MyLittleRangeBook.RangeEventAssets.RangeAssetAggregate;

namespace MyLittleRangeBook
{
    /// <summary>
    /// Provides a JSON serialization context for the MyLittleRangeBook domain.
    /// </summary>
    /// <remarks>
    /// This context is primarily utilized to configure JSON serialization and deserialization settings
    /// for types used within the MyLittleRangeBook application. It applies options such as camelCase naming conventions,
    /// ignoring null values, and case-insensitivity for property names. Additionally, it includes specific type metadata
    /// for serializing domain events and identifiers used in range asset management.
    /// </remarks>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(RangeAssetCreated))]
    [JsonSerializable(typeof(RangeAssetImportStarted))]
    [JsonSerializable(typeof(RangeAssetCopied))]
    [JsonSerializable(typeof(RangeAssetStoredInDatabase))]
    [JsonSerializable(typeof(RangeAssetParsed))]
    [JsonSerializable(typeof(RangeAssetFingerprintComputed))]
    [JsonSerializable(typeof(RangeAssetImportCompleted))]
    [JsonSerializable(typeof(RangeAssetImportFailed))]
    [JsonSerializable(typeof(RangeAssetAssociateWithRangeEvent))]
    [JsonSerializable(typeof(MlrbId))]
    public partial class MlrbJsonSerializerContext : JsonSerializerContext
    {
        // TODO [TO20260531] Need to consolidate this with SystemTextJsonSerializer.
    }
}
