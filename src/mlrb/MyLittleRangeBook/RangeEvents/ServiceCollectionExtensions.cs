using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Persistence.Sqlite;
using MyLittleRangeBook.RangeEvents;

// ReSharper disable once CheckNamespace
namespace MyLittleRangeBook
{

    // [TO20260611] Keep this in the root namespace MyLittleRangeBook
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterRangeEventStuff(this IServiceCollection services)
        {
            services.TryAddScoped<ISimpleRangeEventDataProcessor, SimpleRangeEventDataProcessor>();
            return services;
        }
    }
}
