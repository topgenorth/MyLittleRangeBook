namespace MyLittleRangeBook.RangeEventAssets.Handlers
{
    /// <summary>
    /// Handler that logs the actions taken by the pipeline and their results.
    /// This handler should be placed last in the pipeline to capture all metadata from previous handlers.
    /// </summary>
    public class LoggingHandler : IPipelineHandler<RangeEventAssetFile>
    {
        public string Name => "Logging";

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the LoggingHandler.
        /// </summary>
        /// <param name="logger">Logger instance for recording pipeline execution details.</param>
        public LoggingHandler(ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        public async Task<Result> ExecuteAsync(
            PipelineContext<RangeEventAssetFile> context,
            Func<PipelineContext<RangeEventAssetFile>, Task<Result>> next)
        {
            // Call next handler (in case there are handlers after this one)
            var result = await next(context);

            // Log the overall result and metadata
            if (result.IsSuccess)
            {
                LogSuccessfulExecution(context);
            }
            else
            {
                LogFailedExecution(context, result);
            }

            return result;
        }

        private void LogSuccessfulExecution(PipelineContext<RangeEventAssetFile> context)
        {
            var logContext = new Dictionary<string, string>
            {
                ["AssetFile"] = context.Record.PathToAsset,
                ["RangeEventId"] = context.Record.RangeEventId,
                ["AssetId"] = context.Record.Id.ToString()
            };

            // Add handler-specific metadata
            if (context.Metadata.TryGetValue("DestinationPath", out var destPath))
            {
                logContext["DestinationPath"] = destPath?.ToString() ?? "unknown";
            }

            if (context.Metadata.TryGetValue("CopySuccess", out var copySuccess))
            {
                logContext["FileCopySuccessful"] = copySuccess?.ToString() ?? "unknown";
            }

            var logMessage = string.Join(" | ", logContext.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            _logger.Information("Pipeline completed successfully for RangeEventAssetFile: {LogDetails}", logMessage);
        }

        private void LogFailedExecution(PipelineContext<RangeEventAssetFile> context, Result result)
        {
            var logContext = new Dictionary<string, string>
            {
                ["AssetFile"] = context.Record.PathToAsset,
                ["RangeEventId"] = context.Record.RangeEventId,
                ["AssetId"] = context.Record.Id.ToString()
            };

            // Add handler-specific metadata and errors
            if (context.Metadata.TryGetValue("CopySuccess", out var copySuccess))
            {
                logContext["FileCopySuccessful"] = copySuccess?.ToString() ?? "unknown";
            }

            if (context.Metadata.TryGetValue("CopyError", out var copyError))
            {
                logContext["CopyErrorMessage"] = copyError?.ToString() ?? "unknown";
            }

            var errors = string.Join(" | ", result.Errors.Select(e => e.Message));
            var logMessage = string.Join(" | ", logContext.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            _logger.Error("Pipeline failed for RangeEventAssetFile: {LogDetails} | Errors: {Errors}",
                logMessage,
                errors);
        }
    }
}

