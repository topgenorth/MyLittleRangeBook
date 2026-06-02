using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Firearms;
using static MyLittleRangeBook.Firearms.FirearmAggregate;

namespace MyLittleRangeBook
{
    public static partial class ServiceCollectionExtensions
    {
        static readonly Type[] SupportedFirearmsEvents = [
            typeof(AssetAssociatedWithFirearm),
            typeof(FirearmActive),
            typeof(FirearmBarrelChanged),
            typeof(FirearmCleaned),
            typeof(FirearmCreated),
            typeof(FirearmDischargeMoreRounds),
            typeof(FirearmInactive),
            typeof(FirearmModified),
            typeof(FirearmNoteAdded),
            typeof(FirearmSightingSystemChanged),
            typeof(RangeEventAssociatedWithFirearm),
        ];

        public static IServiceCollection RegisterFirearmEventSourcing(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<IFirearmAggregateRepository, SqliteFirearmAggregateRepository>();
            services.AddScoped<IFirearmsService, FirearmsService>();
            return services;
        }
    }
}
