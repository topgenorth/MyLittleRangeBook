namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    /// Defines a pipeline that processes records through a series of handlers.
    /// </summary>
    /// <typeparam name="TRecord">The type of record being processed.</typeparam>
    public interface IPipeline<TRecord>
    {
        /// <summary>
        /// Add a handler to the pipeline.
        /// </summary>
        /// <typeparam name="THandler">The handler type to add.</typeparam>
        /// <returns>The pipeline for fluent chaining.</returns>
        IPipeline<TRecord> Add<THandler>() where THandler : IPipelineHandler<TRecord>;

        /// <summary>
        /// Execute the pipeline with the given record.
        /// </summary>
        /// <param name="record">The record to process through the pipeline.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A Result indicating success or failure of the overall pipeline execution.</returns>
        Task<Result> ExecuteAsync(TRecord record, CancellationToken cancellationToken = default);
    }
}

