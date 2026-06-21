using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        static readonly Type[] SupportedFirearmsEvents = [
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

            services.TryAddKeyedScoped<IFirearmsService, FirearmsService>(SqliteHelperExtensions.DI_KEYS_SQLITE);
            services.TryAddScoped<IFirearmAggregateRepository, SqliteFirearmAggregateRepository>();
            services.TryAddScoped<IFirearmsService, FirearmsService>();
            return services;
        }
    }
}
