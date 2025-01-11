using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace net.opgenorth.xero.data.sqlite
{
    public static class Extensions
    {
        public const string DEFAULT_DATA_DIRECTORY = ".mlrb";


        public static SqliteOptions InferDataDirectory(this SqliteOptions options)
        {
            options.DataDirectory = InferDataDirectory(options.SqliteFile);

            return options;
        }

        /// <summary>
        ///     If the file does not have a directory, then make a guess.  For DEVELOPMENT, it's same directory
        ///     as the assembly.  For PRODUCTION, it's the user's home directory/.mlrb.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string InferDataDirectory(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return string.Empty;
            }

            string dir = Path.GetDirectoryName(filename);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                return dir;
            }

            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development")
            {
                dir = AppContext.BaseDirectory;
            }
            else
            {
                dir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            dir = Path.Combine(dir, DEFAULT_DATA_DIRECTORY);

            return dir;
        }

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
}
