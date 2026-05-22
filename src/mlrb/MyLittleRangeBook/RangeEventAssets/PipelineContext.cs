namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    /// Encapsulates the context passed through a pipeline.
    /// Carries the record being processed, metadata for inter-handler communication,
    /// and cancellation support.
    /// </summary>
    public class PipelineContext<TRecord>
    {
        /// <summary>
        /// The record being processed through the pipeline.
        /// </summary>
        public TRecord Record { get; set; } = default!;

        /// <summary>
        /// Metadata dictionary for handlers to share state and information.
        /// </summary>
        public Dictionary<string, object> Metadata { get; } = [];

        /// <summary>
        /// Cancellation token for the pipeline execution.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
    }
}

