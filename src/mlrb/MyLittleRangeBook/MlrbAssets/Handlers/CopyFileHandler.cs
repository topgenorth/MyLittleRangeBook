using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.IO;

namespace MyLittleRangeBook.RangeEventAssets.Handlers
{
    /// <summary>
    ///     Handler that copies a MlrbAssetFile to the range asset directory.
    /// </summary>
    public class CopyFileHandler : IPipelineHandler<MlrbAssetFile>
    {

        readonly AssetFileNameResolver _assetNamer;

        readonly string _rangeAssetsDirectory;

        /// <summary>
        ///     Initializes a new instance of the CopyFileHandler.
        /// </summary>
        /// <param name="rangeAssetsDirectory">The directory where range asset files will be copied.</param>
        /// <param name="assetNamer">Optional custom asset file name resolver. If null, uses default.</param>
        public CopyFileHandler(string rangeAssetsDirectory, AssetFileNameResolver? assetNamer = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(rangeAssetsDirectory);
            _rangeAssetsDirectory = rangeAssetsDirectory;
            _assetNamer = assetNamer ?? GetRangeEventAssetFilename;
        }

        /// <summary>
        ///     Initializes a new instance of the CopyFileHandler using configuration.
        /// </summary>
        /// <param name="config">Configuration containing range asset directory path.</param>
        public CopyFileHandler(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _rangeAssetsDirectory = config.GetRangeAssetDirectory();
            _assetNamer = GetRangeEventAssetFilename;
        }

        public string Name => "Copy File to Range Asset";


        public async Task<Result> ExecuteAsync(
            PipelineContext<MlrbAssetFile> context,
            Func<PipelineContext<MlrbAssetFile>, Task<Result>> next)
        {
            try
            {
                string destinationPath = _assetNamer(_rangeAssetsDirectory, context.Record);
                await FileExtensions.CopyFileAsync(context.Record.PathToAsset, destinationPath, context.CancellationToken).ConfigureAwait(false);

                // Store the destination path in metadata for downstream handlers
                context.Metadata["DestinationPath"] = destinationPath;
                context.Metadata["CopySuccess"] = true;

                context.Record.Aggregate.Copied(destinationPath, DateTimeOffset.UtcNow);

                return await next(context);
            }
            catch (Exception ex)
            {
                context.Metadata["CopySuccess"] = false;
                context.Metadata["CopyError"] = ex.Message;
                context.Record.Aggregate.Fail(ex, DateTimeOffset.UtcNow);

                return Result.Fail(new Error($"Failed to copy file '{context.Record.PathToAsset}': {ex.Message}")
                    .CausedBy(ex));
            }
        }

        public static string GetRangeEventAssetFilename(string rangeAssetDirectory,
            MlrbAssetFile mlrbAssetFile)
        {
            string rangeEventAssetDir = Path.Combine(rangeAssetDirectory, mlrbAssetFile.RangeEventId);
            Directory.CreateDirectory(rangeEventAssetDir);

            // [TO20260521] Handle both Windows and Linux separators.
            string filename = Path.GetFileName(mlrbAssetFile.PathToAsset.Replace('\\', '/'));

            string rangeEventAssetFilename = Path.Combine(rangeEventAssetDir, filename);

            return rangeEventAssetFilename;
        }
    }
}
