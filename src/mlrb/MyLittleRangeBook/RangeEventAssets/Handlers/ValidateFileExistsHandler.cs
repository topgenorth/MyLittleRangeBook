using MyLittleRangeBook.IO;

namespace MyLittleRangeBook.RangeEventAssets.Handlers
{
    /// <summary>
    ///     Handler that validates that a RangeEventAssetFile exists on disk.
    ///     This handler should typically be placed early in the pipeline to fail fast if the file is missing.
    /// </summary>
    public class ValidateFileExistsHandler : IPipelineHandler<RangeEventAssetFile>
    {
        /// <summary>
        /// Defines the maximum file size, in bytes, that a SQLite file can have to be considered valid.
        /// This constant is used during validation to ensure the file size does not exceed predefined limits.
        /// </summary>
        internal const int MaxFileSizeForSqlite = 90 * 1024;
        readonly ILogger _logger;

        /// <summary>
        ///     Initializes a new instance of the ValidateFileExistsHandler.
        /// </summary>
        /// <param name="logger">Logger instance for recording validation details.</param>
        public ValidateFileExistsHandler(ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        public string Name => "Validate File Exists";

        public async Task<Result> ExecuteAsync(
            PipelineContext<RangeEventAssetFile> context,
            Func<PipelineContext<RangeEventAssetFile>, Task<Result>> next)
        {
            string filePath = context.Record.PathToAsset;

            if (!File.Exists(filePath))
            {
                var errorMessage = $"Asset file does not exist: '{filePath}'";
                _logger.Warning("Validation failed: {ErrorMessage}", errorMessage);
                context.Metadata["FileExists"] = false;
                context.Metadata["ValidationError"] = errorMessage;
                context.Record.Aggregate.Fail(errorMessage, DateTimeOffset.UtcNow);

                return Result.Fail(errorMessage);
            }

            try
            {
                // Store validation result in metadata
                var fileInfo = new FileInfo(filePath);
                string sha256 = await fileInfo.ComputeSha256HashAsync().ConfigureAwait(false);
                context.Metadata["FileExists"] = true;
                context.Metadata["FileSizeBytes"] = fileInfo.Length;
                context.Metadata["FileLastModified"] = fileInfo.LastWriteTimeUtc;
                context.Metadata["FileSha256"] = sha256;

                if (fileInfo.Length > MaxFileSizeForSqlite)
                {
                    var errorMessage = $"File size exceeds limit: '{filePath}' is {fileInfo.Length} bytes";
                    _logger.Warning("Validation failed: {ErrorMessage}", errorMessage);
                }

                _logger.Verbose("File validation passed: {FilePath} ({FileSize} bytes)",
                    filePath,
                    fileInfo.Length);

                context.Record.Aggregate.FileFingerprinted(sha256, fileInfo.Length, DateTimeOffset.UtcNow);

                return await next(context);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error validating file '{filePath}': {ex.Message}";
                _logger.Error(ex, "File validation error: {ErrorMessage}", errorMessage);
                context.Metadata["FileExists"] = false;
                context.Metadata["ValidationError"] = ex.Message;

                return Result.Fail(new Error(errorMessage).CausedBy(ex));
            }
        }
    }
}
