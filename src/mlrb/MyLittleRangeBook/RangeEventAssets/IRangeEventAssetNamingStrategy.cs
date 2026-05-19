using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     Interface for naming strategies that generate filenames for range event assets.
    /// </summary>
    public interface IRangeEventAssetNamingStrategy
    {
        /// <summary>
        ///     Generates a complete asset file name based on the specified range event ID
        ///     and the provided asset file name.
        /// </summary>
        /// <param name="rangeEventId">The unique identifier of the range event.</param>
        /// <param name="assetFileName">The base file name of the asset.</param>
        /// <returns>A string representing the complete asset file name.</returns>
        Result<(MlrbId assetId, string assetPath)> GenerateAssetFileName(string rangeEventId, string assetFileName);
    }
}
