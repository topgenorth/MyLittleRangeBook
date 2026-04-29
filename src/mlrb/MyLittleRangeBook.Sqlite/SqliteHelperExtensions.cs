using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Services;
using NanoidDotNet;
using SQLitePCL;
using ConfigurationExtensions = MyLittleRangeBook.Config.ConfigurationExtensions;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///     Extension methods for setting up SQLite and registering <see cref="ISqliteHelper" /> in the dependency injection
    ///     container.
    /// </summary>
    public static class SqliteHelperExtensions
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const string DI_KEYS_SQLITE = "sqlite";

        /// <summary>
        ///     The name of the default database.
        /// </summary>
        public const string SQLITE_DATABASE_NAME = "mlrb.db";


        /// <summary>
        ///     Sets the SQLite3 provider and initializes the SQLite environment.
        ///     This should be called at application startup before any database operations.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static void SetSqlite3ProviderAndInit()
        {
            raw.SetProvider(new SQLite3Provider_e_sqlite3());
            Batteries.Init();
        }

        /// <summary>
        ///     Determines the full file path for the SQLite database based on the current environment.
        ///     Suffixes the database name with the environment name (e.g., Development) if not in Production.
        /// </summary>
        /// <param name="inferFromEnvironment">
        ///     If set to true, then the database name will be suffixed with the current environment
        ///     name (e.g., Development). Defaults to true.
        /// </param>
        /// <returns>The full path to the SQLite database file.</returns>
        public static string DefaultSqliteDatabaseName(bool inferFromEnvironment = true)
        {
            // TODO [TO20260425] Move this to the SQLite Assembly
            string fullPath = Path.Combine(ConfigurationExtensions.DefaultUserSettingsDirectory.FullName,
                SQLITE_DATABASE_NAME);
            if (inferFromEnvironment)
            {
                fullPath = new FileInfo(fullPath).InjectEnvironmentIntoFileName().FullName;
            }

            if (!OperatingSystem.IsWindows())
            {
                fullPath = fullPath.ToLowerInvariant();
            }

            return fullPath;
        }


        /// <summary>
        ///     Add some custom functions to the SQLite connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static SqliteConnection AddFunctions(this SqliteConnection connection)
        {
            connection.CreateFunction("nanoid", () => Nanoid.Generate());
            connection.CreateFunction("utcnow", () => DateTimeOffset.UtcNow.ToString("O"));

            return connection;
        }

        /// <summary>
        ///     Register the necessary things in DI as keyed services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
        /// <param name="configuration"></param>
        /// <returns>The original <see cref="IServiceCollection" /> for chaining.</returns>
        public static IServiceCollection AddMyLittleRangeBookSqlite(this IServiceCollection services,
            IConfiguration configuration)
        {
            SetSqlite3ProviderAndInit();

            SqlMapper.AddTypeHandler(typeof(DateTimeOffset), new SqliteDateTimeOffsetHandler());
            SqlMapper.AddTypeHandler(typeof(DateTimeOffset?), new SqliteDateTimeOffsetHandler());

            services.TryAddSingleton(configuration);
            services.TryAddSingleton<ISqliteHelper, SqliteHelper>();

            services.TryAddKeyedSingleton<ISimpleRangeLogService, SqliteSimpleRangeEventService>(DI_KEYS_SQLITE);
            services.TryAddKeyedSingleton<ISimpleRangeEventRepository, SqliteSimpleRangeEventRepository>(DI_KEYS_SQLITE);

            services.TryAddKeyedSingleton<IFirearmsService, SqliteFirearmsService>(DI_KEYS_SQLITE);

            return services;
        }

        public static async Task EnsureSqliteDatabaseIsInAppSettings(string appSettingsFileName)
        {
            string appSettingsJson = await File.ReadAllTextAsync(appSettingsFileName);
            var jsonRoot = JsonNode.Parse(appSettingsJson);
            if (jsonRoot.EnsureDefaultSqliteConnectionString())
            {
                if (jsonRoot is not null)
                {
                    await File.WriteAllTextAsync(appSettingsFileName, jsonRoot.ToString());
                }
            }

        }
        public static bool EnsureDefaultSqliteConnectionString(this JsonNode? rootNode)
        {
            bool wasUpdated = false;
            rootNode ??= new JsonObject();

            JsonNode? n1 = rootNode["ConnectionStrings"];
            if (n1 == null)
            {
                rootNode["ConnectionStrings"] = new JsonObject();
                n1 = rootNode["ConnectionStrings"];
            }

            JsonNode? n2 = n1!["SqliteConnection"];
            if (n2 is null)
            {
                SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder
                {
                    DataSource = DefaultSqliteDatabaseName(), Mode = SqliteOpenMode.ReadWriteCreate
                };

                n1["SqliteConnection"] = $"{builder.ConnectionString}";
                wasUpdated = true;
            }

            return wasUpdated;
        }
    }
}
