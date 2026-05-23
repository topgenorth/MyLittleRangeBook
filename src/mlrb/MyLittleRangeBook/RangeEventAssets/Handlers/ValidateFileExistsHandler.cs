namespace MyLittleRangeBook.RangeEventAssets.Handlers
{
    /// <summary>
    ///     Handler that validates that a RangeEventAssetFile exists on disk.
    ///     This handler should typically be placed early in the pipeline to fail fast if the file is missing.
    /// </summary>
    public class ValidateFileExistsHandler : IPipelineHandler<RangeEventAssetFile>
    {
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

                return Result.Fail(errorMessage);
            }

            try
            {
                // Store validation result in metadata
                var fileInfo = new FileInfo(filePath);
                context.Metadata["FileExists"] = true;
                context.Metadata["FileSizeBytes"] = fileInfo.Length;
                context.Metadata["FileLastModified"] = fileInfo.LastWriteTimeUtc;

                _logger.Information("File validation passed: {FilePath} ({FileSize} bytes)",
                    filePath,
                    fileInfo.Length);

                // Call next handler
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
