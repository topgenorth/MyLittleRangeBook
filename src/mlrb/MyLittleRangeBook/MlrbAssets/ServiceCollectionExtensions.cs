using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Persistence.Sqlite;
using MyLittleRangeBook.RangeEventAssets;
using MyLittleRangeBook.RangeEventAssets.Handlers;
using static MyLittleRangeBook.RangeEventAssets.MlrbAssetAggregate;

namespace MyLittleRangeBook
{
    /// <summary>
    ///     Extension methods for registering range asset pipeline handlers with dependency injection.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        static readonly Type[] SupportedRangeAssetEvents = [
            typeof(MlrbAssetCreated),
            typeof(MlrbAssetImportStarted),
            typeof(MlrbAssetImportFailed),
            typeof(MlrbAssetImportCompleted),
            typeof(MlrbAssetAssociateWithRangeEvent),
            typeof(MlrbAssetFileCopied),
            typeof(MlrbAssetStoredInDatabase),
            typeof(MlrbAssetFingerprintComputed),
            typeof(MlrbAssetParsed),
            typeof(MlrbAssetUpdatedFromFile)
        ];

        public static IServiceCollection RegisterRangeAssetEventSourcing(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<IMlrbAssetAggregateRepository, MlrbAssetAggregateSqliteRepository>();
            services.AddScoped<IRangeAssetProjector, AssociateMlrbAssetToRangeEventProjector>();

            return services;
        }

        /// <summary>
        ///     Register the MlrbAssetFile pipeline and handlers in the dependency injection container.
        ///     This registers:
        ///     - <see cref="ValidateFileExistsHandler" /> for validating files exist on disk
        ///     - <see cref="CopyMlrbAssetToDataDirectory" /> for copying files to the range asset directory
        ///     - <see cref="LoggingHandler" /> for logging pipeline execution and results
        ///     - <see cref="IPipeline{MlrbAssetFile}" /> for executing the pipeline
        ///     Handler execution order:
        ///     1. ValidateFileExistsHandler - fails fast if file doesn't exist
        ///     2. CopyMlrbAssetToDataDirectory - copies file to range asset directory
        ///     3. LoggingHandler - logs all actions and results
        /// </summary>
        /// <param name="services">The service collection to register handlers with.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        ///     <code>
        /// services.RegisterRangeAssetHandlers();
        /// 
        /// var pipeline = serviceProvider.GetRequiredService&lt;IPipeline&lt;MlrbAssetFile&gt;&gt;();
        /// var result = await pipeline.ExecuteAsync(assetFile);
        /// </code>
        /// </example>
        public static IServiceCollection RegisterRangeAssetHandlers(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddScoped<ValidateFileExistsHandler>(serviceProvider =>
                new ValidateFileExistsHandler(serviceProvider.GetRequiredService<ILogger>()));

            services.AddScoped<CopyMlrbAssetToDataDirectory>(serviceProvider =>
                new CopyMlrbAssetToDataDirectory(serviceProvider.GetRequiredService<IConfiguration>()));

            services.AddScoped<LoggingHandler>(serviceProvider =>
                new LoggingHandler(serviceProvider.GetRequiredService<ILogger>()));

            services.AddScoped<InsertAssetFileSqliteHandler>(serviceProvider =>
                new InsertAssetFileSqliteHandler(serviceProvider.GetRequiredService<ISqliteHelper>()));


            // Register handlers by concrete type for the pipeline
            services.AddScoped<IPipelineHandler<MlrbAssetFile>>(sp =>
                sp.GetRequiredService<ValidateFileExistsHandler>());
            services.AddScoped<IPipelineHandler<MlrbAssetFile>>(sp =>
                sp.GetRequiredService<CopyMlrbAssetToDataDirectory>());
            services.AddScoped<IPipelineHandler<MlrbAssetFile>>(sp =>
                sp.GetRequiredService<InsertAssetFileSqliteHandler>());
            services.AddScoped<IPipelineHandler<MlrbAssetFile>>(sp =>
                sp.GetRequiredService<LoggingHandler>());

            // Register the pipeline with handlers in order
            services.AddScoped<IPipeline<MlrbAssetFile>>(serviceProvider =>
            {
                var pipeline = new Pipeline<MlrbAssetFile>(serviceProvider,
                    serviceProvider.GetRequiredService<ILogger>());

                return pipeline
                    .Add<ValidateFileExistsHandler>()
                    .Add<CopyMlrbAssetToDataDirectory>()
                    .Add<InsertAssetFileSqliteHandler>()
                    .Add<LoggingHandler>(); // [TO20260525] Keep the logging handler for last.
            });

            return services;
        }
    }
}
