using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Services;
using SQLitePCL;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///     Extension methods for setting up SQLite and registering <see cref="ISqliteHelper" /> in the dependency injection
    ///     container.
    /// </summary>
    public static class SqliteHelperExtensions
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const string SQLITE_KEY = "sqlite";
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
        ///     Register the necessary things in DI as keyed services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
        /// <param name="configuration"></param>
        /// <returns>The original <see cref="IServiceCollection" /> for chaining.</returns>
        public static IServiceCollection AddMyLittleRangeBookSqlite(this IServiceCollection services, IConfiguration configuration)
        {
            SetSqlite3ProviderAndInit();

            string? connectionString = configuration.GetConnectionString("SqliteConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "SQLite connection string 'SqliteConnection' is not configured.");
            }

            services.TryAddSingleton<ISqliteHelper>(new SqliteHelper(connectionString));
            services.TryAddKeyedSingleton<ISimpleRangeLogService, SqliteSimpleRangeEventService>(SQLITE_KEY);
            services.TryAddKeyedSingleton<ISimpleRangeEventRepository, SqliteSimpleRangeEventRepository>(SQLITE_KEY);

            services.TryAddKeyedSingleton<IFirearmsService, SqliteFirearmsService>(SQLITE_KEY);

            return services;
        }
    }
}
