using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using net.opgenorth.xero.shotview;

namespace net.opgenorth.xero.data.sqlite;

public static class SqliteExtensions
{
    public static IServiceCollection AddWorksheetSqlite(this IServiceCollection services)
    {
        services.TryAddScoped<IGetShotSession, MyLittleRangeBookRepository>();
        services.TryAddScoped<IPersistShotSession, MyLittleRangeBookRepository>();
        services.TryAddScoped<MyLittleRangeBookRepository>();
        return services;
    }
}
