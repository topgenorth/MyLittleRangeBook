using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     Represents an asset file specifically associated with a range event in the context of the application.
    ///     Provides functionality to define, identify, and process assets for range events, including copying the
    ///     asset file to the appropriate directory.
    /// </summary>
    public record RangeEventAssetFile
    {
        /// <summary>
        ///     This delegate is used to create the name of a file based asset that will be copied over to the range event
        ///     directory.
        /// </summary>
        public delegate string AssetFileNameResolver(string rangeEventTargetDir, string assetFileName);

        /// <summary>
        ///     This delegate is used to resolve and generate a unique identifier, in the form of an MlrbId,
        ///     for a file-based asset based on its file name.
        /// </summary>
        public delegate MlrbId AssetIdResolver(string assetFileName);

        /// <summary>
        ///     Path to the asset that is to be copied over to the range event asset directory.
        /// </summary>
        readonly string _pathToAsset;

        /// <summary>
        ///     The ID of the range event.
        /// </summary>
        readonly string _rangeEventId;

        string? _pathToRangeEventAsset;

        public RangeEventAssetFile(string rangeEventId, string pathToAsset)
        {
            _rangeEventId = rangeEventId;
            _pathToAsset = pathToAsset;
        }


        public string? PathToRangeEventAsset => _pathToRangeEventAsset;

        /// <summary>
        ///     Copies an asset file to the directory associated with a given range event, using a custom filename resolver.
        /// </summary>
        /// <returns>
        ///     A result containing the full path of the copied file if successful, or an error description if the operation fails.
        /// </returns>
        public Result<string> CopyToRangeEvent(AssetFileNameResolver assetNamer)
        {
            Result<string> result;
            try
            {
                _pathToRangeEventAsset = assetNamer(_rangeEventId, _pathToAsset);

                File.Copy(_pathToAsset, _pathToRangeEventAsset, true);

                result = Result.Ok(_pathToRangeEventAsset);
            }
            catch (Exception e)
            {
                Error? err = new Error(e.Message).CausedBy(e);

                result = Result.Fail(err);
            }

            return result;
        }
    }
}
