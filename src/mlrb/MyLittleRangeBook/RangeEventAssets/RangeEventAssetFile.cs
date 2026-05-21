using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     Represents an asset file specifically associated with a range event in the context of the application.
    ///     Provides functionality to define, identify, and process assets for range events, including copying the
    ///     asset file to the asset directory for a RangeEvent.
    /// </summary>
    public record RangeEventAssetFile
    {
        public RangeEventAssetFile(string pathToAsset, string? rangeEventId = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pathToAsset);
            PathToAsset = pathToAsset;
            RangeEventId = rangeEventId ?? MlrbId.Empty.ToString();
            Id = MlrbId.FromFile(new FileInfo(pathToAsset));
        }

        /// <summary>
        ///     A unique ID that will identify the range asset file
        /// </summary>
        public MlrbId Id { get; private set; }

        /// <summary>
        ///     Path to the asset that is to be copied over to the range event asset directory.
        /// </summary>
        public string PathToAsset { get; }

        /// <summary>
        ///     The ID of the range event.
        /// </summary>
        public string RangeEventId { get; private set; }

        /// <summary>
        ///     Constructs the full destination path for the asset file associated with a range event.
        ///     This includes combining the base directory for range assets, the specific range event directory,
        ///     and the file name of the asset.
        /// </summary>
        /// <param name="rangeAssetsDirectory">
        ///     The base directory path where range event assets are stored.
        /// </param>
        /// <param name="rangeEventAssetFile">
        ///     An instance of <see cref="RangeEventAssetFile" /> representing the asset file and its associated details.
        /// </param>
        /// <returns>
        ///     A string representing the full destination path for the specified range event asset file.
        /// </returns>
        public static string DefaultRangeEventAssetFileDestination(string rangeAssetsDirectory,
            RangeEventAssetFile rangeEventAssetFile)
        {
            string rangeEventAssetDir = Path.Combine(rangeAssetsDirectory, rangeEventAssetFile.RangeEventId);
            string assetFileName = Path.GetFileName(rangeEventAssetFile.PathToAsset);
            string rangeEventAssetFileName = Path.Combine(rangeEventAssetDir, assetFileName);

            return rangeEventAssetFileName;
        }


        public RangeEventAssetFile ForRangeEvent(string rangeEventId)
        {
            RangeEventId = rangeEventId;

            return this;
        }

        public override string ToString()
        {
            return PathToAsset;
        }
    }
}
