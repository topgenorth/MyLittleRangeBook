using FluentResults;
using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Models;
using NanoidDotNet;
using Serilog;

namespace MyLittleRangeBook.Services
{
    public interface IImportRangeEventAsset
    {
        /// <summary>
        ///     Will copy the asset to the data directory for the app, and associate it with the specified range event.
        /// </summary>
        /// <param name="assetToImport"></param>
        /// <param name="rangeEventId"></param>
        /// <param name="ct"></param>
        /// <returns>A tuple that holds the ID of the new asset, and the path it was copied to.</returns>
        Task<Result<(string assetId, string destinationPath)>> ImportAssetForRangeEvent(string assetToImport,
            string rangeEventId, CancellationToken ct = default);
    }

    /// <summary>
    ///  This class will copy file for a given range event over to the data directory for the application.
    /// </summary>
    public class SimpleAssetImporter : IImportRangeEventAsset
    {
        public static readonly string RangeAssetsFolderName = OperatingSystem.IsWindows() ? "RangeEventAssets" : "range-event-assets";
        public string RangeAssetsDirectory { get; init; }



        /// <summary>
        ///
        /// </summary>
        /// <param name="dataDirectory">The data directory where range event assets will be stored. Must already exist.</param>
        public SimpleAssetImporter(string dataDirectory)
        {
            RangeAssetsDirectory = Path.Combine(dataDirectory, RangeAssetsFolderName);
        }

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
        ///     Generates the name of the asset file that will be used by MLRB. Will create the directory if necessary.
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
