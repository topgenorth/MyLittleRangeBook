using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MyLittleRangeBook.IO;

namespace MyLittleRangeBook.Config
{
    /// <summary>
    ///     Extension methods for configuration-related operations.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        ///     The folder name used to store range event assets, with platform-specific naming
        ///     conventions. On Windows, it is "RangeEventAssets", and on non-Windows platforms,
        ///     it is "range-event-assets".
        /// </summary>
        public static readonly string RangeAssetsFolderName =
            OperatingSystem.IsWindows() ? "RangeEventAssets" : "range-event-assets";

        /// <summary>
        ///     The name of the default database.
        /// </summary>
        // TODO [TO20260425] Move this to the SQLite Assembly
        internal static readonly string SqliteDatabaseName = "mlrb.db";

        /// <summary>
        ///     Name of the JSON file that holds application settings.
        /// </summary>
        internal static readonly string AppSettingsFileName = "appsettings.json";

        /// <summary>
        ///     Default name of this application's local application data folder.'
        /// </summary>
        internal static readonly string MlrbDataFolderName =
            OperatingSystem.IsWindows() ? "MyLittleRangeBook" : "mylittlerangebook";

        /// <summary>
        ///     Gets the user settings directory path for this application.
        ///     Uses OS-specific local application data directory.
        ///     Creates a dedicated folder for this application to avoid conflicts.
        /// </summary>
        public static DirectoryInfo DefaultUserSettingsDirectory => new(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            MlrbDataFolderName));

        /// <summary>
        ///     Gets the default log directory path for this application. It is a "Logs" subdirectory within the user settings
        ///     directory. This keeps logs organized and separate from other application data.
        /// </summary>
        public static DirectoryInfo DefaultLogDirectory => new(Path.Combine(DefaultUserSettingsDirectory.FullName,
            OperatingSystem.IsWindows() ? "Logs" : "logs"));

        /// <summary>
        ///     Full file path of the default log file, used for logging application events.
        /// </summary>
        public static string DefaultLogFile => Path.Combine(DefaultLogDirectory.FullName, "mlrb-.log");

        public static FileInfo DefaultAppSettingsFile =>
            new FileInfo(Path.Combine(DefaultUserSettingsDirectory.FullName, AppSettingsFileName))
                .InjectEnvironmentIntoFileName();

        /// <summary>
        ///     Reads the default Serilog configuration section from an embedded JSON file. This provides a fallback configuration
        ///     for Serilog if no configuration is found in the appsettings.json file. The embedded JSON file should contain a
        ///     valid Serilog configuration section that can be merged with the application's configuration at runtime.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> DefaultSerilogSectionJson()
        {
            return await typeof(ConfigurationExtensions)
                .Assembly.ReadEmbeddedTextFileAsync("MyLittleRangeBook.Config.AppSettings.SerilogSection.json");
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static async Task<string> DefaultLoggingSectionJson()
        {
            return await typeof(ConfigurationExtensions)
                .Assembly.ReadEmbeddedTextFileAsync("MyLittleRangeBook.Config.AppSettings.LoggingSection.json");
        }

        /// <summary>
        ///     Configures the application's configuration sources based on the current environment. In production, it loads only
        ///     the default appsettings.json file from the user settings directory. In development and staging environments, it
        ///     loads appsettings.json and appsettings.{Environment}.json from the current directory, as well as the default
        ///     appsettings.json from the user settings directory. Environment variables are not included in this configuration for
        ///     now.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostApplicationBuilder AddMyLittleRangeBookConfig(this IHostApplicationBuilder builder)
        {
            builder.Configuration.Sources.Clear();

            if (EnvironmentExtensions.IsProduction)
            {
                builder.Configuration
                    .AddJsonFile(DefaultAppSettingsFile.FullName, false, true);
            }
            else
            {
                string env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

                builder.Configuration
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{env}.json", true, true);
                builder.Configuration.AddJsonFile(DefaultAppSettingsFile.FullName, true, true);
                // builder.Services.AddPostgresHelper(builder.Configuration);
            }

            // [TO20260425] Leave out the environment variables for now.
            // builder.Configuration.AddEnvironmentVariables();
            return builder;
        }

        /// <summary>
        ///     Configures the application's configuration sources based on the current environment.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IConfigurationRoot AddMyLittleRangeBookConfig(this IServiceCollection services)
        {
            IConfigurationBuilder cb = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());

            if (EnvironmentExtensions.IsProduction)
            {
                cb.AddJsonFile(DefaultAppSettingsFile.FullName, false, true);
            }
            else
            {
                string env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;
                cb.AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{env}.json", true, true);
                cb.AddJsonFile(DefaultAppSettingsFile.FullName, true, true);
                // builder.Services.AddPostgresHelper(builder.Configuration);
            }

            IConfigurationRoot config = cb.Build();
            services.TryAddSingleton(config);

            return config;
        }

        /// <summary>
        ///     Retrieves the directory path where range asset files are stored. This directory is determined
        ///     by appending the 'RangeAssetsFolderName' value to the directory path of the SQLite connection string
        ///     specified in the configuration.
        /// </summary>
        /// <param name="config">The application configuration object that provides access to the SQLite connection string.</param>
        /// <returns>
        ///     The fully qualified directory path for the range asset files.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the SQLite connection string ('SqliteConnection') is not configured
        ///     in the application settings.
        /// </exception>
        public static string GetRangeAssetDirectory(this IConfiguration config)
        {
            string connectionString = config.GetSqliteConnectionString();
            var sb = new SqliteConnectionStringBuilder(connectionString);
            string db = sb.DataSource;
            string? dir = Path.GetDirectoryName(db);
            string rangeAssetsDir = Path.Combine(dir!, RangeAssetsFolderName);
            Directory.CreateDirectory(rangeAssetsDir);

            return rangeAssetsDir;
        }

        /// <summary>
        ///     Retrieves the SQLite connection string from the given configuration. This connection string is used to
        ///     establish a connection to the SQLite database configured in the application settings.
        /// </summary>
        /// <param name="config">
        ///     The application configuration object from which to retrieve the SQLite connection string.
        ///     Typically, this is an instance of <see cref="IConfiguration" />.
        /// </param>
        /// <returns>
        ///     A <see cref="string" /> representing the SQLite connection string configured under the key
        ///     "SqliteConnection" in the application's configuration.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the connection string "SqliteConnection" is not configured or is null/empty in the provided
        ///     configuration.
        /// </exception>
        public static string GetSqliteConnectionString(this IConfiguration config)
        {
            string? connectionString = config.GetConnectionString("SqliteConnection");

            return string.IsNullOrEmpty(connectionString)
                ? throw new InvalidOperationException("SQLite connection string 'SqliteConnection' is not configured.")
                : connectionString!;
        }
    }
}
