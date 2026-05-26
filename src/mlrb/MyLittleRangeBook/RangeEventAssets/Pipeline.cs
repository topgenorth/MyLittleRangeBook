using Microsoft.Extensions.DependencyInjection;

namespace MyLittleRangeBook
{
    /// <summary>
    ///     Encapsulates the context passed through a pipeline.
    ///     Carries the record being processed, metadata for inter-handler communication,
    ///     and cancellation support.
    /// </summary>
    public class PipelineContext<TRecord>
    {
        /// <summary>
        ///     The record being processed through the pipeline.
        /// </summary>
        public TRecord Record { get; set; } = default!;

        /// <summary>
        ///     Metadata dictionary for handlers to share state and information.
        /// </summary>
        public Dictionary<string, object> Metadata { get; } = [];

        /// <summary>
        ///     Cancellation token for the pipeline execution.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
    }

    /// <summary>
    ///     Defines a pipeline that processes records through a series of handlers.
    /// </summary>
    /// <typeparam name="TRecord">The type of record being processed.</typeparam>
    public interface IPipeline<TRecord>
    {
        /// <summary>
        ///     Add a handler to the pipeline.
        /// </summary>
        /// <typeparam name="THandler">The handler type to add.</typeparam>
        /// <returns>The pipeline for fluent chaining.</returns>
        IPipeline<TRecord> Add<THandler>() where THandler : IPipelineHandler<TRecord>;

        /// <summary>
        ///     Execute the pipeline with the given record.
        /// </summary>
        /// <param name="record">The record to process through the pipeline.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A Result indicating success or failure of the overall pipeline execution.</returns>
        Task<Result> ExecuteAsync(TRecord record, CancellationToken cancellationToken = default);
    }

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

    /// <summary>
    ///     Implementation of a pipeline that chains handlers together in a chain-of-responsibility pattern.
    ///     Each handler performs its work and then delegates to the next handler in the chain.
    /// </summary>
    /// <typeparam name="TRecord">The type of record being processed.</typeparam>
    public class Pipeline<TRecord> : IPipeline<TRecord>
    {
        readonly List<Type> _handlerTypes = [];
        readonly ILogger _logger;
        readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     Initializes a new instance of the Pipeline class.
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving handler instances.</param>
        /// <param name="logger">Logger for handler execution tracking.</param>
        public Pipeline(IServiceProvider serviceProvider, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(logger);

            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        ///     Add a handler to the pipeline.
        /// </summary>
        /// <typeparam name="THandler">The handler type to add.</typeparam>
        /// <returns>The pipeline for fluent chaining.</returns>
        public IPipeline<TRecord> Add<THandler>() where THandler : IPipelineHandler<TRecord>
        {
            _handlerTypes.Add(typeof(THandler));

            return this;
        }

        /// <summary>
        ///     Execute the pipeline with the given record.
        /// </summary>
        /// <param name="record">The record to process through the pipeline.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A Result indicating success or failure of the overall pipeline execution.</returns>
        public async Task<Result> ExecuteAsync(TRecord record, CancellationToken cancellationToken = default)
        {
            var context = new PipelineContext<TRecord> { Record = record, CancellationToken = cancellationToken };

            Func<PipelineContext<TRecord>, Task<Result>> handlerChain = BuildHandlerChain();

            return await handlerChain(context);
        }

        /// <summary>
        ///     Builds the handler chain by creating a series of delegates that invoke each handler
        ///     in the order they were added, with each handler responsible for calling the next.
        /// </summary>
        Func<PipelineContext<TRecord>, Task<Result>> BuildHandlerChain()
        {
            // Start with a terminal handler that returns success
            Func<PipelineContext<TRecord>, Task<Result>> chain =
                _ => Task.FromResult(Result.Ok());

            // Build the chain backward so handlers are invoked in the order they were added
            for (int i = _handlerTypes.Count - 1; i >= 0; i--)
            {
                Type handlerType = _handlerTypes[i];
                Func<PipelineContext<TRecord>, Task<Result>> nextHandler = chain;

                chain = async context =>
                {
                    var handler = (IPipelineHandler<TRecord>)_serviceProvider.GetRequiredService(handlerType);
                    _logger.Verbose("Executing handler: {HandlerName}", handler.Name);

                    try
                    {
                        Result result = await handler.ExecuteAsync(context, nextHandler);
                        if (result.IsFailed)
                        {
                            _logger.Error("Handler {HandlerName} failed: {Errors}",
                                handler.Name,
                                string.Join("; ", result.Errors.Select(e => e.Message)));
                        }

                        return result;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Handler {HandlerName} threw an exception", handler.Name);

                        return Result.Fail(new Error($"Handler {handler.Name} failed: {ex.Message}"));
                    }
                };
            }

            return chain;
        }
    }
}
