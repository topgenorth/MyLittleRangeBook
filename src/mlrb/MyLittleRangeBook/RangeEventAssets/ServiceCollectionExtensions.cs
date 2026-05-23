using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.RangeEventAssets.Handlers;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     Extension methods for registering range asset pipeline handlers with dependency injection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Register the RangeEventAssetFile pipeline and handlers in the dependency injection container.
        ///     This registers:
        ///     - <see cref="ValidateFileExistsHandler" /> for validating files exist on disk
        ///     - <see cref="CopyFileHandler" /> for copying files to the range asset directory
        ///     - <see cref="LoggingHandler" /> for logging pipeline execution and results
        ///     - <see cref="IPipeline{RangeEventAssetFile}" /> for executing the pipeline
        ///     Handler execution order:
        ///     1. ValidateFileExistsHandler - fails fast if file doesn't exist
        ///     2. CopyFileHandler - copies file to range asset directory
        ///     3. LoggingHandler - logs all actions and results
        /// </summary>
        /// <param name="services">The service collection to register handlers with.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        ///     <code>
        /// services.RegisterRangeAssetHandlers();
        /// 
        /// var pipeline = serviceProvider.GetRequiredService&lt;IPipeline&lt;RangeEventAssetFile&gt;&gt;();
        /// var result = await pipeline.ExecuteAsync(assetFile);
        /// </code>
        /// </example>
        public static IServiceCollection RegisterRangeAssetHandlers(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            // Register individual handlers with factories to resolve their dependencies
            services.AddScoped<ValidateFileExistsHandler>(serviceProvider =>
                new ValidateFileExistsHandler(serviceProvider.GetRequiredService<ILogger>()));

            services.AddScoped<CopyFileHandler>(serviceProvider =>
                new CopyFileHandler(serviceProvider.GetRequiredService<IConfiguration>()));

            services.AddScoped<LoggingHandler>(serviceProvider =>
                new LoggingHandler(serviceProvider.GetRequiredService<ILogger>()));

            // Register handlers by concrete type for the pipeline
            services.AddScoped<IPipelineHandler<RangeEventAssetFile>>(sp =>
                sp.GetRequiredService<ValidateFileExistsHandler>());
            services.AddScoped<IPipelineHandler<RangeEventAssetFile>>(sp => sp.GetRequiredService<CopyFileHandler>());
            services.AddScoped<IPipelineHandler<RangeEventAssetFile>>(sp => sp.GetRequiredService<LoggingHandler>());

            // Register the pipeline with handlers in order
            services.AddScoped<IPipeline<RangeEventAssetFile>>(serviceProvider =>
            {
                var pipeline = new Pipeline<RangeEventAssetFile>(serviceProvider,
                    serviceProvider.GetRequiredService<ILogger>());

                return pipeline
                    .Add<ValidateFileExistsHandler>()
                    .Add<CopyFileHandler>()
                    .Add<LoggingHandler>();
            });

            return services;
        }
    }
}
