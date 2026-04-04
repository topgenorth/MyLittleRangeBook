using System.Diagnostics;
using Microsoft.Data.Sqlite;
using NanoidDotNet;

namespace MySimpleRangeLog.CLI.Database
{
    public class SqliteHelper
    {
        /// <summary>
        ///     Name of the database for "production".
        /// </summary>
        public const string DATABASE_NAME = "simplerangelog.db";

        /// <summary>
        ///     Gets the settings directory path for storing user configuration.
        ///     Uses OS-specific local application data directory.
        ///     Creates a dedicated folder for this application to avoid conflicts.
        /// </summary>
        internal static string SettingsDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SimpleRangeLog");

        /// <summary>
        ///     Creates a new <see cref="SqliteConnection" /> and opens it for usage.
        /// </summary>
        /// <remarks>
        ///     Ensure that the connection is disposed of after use.
        /// </remarks>
        /// <returns>The opened connection.</returns>
        internal static async Task<SqliteConnection> GetOpenConnectionAsync(string connectionString = null,
            CancellationToken cancellationToken = default)
        {
            connectionString ??= new SqliteHelper().GetConnectionString();
            try
            {
                var connection = new SqliteConnection(connectionString);
                connection.CreateFunction("nanoid", () => Nanoid.Generate());
                connection.CreateFunction("utcnow", () => DateTimeOffset.UtcNow.ToString("O"));

                Log.Verbose("Using SQLite database {connectionString}", connectionString);
                await connection.OpenAsync(cancellationToken);

                return connection;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Log.Logger.Error(e, "Failed to open database connection");

                throw;
            }
        }

        /// <summary>
        ///     Gets the absolute path to the SQLite database file on the local machine.
        ///     Creates the app-specific subdirectory in the local app data folder if it doesn't exist.
        /// </summary>
        /// <returns>The full path to the database file (e.g., C:\Users\Name\AppData\Local\SimpleRangeLog\fileName.db on Windows)</returns>
        public string GetConnectionString()
        {
            var dbPath = GetDatabaseName();
            var settingsDirectory = Path.GetDirectoryName(dbPath)!;
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
                Log.Logger.Verbose("Creating folder for database {SettingsDirectory}.", settingsDirectory);
            }


            var cb = new SqliteConnectionStringBuilder { DataSource = dbPath, Mode = SqliteOpenMode.ReadWriteCreate };


            return cb.ConnectionString;
        }

        public string GetDatabaseName()
        {
            var settingsDirectory = SettingsDirectory;
            string dbPath;
            if (EnvironmentHelper.IsProduction)
            {
                dbPath = Path.Combine(settingsDirectory, DATABASE_NAME);
            }
            else
            {
                var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;
                var dbName = $"simplerangelog-{env!.ToLower()}.db";
                dbPath = Path.Combine(settingsDirectory, dbName);
            }

            return dbPath;
        }
    }
}
