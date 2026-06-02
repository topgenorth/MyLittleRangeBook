using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.IO;

namespace MyLittleRangeBook.RangeEventAssets.Handlers
{
    /// <summary>
    ///     Handler that copies a MlrbAssetFile to the MLRB asset directory.
    /// </summary>
    public class CopyMlrbAssetHandler : IPipelineHandler<MlrbAssetFile>
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
        public static readonly string MlrbAssetsFolderName = OperatingSystem.IsWindows() ? "Assets" : "assets";

        /// <summary>
        /// A delegate that will resolve the file name for an MlrbAssetFile.
        /// </summary>
        readonly AssetFileNameResolver _assetNamer;

        readonly string _dataDirectory;

        /// <summary>
        ///     Initializes a new instance of the CopyMlrbAssetHandler using configuration.
        /// </summary>
        /// <param name="config">Configuration containing range asset directory path.</param>
        public CopyMlrbAssetHandler(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _dataDirectory = config.GetSqliteDatabaseDirectory();

            // TODO [TO20260602] It would be nice to include with ExecuteAsync or perhaps the PipelineContext somehow.
            _assetNamer = GetMlrbAssetFileNameForDataDirectory;
        }

        public string Name => "Copy file to asset folder";

        public async Task<Result> ExecuteAsync(PipelineContext<MlrbAssetFile> context,
            Func<PipelineContext<MlrbAssetFile>, Task<Result>> next)
        {
            try
            {
                string sourcePath = context.Record.FileToImport;
                Result<ReadOnlyMemory<byte>> fileContents = await sourcePath
                    .LoadFileBytesAsync(context.CancellationToken)
                    .ConfigureAwait(false);

                string destinationPath = _assetNamer(_dataDirectory, context.Record);
                await File.WriteAllBytesAsync(destinationPath, fileContents.Value.ToArray(), context.CancellationToken)
                    .ConfigureAwait(false);

                context.Record.Aggregate.Copied(destinationPath, fileContents.Value.ToArray(), DateTimeOffset.UtcNow);

                // Store the destination path in metadata for downstream handlers
                context.Metadata["DestinationPath"] = destinationPath;
                context.Metadata["CopySuccess"] = true;
                return await next(context);
            }
            catch (Exception ex)
            {
                context.Metadata["CopySuccess"] = false;
                context.Metadata["CopyError"] = ex.Message;
                context.Record.Aggregate.Fail(ex, DateTimeOffset.UtcNow);

                return Result.Fail(new Error($"Failed to copy file '{context.Record.FileToImport}': {ex.Message}")
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
            string dir = Path.Combine(dataDirectory, MlrbAssetsFolderName);
            Directory.CreateDirectory(dir);

            // [TO20260521] Prefix the original filename with the asset Id; handle both Windows and Linux separators.
            string filename = mlrbAssetFile.Id + "-" + Path.GetFileName(mlrbAssetFile.FileToImport.Replace('\\', '/'));

            string mlrbAssetFileName = Path.Combine(dir, filename);
            return mlrbAssetFileName;
        }
    }
}
