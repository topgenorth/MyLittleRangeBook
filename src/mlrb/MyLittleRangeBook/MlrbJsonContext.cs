using System.Text.Json.Serialization;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEventAssets;

namespace MyLittleRangeBook
{
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
