using FluentResults;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using NanoidDotNet;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     This class will copy file for a given range event over to the data directory for the application.
    /// </summary>
    public class SimpleAssetImporter : IRangeEventAssetImporter
    {
        public static readonly string RangeAssetsFolderName =
            OperatingSystem.IsWindows() ? "RangeEventAssets" : "range-event-assets";


        /// <summary>
        /// </summary>
        /// <param name="dataDirectory">The data directory where range event assets will be stored. Must already exist.</param>
        public SimpleAssetImporter(string dataDirectory)
        {
            RangeAssetsDirectory = Path.Combine(dataDirectory, RangeAssetsFolderName);
        }

        public string RangeAssetsDirectory { get; init; }

        /// <summary>
        ///     Will copy the asset (file) to the application's directory. The file will be given an unique filename that
        ///     contain the ID of the range event.
        /// </summary>
        /// <param name="assetToImport">The full path to a file.</param>
        /// <param name="rangeEventId">The nanoid of the range event.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Result<(string assetId, string destinationPath)>> ImportAssetForRangeEvent(
            string assetToImport,
            string rangeEventId,
            CancellationToken ct = default)
        {
            try
            {
                string eventHistoryDir = Path.Combine(RangeAssetsFolderName, rangeEventId);
                Directory.CreateDirectory(eventHistoryDir);
            }
            catch (Exception ex)
            {
                Error e = new Error(ex.Message).CausedBy(ex).Enrich(rangeEventId, null);
                e.Metadata.Add("asset_path", assetToImport);

                return Result.Fail(e.Message);
            }

            Result<(string id, string pathToAsset)> fileName = await GenerateAssetFileName(rangeEventId, assetToImport);

            if (fileName.IsFailed)
            {
                return fileName;
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

        /// <summary>
        ///     Generates the name of the asset file that MLRB will use. Will create the directory if necessary.
        /// </summary>
        /// <param name="rangeEventId"></param>
        /// <param name="pathToAsset"></param>
        /// <returns></returns>
        async Task<Result<(string id, string pathToAsset)>> GenerateAssetFileName(string rangeEventId,
            string pathToAsset)
        {
            string assetDirectory;
            try
            {
                assetDirectory = Path.Combine(RangeAssetsDirectory, rangeEventId);
                Directory.CreateDirectory(assetDirectory);
            }
            catch (Exception e)
            {
                Error? error = new Error("Failed to create asset directory for range event").CausedBy(e);

                return Result.Fail(error);
            }

            string? assetId = await Nanoid.GenerateAsync().ConfigureAwait(false);
            string extension = Path.GetExtension(pathToAsset);
            var newFileName = $"{assetId}{extension}";
            string assetPath = Path.Combine(assetDirectory, newFileName);


            return Result.Ok((_assetId: assetId, assetPath));
        }
    }
}
