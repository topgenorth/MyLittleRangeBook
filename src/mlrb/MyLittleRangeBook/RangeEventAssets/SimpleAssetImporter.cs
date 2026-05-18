using FluentResults;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     This class will copy file for a given range event over to the data directory for the application.
    /// </summary>
    public class SimpleAssetImporter : IRangeEventAssetImporter
    {
        public static readonly string RangeAssetsFolderName =
            OperatingSystem.IsWindows() ? "RangeEventAssets" : "range-event-assets";

        readonly IRangeEventAssetNamingStrategy _assetNamer;

        /// <summary>
        ///     Represents a component that copies files associated with range events
        ///     into the application's designated data directory. Provides functionality for
        ///     generating filenames and managing file imports.
        /// </summary>
        public SimpleAssetImporter(string assetsDirectory, FileNameStrategyBase assetNamer)
        {
            RangeAssetsDirectory = assetsDirectory;

            _assetNamer = assetNamer.In(RangeAssetsDirectory);
        }

        /// <summary>
        ///     The directory that holds all RangeAssets for MLRB.
        /// </summary>
        public string RangeAssetsDirectory { get; init; }


        /// <summary>
        ///     Will copy the asset (file) to the application's directory. The file will be given an unique filename that
        ///     contain the ID of the range event.
        /// </summary>
        /// <param name="rangeEventId">A unique ID that identifies the range event..</param>
        /// <param name="assetToImport">The full path to a file.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task<Result<(MlrbId assetId, string destinationPath)>> ImportAssetForRangeEvent(
            string rangeEventId,
            string assetToImport,
            CancellationToken ct = default)
        {
            try
            {
                string rangeEventAssetDir = Path.Combine(RangeAssetsDirectory, rangeEventId);
                Directory.CreateDirectory(RangeAssetsDirectory);
                Directory.CreateDirectory(rangeEventAssetDir);
            }
            catch (Exception ex)
            {
                Error e = new Error(ex.Message).CausedBy(ex).Enrich(rangeEventId, null);
                e.Metadata.Add("asset_path", assetToImport);

                return Result.Fail(e.Message);
            }

            Result<(MlrbId id, string pathToAsset)> fileName =
                _assetNamer.GenerateAssetFileName(rangeEventId, assetToImport);

            if (fileName.IsFailed)
            {
                Error err = new Error("Could not generate the name of the asset.")
                    .Enrich(rangeEventId);
                err.Metadata.Add("asset_name", assetToImport);

                return Result.Fail(err);
            }

            try
            {
                File.Copy(assetToImport, fileName.Value.pathToAsset, true);
            }
            catch (Exception ex)
            {
                Error? e = new Error(ex.Message).CausedBy(ex).Enrich(rangeEventId, null);
                e.Metadata.Add("asset_path", assetToImport);
                e.Metadata.Add("asset_destination", fileName.Value.pathToAsset);

                return Result.Fail(e);
            }

            return Result.Ok(fileName.Value);
        }
    }
}
