using System.Text.Json.Serialization;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEventAssets;

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
    [JsonSerializable(typeof(RangeAssetAggregate.RangeAssetCreated))]
    [JsonSerializable(typeof(RangeAssetAggregate.RangeAssetImportStarted))]
    [JsonSerializable(typeof(RangeAssetAggregate.RangeAssetCopied))]
    [JsonSerializable(typeof(RangeAssetAggregate.RangeAssetStoredInDatabase))]
    [JsonSerializable(typeof(RangeAssetAggregate.RangeAssetParsed))]
    [JsonSerializable(typeof(RangeAssetAggregate.RangeAssetFingerprintComputed))]
    [JsonSerializable(typeof(RangeAssetAggregate.RangeAssetImportCompleted))]
    [JsonSerializable(typeof(RangeAssetAggregate.RangeAssetImportFailed))]
    [JsonSerializable(typeof(RangeAssetAggregate.RangeAssetAssociateWithRangeEvent))]
    [JsonSerializable(typeof(MlrbId))]
    partial class MlrbJsonContext : JsonSerializerContext
    {
    }
}
