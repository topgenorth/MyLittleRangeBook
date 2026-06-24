using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.EventSourcing;

// ReSharper disable once CheckNamespace
namespace MyLittleRangeBook
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterEventSourcingStuff(this IServiceCollection services)
        {
            services.TryAddScoped<IEventSourcingService, SqliteEventSourcingService>();
            return services;
        }
    }
}