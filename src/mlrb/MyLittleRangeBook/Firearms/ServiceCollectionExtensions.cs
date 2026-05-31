using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Firearms;
using static MyLittleRangeBook.Firearms.FirearmAggregate;

namespace MyLittleRangeBook
{
    public static partial class ServiceCollectionExtensions
    {
        static readonly Type[] SupportedFirearmsEvents = [
            typeof(BarrelChanged),
            typeof(FirearmActive),
            typeof(FirearmCleaned),
            typeof(FirearmCreated),
            typeof(FirearmInactive),
            typeof(FirearmModified),
            typeof(FiredMoreBullets),
            typeof(NewNoteAdded),
            typeof(SightingSystemChanged),
            typeof(UsedInRangeEvent)
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
