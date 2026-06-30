using System.Text.Json.Serialization;
using MyLittleRangeBook.Models;
using static MyLittleRangeBook.Firearms.FirearmAggregate;
using static MyLittleRangeBook.MlrbAssets.MlrbAssetAggregate;

namespace MyLittleRangeBook.Persistence
{
    /// <summary>
    /// Provides a JSON serialization context for the MyLittleRangeBook domain.
    /// </summary>
    /// <remarks>
    /// This context is primarily used to configure JSON serialization and deserialization settings
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

    #region Domain Events: MrlbAsset
    // [TO20260609] This list should match the SupportedRangeAssetsEvents list in MlrbAssets/ServiceCollectionExtensions.cs
    [JsonSerializable(typeof(MrlbAssetAssociatedWithFirearm))]
    [JsonSerializable(typeof(MlrbAssetAssociatedWithSimpleRangeEvent))]
    [JsonSerializable(typeof(MlrbAssetCreated))]
    [JsonSerializable(typeof(MlrbAssetImportStarted))]
    [JsonSerializable(typeof(MlrbAssetFileCopied))]
    [JsonSerializable(typeof(MlrbAssetStoredInDatabase))]
    [JsonSerializable(typeof(MlrbAssetParsed))]
    [JsonSerializable(typeof(MlrbAssetFingerprintComputed))]
    [JsonSerializable(typeof(MlrbAssetImportCompleted))]
    [JsonSerializable(typeof(MlrbAssetImportFailed))]
    [JsonSerializable(typeof(MlrbAssetUpdatedFromFile))]
    #endregion

    #region Domain Events: Firearms
    // [TO20260609] This list should match the SupportedFirearmEvents list in  Firearms/ServiceCollectionExtensions.cs
    [JsonSerializable(typeof(FirearmActive))]
    [JsonSerializable(typeof(FirearmAssociatedWithAsset))]
    [JsonSerializable(typeof(FirearmAssociatedWithRangeEvent))]
    [JsonSerializable(typeof(FirearmBarrelChanged))]
    [JsonSerializable(typeof(FirearmCleaned))]
    [JsonSerializable(typeof(FirearmCreated))]
    [JsonSerializable(typeof(FirearmRoundCountAltered))]
    [JsonSerializable(typeof(FirearmInactive))]
    [JsonSerializable(typeof(FirearmModified))]
    [JsonSerializable(typeof(FirearmNoteAdded))]
    [JsonSerializable(typeof(FirearmSightingSystemChanged))]
    #endregion


    [JsonSerializable(typeof(MlrbId))]
    public partial class MlrbJsonSerializerContext : JsonSerializerContext
    {
        // TODO [TO20260531] Need to consolidate this with SystemTextJsonEventSerializer.
    }
}
