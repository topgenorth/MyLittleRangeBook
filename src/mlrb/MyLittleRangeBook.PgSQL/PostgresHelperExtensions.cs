using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyLittleRangeBook.PgSQL
{
    public static class PostgresHelperExtensions
    {
        public static IServiceCollection AddPostgresHelper(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("PostgresqlConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                Serilog.Log.Warning("PostgreSQL connection string 'PostgresqlConnection' is not configured.");
                return services;
            }

            services.AddSingleton<IPostgresHelper>(new PostgresHelper(connectionString));
            return services;
        }
    }
}
