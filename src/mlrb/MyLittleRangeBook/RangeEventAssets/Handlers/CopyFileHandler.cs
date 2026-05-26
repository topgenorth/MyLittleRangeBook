using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Config;

namespace MyLittleRangeBook.RangeEventAssets.Handlers
{
    /// <summary>
    ///     Handler that copies a RangeEventAssetFile to the range asset directory.
    /// </summary>
    public class CopyFileHandler : IPipelineHandler<RangeEventAssetFile>
    {
        const int BufferSize = 81920;
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
            PipelineContext<RangeEventAssetFile> context,
            Func<PipelineContext<RangeEventAssetFile>, Task<Result>> next)
        {
            try
            {
                // Copy the file
                await using var source = new FileStream(
                    context.Record.PathToAsset,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    BufferSize,
                    FileOptions.Asynchronous);

                string destinationPath = _assetNamer(_rangeAssetsDirectory, context.Record);
                await using var destination = new FileStream(
                    destinationPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    BufferSize,
                    FileOptions.Asynchronous);

                await source.CopyToAsync(destination, context.CancellationToken);

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
            RangeEventAssetFile rangeEventAssetFile)
        {
            string rangeEventAssetDir = Path.Combine(rangeAssetDirectory, rangeEventAssetFile.RangeEventId);
            Directory.CreateDirectory(rangeEventAssetDir);

            // [TO20260521] Handle both Windows and Linux separators.
            string filename = Path.GetFileName(rangeEventAssetFile.PathToAsset.Replace('\\', '/'));

            string rangeEventAssetFilename = Path.Combine(rangeEventAssetDir, filename);

            return rangeEventAssetFilename;
        }
    }
}
