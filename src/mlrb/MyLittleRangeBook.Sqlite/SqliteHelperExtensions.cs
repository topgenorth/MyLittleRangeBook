using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Services;
using NanoidDotNet;
using SQLitePCL;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///     Extension methods for setting up SQLite and registering <see cref="ISqliteHelper" /> in the dependency injection
    ///     container.
    /// </summary>
    public  static class SqliteHelperExtensions
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
        /// Add some custom functions to the SQLite connection.
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
        public static IServiceCollection AddMyLittleRangeBookSqlite(this IServiceCollection services, IConfiguration configuration)
        {
            SetSqlite3ProviderAndInit();

            services.TryAddSingleton<IConfiguration>(configuration);
            services.TryAddSingleton<ISqliteHelper, SqliteHelper>();
            services.TryAddKeyedSingleton<ISimpleRangeLogService, SqliteSimpleRangeEventService>(SQLITE_KEY);
            services.TryAddKeyedSingleton<ISimpleRangeEventRepository, SqliteSimpleRangeEventRepository>(SQLITE_KEY);

            services.TryAddKeyedSingleton<IFirearmsService, SqliteFirearmsService>(SQLITE_KEY);

            return services;
        }
    }
}
