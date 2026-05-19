using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     Strategy for naming assets in the range event directory based on the file name of the asset.
    /// </summary>
    public abstract class FileNameStrategyBase
    {
        /// <summary>
        ///     Represents a unique identifier for an asset in the context of range event asset management.
        /// </summary>
        /// <remarks>
        ///     This identifier is based on the <see cref="MlrbId" /> type, which provides a time-sortable unique value.
        ///     It is primarily used within file naming strategies to generate consistent and collision-resistant file names
        ///     for range event assets.
        /// </remarks>
        protected MlrbId AssetId = MlrbId.Empty;

        /// <summary>
        ///     Represents the directory path where range event assets are stored.
        /// </summary>
        /// <remarks>
        ///     This property is primarily used as a base directory for organizing and storing asset files associated with range
        ///     events.
        ///     It serves as a root path where individual range event subdirectories are created, ensuring a structured and
        ///     consistent directory layout
        ///     for managing assets.
        /// </remarks>
        protected string? RangeAssetsDirectory;

        /// <summary>
        ///     Generates a file name for an asset within a specified range event directory and creates the necessary directory if
        ///     it does not exist.
        /// </summary>
        /// <param name="rangeEventId">The identifier for the range event associated with the asset.</param>
        /// <param name="assetFileName">The original name of the asset file.</param>
        /// <returns>
        ///     A result containing a tuple with the unique asset identifier and the generated asset file path, or an error if
        ///     the operation fails.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if any of the provided parameters are null, empty, or consist only of
        ///     white-space characters.
        /// </exception>
        public Result<(MlrbId assetId, string assetPath)> GenerateAssetFileName(string rangeEventId,
            string assetFileName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(RangeAssetsDirectory);
            ArgumentException.ThrowIfNullOrWhiteSpace(assetFileName);
            ArgumentException.ThrowIfNullOrWhiteSpace(rangeEventId);
            string targetDir;

            try
            {
                targetDir = Path.Combine(RangeAssetsDirectory, rangeEventId);
                Directory.CreateDirectory(targetDir);
            }
            catch (Exception e)
            {
                Error? error = new Error("Failed to create asset directory for range event").CausedBy(e);

                return Result.Fail(error);
            }

            AssetId = new MlrbId();
            string assetPath = MakeAssetFileNameForRangeEvent(targetDir, assetFileName);

            return Result.Ok((AssetId, assetPath));
        }


        /// <summary>
        ///     Generates a suitable file name for an asset within a specific range event directory.
        /// </summary>
        /// <param name="rangeEventTargetDir">The target directory for the range event assets.</param>
        /// <param name="assetFileName">The original file name of the asset.</param>
        /// <returns>The full path of the generated asset file within the target directory.</returns>
        protected abstract string MakeAssetFileNameForRangeEvent(string rangeEventTargetDir, string assetFileName);
    }
}
