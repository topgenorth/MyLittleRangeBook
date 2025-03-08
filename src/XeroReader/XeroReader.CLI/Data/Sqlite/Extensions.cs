using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace net.opgenorth.xero.Data.Sqlite;

public static class Extensions
{
    /// <summary>
    ///     Add the options to DI for the Sqlite database.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddSqliteDatabase(this IHostApplicationBuilder builder)
    {
        IConfigurationManager c = builder.Configuration;
        IConfigurationSection x = c.GetSection(SqliteOptions.ConfigSection);

        builder.Services.Configure<SqliteOptions>(x);
        builder.Services.TryAddScoped<IDbZookeeper, SqliteDbZookeeper>();

        return builder;
    }


    public static string MakeSqliteConnectionString(this SqliteOptions opt)
    {
        string sqliteFile = opt.SqliteFile;
        SqliteConnectionStringBuilder b = new($"Data Source={sqliteFile}");

        return b.ConnectionString;
    }

    public static string GetAppNameAndVersion()
    {
        string location = Environment.GetCommandLineArgs()[0];
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(location);
        string version = fvi.ProductVersion;

        return $"{fvi.ProductName} {version}";
    }
}
