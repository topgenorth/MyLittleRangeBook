using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Cartridges;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    // [TO20260611] Keep this in the root MyLittleRangeBook namespace.
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCartridges(this IServiceCollection services)
        {
            services.TryAddKeyedScoped<ICartridgesService, CartridgesService>(SqliteHelperExtensions.DI_KEYS_SQLITE);
            services.TryAddScoped<ICartridgesService, CartridgesService>();
            return services;
        }
    }
}
