using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Firearms;

namespace MyLittleRangeBook
{
    public static partial class ServiceCollectionExtensions
    {
        static readonly Type[] SupportedFirearmsEvents = [
            typeof(FirearmAggregate.BarrelChanged),
            typeof(FirearmAggregate.FirearmCleaned),
            typeof(FirearmAggregate.FirearmCreated),
            typeof(FirearmAggregate.Modified),
            typeof(FirearmAggregate.NewNoteAdded),
            typeof(FirearmAggregate.RoundsFired),
            typeof(FirearmAggregate.SightingSystemChanged),
            typeof(FirearmAggregate.UsedInRangeEvent)
        ];

        public static IServiceCollection RegisterFirearmEventSourcing(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<IFirearmAggregateRepository, SqliteFirearmAggregateRepository>();
            return services;
        }
    }
}
