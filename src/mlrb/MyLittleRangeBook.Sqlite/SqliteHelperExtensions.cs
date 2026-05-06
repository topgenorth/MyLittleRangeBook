using System.Text.Json.Nodes;
using Dapper;
using FluentResults;
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

        public static readonly Func<JsonNode?, Result> SqliteConnectionStringBootStrapper = rootNode =>
        {
            if (rootNode is not JsonObject rootObject)
            {
                return Result.Fail("Root appsettings JSON must be a JSON object.");
            }

            JsonNode? n1 = rootObject["ConnectionStrings"];
            if (n1 == null)
            {
                rootObject["ConnectionStrings"] = new JsonObject();
                n1 = rootObject["ConnectionStrings"];
            }

            JsonNode? n2 = n1!["SqliteConnection"];
            if (n2 is null)
            {
                var builder = new SqliteConnectionStringBuilder
                {
                    DataSource = DefaultSqliteDatabaseName(), Mode = SqliteOpenMode.ReadWriteCreate
                };

                n1["SqliteConnection"] = $"{builder.ConnectionString}";
            }

            return Result.Ok();
        };


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

            services.TryAddKeyedTransient<ISimpleRangeLogService, SqliteSimpleRangeEventService>(DI_KEYS_SQLITE);
            services.TryAddKeyedTransient<ISimpleRangeEventRepository, SqliteSimpleRangeEventRepository>(DI_KEYS_SQLITE);
            services.TryAddKeyedTransient<IFirearmsDbService, SqliteFirearmsDbService>(DI_KEYS_SQLITE);
            services.TryAddKeyedTransient<IFitFilesDbService, SqliteFitFilesDbService>(DI_KEYS_SQLITE);

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

        /// <summary>
        ///     Ensures that the given JSON node contains a default SQLite connection string in the "ConnectionStrings" section. If
        ///     the "ConnectionStrings" section or the "SqliteConnection" entry does not exist, they will be created with a default
        ///     connection string that points to a SQLite database file in the user's local application data folder. The method
        ///     returns true if the JSON node was modified (i.e., if the connection string was added), and false if it already
        ///     contained a SQLite connection string. This is useful for bootstrapping an appsettings.json file with a default
        ///     SQLite connection string if one does not already exist.
        /// </summary>
        /// <param name="rootNode"></param>
        /// <returns></returns>
        public static bool EnsureDefaultSqliteConnectionString(this JsonNode? rootNode)
        {
            var wasUpdated = false;
            rootNode ??= new JsonObject();

            JsonNode? n1 = rootNode["ConnectionStrings"];
            if (n1 == null)
            {
                rootNode["ConnectionStrings"] = new JsonObject();
                n1 = rootNode["ConnectionStrings"];
            }

            JsonNode? n2 = n1!["SqliteConnection"];
            if (n2 is not null)
            {
                return wasUpdated;
            }

            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = DefaultSqliteDatabaseName(), Mode = SqliteOpenMode.ReadWriteCreate
            };

            n1["SqliteConnection"] = $"{builder.ConnectionString}";
            wasUpdated = true;

            return wasUpdated;
        }
    }
}
