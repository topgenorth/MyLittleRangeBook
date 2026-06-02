using System.Text.Json.Serialization;
using MyLittleRangeBook.Models;
using static MyLittleRangeBook.Firearms.FirearmAggregate;
using static MyLittleRangeBook.RangeEventAssets.MlrbAssetAggregate;

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
    [JsonSerializable(typeof(MlrbAssetCreated))]
    [JsonSerializable(typeof(MlrbAssetImportStarted))]
    [JsonSerializable(typeof(MlrbAssetFileCopied))]
    [JsonSerializable(typeof(MlrbAssetStoredInDatabase))]
    [JsonSerializable(typeof(MlrbAssetParsed))]
    [JsonSerializable(typeof(MlrbAssetFingerprintComputed))]
    [JsonSerializable(typeof(MlrbAssetImportCompleted))]
    [JsonSerializable(typeof(MlrbAssetImportFailed))]
    [JsonSerializable(typeof(MlrbAssetAssociateWithRangeEvent))]
    [JsonSerializable(typeof(MlrbAssetUpdatedFromFile))]
    #endregion

    #region Domain Events: Firearms
    [JsonSerializable(typeof(AssetAssociatedWithFirearm))]
    [JsonSerializable(typeof(FirearmActive))]
    [JsonSerializable(typeof(FirearmBarrelChanged))]
    [JsonSerializable(typeof(FirearmCleaned))]
    [JsonSerializable(typeof(FirearmCreated))]
    [JsonSerializable(typeof(FirearmNoteAdded))]
    [JsonSerializable(typeof(FirearmSightingSystemChanged))]
    [JsonSerializable(typeof(FirearmDischargeMoreRounds))]
    [JsonSerializable(typeof(FirearmInactive))]
    [JsonSerializable(typeof(FirearmModified))]
    [JsonSerializable(typeof(FirearmNoteAdded))]
    [JsonSerializable(typeof(FirearmSightingSystemChanged))]
    [JsonSerializable(typeof(RangeEventAssociatedWithFirearm))]
    #endregion


    [JsonSerializable(typeof(MlrbId))]
    public partial class MlrbJsonSerializerContext : JsonSerializerContext
    {
        // TODO [TO20260531] Need to consolidate this with SystemTextJsonEventSerializer.
    }
}
