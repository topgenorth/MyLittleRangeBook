using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Cartridges;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEvents;
using SQLitePCL;
using ConfigurationExtensions = MyLittleRangeBook.Config.ConfigurationExtensions;

namespace MyLittleRangeBook.Persistence.Sqlite
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
        ///     A function that initializes and ensures the presence of a valid SQLite connection string
        ///     within a JSON-based application settings structure. Operates on a <see cref="JsonNode" />
        ///     root node and updates or creates the necessary connection string entries, ensuring
        ///     readiness for database operations.
        /// </summary>
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
            // [TO20260524] Note that the Nanoid is actually a ULID.
            connection.CreateFunction("nanoid", () => new MlrbId().ToString());
            connection.CreateFunction("utcnow", () => DateTimeOffset.UtcNow.ToString("O"));

            return connection;
        }

        /// <summary>
        ///     Register the necessary things in DI as keyed services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
        /// <param name="configuration"></param>
        /// <returns>The original <see cref="IServiceCollection" /> for chaining.</returns>
        public static IServiceCollection RegisterMyLittleRangeBookSqlite(this IServiceCollection services,
            IConfiguration configuration)
        {
            SetSqlite3ProviderAndInit();

            SqlMapper.AddTypeHandler(typeof(DateTimeOffset), new SqliteDateTimeOffsetHandler());
            SqlMapper.AddTypeHandler(typeof(DateTimeOffset?), new SqliteDateTimeOffsetHandler());
            SqlMapper.AddTypeHandler(typeof(MlrbId), new SqliteMlrbIdHandler());

            services.TryAddSingleton(configuration);

            services.TryAddSingleton<ISqliteHelper, SqliteHelper>();

            services.TryAddKeyedScoped<ISimpleRangeEventService, SqliteSimpleRangeEventService>(DI_KEYS_SQLITE);
            services.TryAddScoped<ISimpleRangeEventService, SqliteSimpleRangeEventService>();
            services
                .TryAddKeyedScoped<ISimpleRangeEventRepository, SqliteSimpleRangeEventRepository>(DI_KEYS_SQLITE);
            services.TryAddScoped<ISimpleRangeEventRepository, SqliteSimpleRangeEventRepository>();

            return services;
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
