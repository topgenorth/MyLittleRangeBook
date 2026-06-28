using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.Firearms.FirearmAggregate;

// ReSharper disable once CheckNamespace
namespace MyLittleRangeBook
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// This should match the JsonSerializeble attributes in MlrbJsonSerializerContext.cs
        /// </summary>
        static readonly Type[] s_supportedFirearmsEvents = [
            typeof(FirearmActive),
            typeof(FirearmAssociatedWithAsset),
            typeof(FirearmAssociatedWithRangeEvent),
            typeof(FirearmBarrelChanged),
            typeof(FirearmCleaned),
            typeof(FirearmCreated),
            typeof(FirearmDischargeMoreRounds),
            typeof(FirearmInactive),
            typeof(FirearmModified),
            typeof(FirearmNoteAdded),
            typeof(FirearmSightingSystemChanged),
        ];

        public static IServiceCollection RegisterFirearmEventSourcing(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            // services.TryAddKeyedScoped<IProjector, FirearmProjector>(FirearmProjector.DI_KEY);
            services.TryAddKeyedScoped<IFirearmsService, FirearmsService>(SqliteHelperExtensions.DI_KEYS_SQLITE);
            services.TryAddScoped<IFirearmsService, FirearmsService>();
            services.TryAddScoped<IFirearmAggregateRepository, SqliteFirearmAggregateRepository>();
            return services;
        }
    }
}
