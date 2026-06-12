using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{

    // [TO20260611] Keep this in the root namespace MyLittleRangeBook
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterRangeEventStuff(this IServiceCollection services)
        {
            services.TryAddKeyedScoped<IFirearmsService, FirearmsService>(SqliteHelperExtensions.DI_KEYS_SQLITE);
            services.TryAddScoped<IFirearmsService, FirearmsService>();

            return services;
        }
    }
}
