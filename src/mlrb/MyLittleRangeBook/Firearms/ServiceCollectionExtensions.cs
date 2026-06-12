using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.Firearms.FirearmAggregate;

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
            typeof(FirearmRoundCountRecalculated),
            typeof(FirearmSightingSystemChanged),
        ];

        public static IServiceCollection RegisterFirearmEventSourcing(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddKeyedScoped<IFirearmsService, FirearmsService>(SqliteHelperExtensions.DI_KEYS_SQLITE);
            services.AddScoped<IFirearmAggregateRepository, SqliteFirearmAggregateRepository>();
            services.AddScoped<IFirearmsService, FirearmsService>();
            return services;
        }
    }
}
