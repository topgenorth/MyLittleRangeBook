using Microsoft.Extensions.DependencyInjection;
using SQLitePCL;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    /// Extension methods for setting up SQLite and registering <see cref="ISqliteHelper"/> in the dependency injection container.
    /// </summary>
    public static class SqliteHelperExtensions
    {
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
        /// Registers the <see cref="ISqliteHelper"/> service and initializes the SQLite provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddSqliteHelper(this IServiceCollection services)
        {
            if (services.Any(x => x.ServiceType == typeof(ISqliteHelper)))
            {
                return services;
            }

            SetSqlite3ProviderAndInit();

            services.AddSingleton<ISqliteHelper, SqliteHelper>();
            return services;
        }
    }
}