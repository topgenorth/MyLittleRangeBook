using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.IO;

namespace MyLittleRangeBook.RangeEventAssets.Handlers
{
    /// <summary>
    ///     Handler that copies a MlrbAssetFile to the range asset directory.
    /// </summary>
    public class CopyMlrbAssetToDataDirectory : IPipelineHandler<MlrbAssetFile>
    {
        /// <summary>
        ///     The folder name used to store range event assets, with platform-specific naming
        ///     conventions. On Windows, it is "RangeEventAssets", and on non-Windows platforms,
        ///     it is "range-event-assets".
        /// </summary>
        public static readonly string RangeEventAssetsFolderName =
            OperatingSystem.IsWindows() ? "RangeEventAssets" : "range-event-assets";

        /// <summary>
        /// The folder name used to store assets specific to the "My Little Range Book" application,
        /// adhering to platform-specific naming conventions. On Windows, it is "MlrbAssets", and
        /// on non-Windows platforms, it is "mlrb-assets".
        /// </summary>
        public static readonly string MlrbAssetsFolderName = OperatingSystem.IsWindows() ? "MlrbAssets" : "mlrb-assets";

        /// <summary>
        /// A delegate that will resolve the file name for an MlrbAssetFile.
        /// </summary>
        readonly AssetFileNameResolver _assetNamer;

        readonly string _dataDirectory;

        /// <summary>
        ///     Initializes a new instance of the CopyMlrbAssetToDataDirectory using configuration.
        /// </summary>
        /// <param name="config">Configuration containing range asset directory path.</param>
        public CopyMlrbAssetToDataDirectory(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _dataDirectory = config.GetSqliteDatabaseDirectory();

            // TODO [TO20260602] It would be nice to include with ExecuteAsync or perhaps the PipelineContext somehow.
            _assetNamer = GetMlrbAssetFileNameForDataDirectory;
        }

        public string Name => "Copy File to Range Asset";

        public async Task<Result> ExecuteAsync(
            PipelineContext<MlrbAssetFile> context,
            Func<PipelineContext<MlrbAssetFile>, Task<Result>> next)
        {
            try
            {
                string destinationPath = _assetNamer(_dataDirectory, context.Record);
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

        /// <summary>
        /// This will copy the asset file a directory
        /// </summary>
        /// <param name="dataDirectory">This should be the same directory as the SQLite database.</param>
        /// <param name="mlrbAssetFile"></param>
        /// <returns></returns>
        public static string GetMlrbAssetFileNameForDataDirectory(string dataDirectory, MlrbAssetFile mlrbAssetFile)
        {
            string dir = Path.Combine(dataDirectory, mlrbAssetFile.Id);
            Directory.CreateDirectory(dir);
            // [TO20260521] Handle both Windows and Linux separators.
            string filename = Path.GetFileName(mlrbAssetFile.PathToAsset.Replace('\\', '/'));
            string mlrbAssetFileName = Path.Combine(dir, filename);
            return mlrbAssetFileName;
        }
    }
}
