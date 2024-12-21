using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace net.opgenorth.xero.data.sqlite
{
    public static class Extensions
    {
        /// <summary>
        ///     Add the options to DI for the Sqlite database.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostApplicationBuilder AddGarminShotViewDatabase(this IHostApplicationBuilder builder)
        {
            IConfigurationManager c = builder.Configuration;
            IConfigurationSection x = c.GetSection(GarminShotViewSqliteOptions.ConfigSection);

            builder.Services.Configure<GarminShotViewSqliteOptions>(x);
            builder.Services.TryAddScoped<IDbZookeeper, SqliteDbZookeeper>();

            return builder;
        }

        public  static string MakeConnectionString(this GarminShotViewSqliteOptions opt)
        {
            SqliteConnectionStringBuilder b = new($"Data Source={opt.SqliteFile}");

            return b.ConnectionString;
        }
    }
}
