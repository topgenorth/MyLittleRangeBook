using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.MlrbAssets;
using MyLittleRangeBook.MlrbAssets.Handlers;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.MlrbAssets.MlrbAssetAggregate;

namespace MyLittleRangeBook
{
    /// <summary>
    ///     Extension methods for registering range asset pipeline handlers with dependency injection.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        ///     This should match the JsonSerializeble attributes in MlrbJsonSerializerContext.cs
        /// </summary>
        static readonly Type[] SupportedRangeAssetEvents =
        [
            typeof(MlrbAssetCreated),
            typeof(MrlbAssetAssociatedWithFirearm),
            typeof(MlrbAssetAssociatedWithSimpleRangeEvent),
            typeof(MlrbAssetImportStarted),
            typeof(MlrbAssetImportFailed),
            typeof(MlrbAssetImportCompleted),
            typeof(MlrbAssetFileCopied),
            typeof(MlrbAssetStoredInDatabase),
            typeof(MlrbAssetFingerprintComputed),
            typeof(MlrbAssetParsed),
            typeof(MlrbAssetUpdatedFromFile),
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
        ///     - <see cref="CopyMlrbAssetHandler" /> for copying files to the range asset directory
        ///     - <see cref="LoggingHandler" /> for logging pipeline execution and results
        ///     - <see cref="IPipeline{MlrbAssetFile}" /> for executing the pipeline
        ///     Handler execution order:
        ///     1. ValidateFileExistsHandler - fails fast if file doesn't exist
        ///     2. CopyMlrbAssetHandler - copies file to range asset directory
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
                                                              new ValidateFileExistsHandler(serviceProvider
                                                                          .GetRequiredService<ILogger>()));

            services.AddScoped<CopyMlrbAssetHandler>(serviceProvider =>
                                                         new CopyMlrbAssetHandler(serviceProvider
                                                                     .GetRequiredService<IConfiguration>()));

            services.AddScoped<LoggingHandler>(serviceProvider =>
                                                   new LoggingHandler(serviceProvider.GetRequiredService<ILogger>()));

            services.AddScoped<InsertAssetFileSqliteHandler>(serviceProvider =>
                                                                 new InsertAssetFileSqliteHandler(serviceProvider
                                                                             .GetRequiredService<ISqliteHelper>()));

            services.TryAddScoped<IXeroCsvShotSessionParser, XeroCsvShotSessionParser>();
            services.TryAddScoped<GarminShotViewCsvHandler>(sp =>
                                                                new GarminShotViewCsvHandler(
                                                                     sp.GetRequiredService<ILogger>(),
                                                                     sp.GetRequiredService<IXeroCsvShotSessionParser>(),
                                                                     sp.GetRequiredService<IFirearmsService>(),
                                                                     sp.GetRequiredService<ISqliteHelper>()
                                                                    ));

            // Register handlers by concrete type for the pipeline
            services.AddScoped<IPipelineHandler<MlrbAssetFile>>(sp => sp
                                                                   .GetRequiredService<ValidateFileExistsHandler>());
            services.AddScoped<IPipelineHandler<MlrbAssetFile>>(sp => sp.GetRequiredService<CopyMlrbAssetHandler>());
            services.AddScoped<IPipelineHandler<MlrbAssetFile>>(sp => sp
                                                                   .GetRequiredService<InsertAssetFileSqliteHandler>());
            services.AddScoped<IPipelineHandler<MlrbAssetFile>>(sp => sp.GetRequiredService<LoggingHandler>());

            // Register the pipeline with handlers in order
            services.AddScoped<IPipeline<MlrbAssetFile>>(serviceProvider =>
                                                         {
                                                             Pipeline<MlrbAssetFile> pipeline = new(serviceProvider,
                                                                      serviceProvider.GetRequiredService<ILogger>());

                                                             return pipeline
                                                                   .Add<ValidateFileExistsHandler>()
                                                                   .Add<CopyMlrbAssetHandler>()
                                                                   .Add<InsertAssetFileSqliteHandler>()
                                                                   .Add<GarminShotViewCsvHandler>()
                                                                   .Add<LoggingHandler>();
                                                         });

            return services;
        }
    }
}