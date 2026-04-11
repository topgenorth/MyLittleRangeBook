using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SQLitePCL;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///     Extension methods for setting up SQLite and registering <see cref="ISqliteHelper" /> in the dependency injection
    ///     container.
    /// </summary>
    public static class SqliteHelperExtensions
    {
        internal static readonly string DatabaseName = "mlrb.db";

        /// <summary>
        ///     Gets the settings directory path for storing user configuration.
        ///     Uses OS-specific local application data directory.
        ///     Creates a dedicated folder for this application to avoid conflicts.
        /// </summary>
        internal static string DatabaseDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MyLittleRangeBook");

        /// <summary>
        ///     Sets the SQLite3 provider and initializes the SQLite environment.
        ///     This should be called at application startup before any database operations.
        /// </summary>
        public static void SetSqlite3ProviderAndInit()
        {
            raw.SetProvider(new SQLite3Provider_e_sqlite3());
            Batteries.Init();
        }

        /// <summary>
        ///     Registers the <see cref="ISqliteHelper" /> service and initializes the SQLite provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
        /// <returns>The original <see cref="IServiceCollection" /> for chaining.</returns>
        public static IServiceCollection AddSqliteHelper(this IServiceCollection services, IConfiguration configuration)
        {
            if (services.Any(x => x.ServiceType == typeof(ISqliteHelper)))
            {
                return services;
            }

            SetSqlite3ProviderAndInit();

            string? connectionString = configuration.GetConnectionString("SqliteConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "SQLite connection string 'SqliteConnection' is not configured.");
            }

            services.AddSingleton<ISqliteHelper>(new SqliteHelper(connectionString));

            return services;
        }

        /// <summary>
        ///     Determines the full file path for the SQLite database based on the current environment.
        ///     Suffixes the database name with the environment name (e.g., Development) if not in Production.
        /// </summary>
        /// <returns>The full path to the SQLite database file.</returns>
        public static string GetSqliteDatabaseName()
        {
            string settingsDirectory = DatabaseDirectory;
            string env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;

            string dbPath;
            if ("Production".Equals(env, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(env))
            {
                dbPath = Path.Combine(settingsDirectory, DatabaseName);
            }
            else
            {
                var f = new FileInfo(DatabaseName);
                dbPath = Path.Combine(settingsDirectory, $"{f.Name}-{env.ToLower()}.{f.Extension}");
            }

            return dbPath;
        }
    }
}
