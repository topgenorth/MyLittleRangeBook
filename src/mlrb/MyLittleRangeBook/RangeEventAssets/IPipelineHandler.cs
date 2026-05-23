namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     Defines a single handler in a pipeline that performs a discrete piece of work
    ///     and then delegates to the next handler in the chain.
    /// </summary>
    /// <typeparam name="TRecord">The type of record being processed.</typeparam>
    public interface IPipelineHandler<TRecord>
    {
        /// <summary>
        ///     Gets the name of this handler for logging and debugging purposes.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Execute this handler's work and then invoke the next handler in the pipeline.
        /// </summary>
        /// <param name="context">The pipeline context containing the record and metadata.</param>
        /// <param name="next">Delegate to invoke the next handler in the chain.</param>
        /// <returns>A Result indicating success or failure of the pipeline execution.</returns>
        Task<Result> ExecuteAsync(
            PipelineContext<TRecord> context,
            Func<PipelineContext<TRecord>, Task<Result>> next);
    }
}
