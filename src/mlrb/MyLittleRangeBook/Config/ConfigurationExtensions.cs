using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;

namespace MyLittleRangeBook.Config
{
    public static class ConfigurationExtensions
    {
        const string LogFileTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        ///     The name of the default database.
        /// </summary>
        // TODO [TO20260425] Move this to the SQLite Assembly
        internal static readonly string SqliteDatabaseName = "mlrb.db";

        internal static readonly string AppSettingsFileName = "appsettings.json";

        /// <summary>
        ///     Default name of this application's local application data folder.'
        /// </summary>
        internal static readonly string DefaultLocalAppDataFolder =
            OperatingSystem.IsWindows() ? "MyLittleRangeBook" : "mylittlerangebook";

        /// <summary>
        ///     Gets the user settings directory path for this application.
        ///     Uses OS-specific local application data directory.
        ///     Creates a dedicated folder for this application to avoid conflicts.
        /// </summary>
        public static DirectoryInfo DefaultUserSettingsDirectory => new(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            DefaultLocalAppDataFolder));

        public static DirectoryInfo DefaultLogDirectory =>
            new(Path.Combine(DefaultUserSettingsDirectory.FullName, "Logs"));

        public static string DefaultLogFile => Path.Combine(DefaultLogDirectory.FullName, "mlrb-.log");

        public static FileInfo DefaultAppSettingsFile =>
            new FileInfo(Path.Combine(DefaultUserSettingsDirectory.FullName, AppSettingsFileName))
                .InjectEnvironmentIntoFileName();

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
            string fullPath = Path.Combine(DefaultUserSettingsDirectory.FullName, SqliteDatabaseName);
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

        public static IHostApplicationBuilder AddMyLittleRangeBookJsonFiles(this IHostApplicationBuilder builder)
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
                // builder.Configuration.AddJsonFile(DefaultAppSettingsFile.FullName, true, true);
                // builder.Services.AddPostgresHelper(builder.Configuration);
            }

            // [TO20260425] Leave out the environment variables for now.
            // builder.Configuration.AddEnvironmentVariables();
            return builder;
        }

        public static IConfigurationRoot AddMyLittleRangeBookJsonFiles(this IServiceCollection services)
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
                // builder.Configuration.AddJsonFile(DefaultAppSettingsFile.FullName, true, true);
                // builder.Services.AddPostgresHelper(builder.Configuration);
            }

            IConfigurationRoot config = cb.Build();
            services.TryAddSingleton(config);

            return config;
        }

        /// <summary>
        ///     Configure Serilog to log to files.
        /// </summary>
        /// <param name="sinkConfiguration"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static LoggerConfiguration MlrbLogFiles(this LoggerSinkConfiguration sinkConfiguration)
        {
            ArgumentNullException.ThrowIfNull(sinkConfiguration);

            return sinkConfiguration.File(
                    DefaultLogFile,
                    rollingInterval: RollingInterval.Day, // Create new log file each day
                    retainedFileCountLimit: 7, // Keep only 7 days of logs
                    shared: true, // Allow multiple instances to write
                    flushToDiskInterval: TimeSpan.FromSeconds(1), // Periodically flush to disk
                    buffered: false, // Write directly for reliability
                    outputTemplate:
                    LogFileTemplate)
                ;
        }
    }
}
