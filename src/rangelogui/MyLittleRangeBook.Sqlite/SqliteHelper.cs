using NanoidDotNet;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///     A helper class for managing SQLite database connections, initialization, and configuration.
    ///     Provides methods for setting up the database environment, obtaining connections, and generating connection strings.
    /// </summary>
    public class SqliteHelper : ISqliteHelper
    {
        /// <summary>
        ///     Gets the settings directory path for storing user configuration.
        ///     Uses OS-specific local application data directory.
        ///     Creates a dedicated folder for this application to avoid conflicts.
        /// </summary>
        internal static string DatabaseDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MyLittleRangeBook");

        /// <summary>
        ///     Creates a new <see cref="SqliteConnection" /> and opens it for usage.
        /// </summary>
        /// <remarks>
        ///     Ensure that the connection is disposed of after use.
        /// </remarks>
        /// <param name="connectionString">Optional connection string to use. If null, the default connection string is generated.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The opened connection.</returns>
        public async Task<SqliteConnection> OpenSqliteConnectionAsync(string connectionString,
            CancellationToken cancellationToken = default)
        {
            var connection = new SqliteConnection(connectionString);
            connection.CreateFunction("nanoid", () => Nanoid.Generate());
            connection.CreateFunction("utcnow", () => DateTimeOffset.UtcNow.ToString("O"));
            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        /// <summary>
        ///     Generates a SQLite connection string based on the current environment and file path settings.
        ///     Ensures the parent directory for the database file exists.
        /// </summary>
        /// <returns>A connection string configured for the local database file.</returns>
        public string GetSqliteConnectionString()
        {
            var dbPath = GetSqliteDatabaseName();
            var settingsDirectory = Path.GetDirectoryName(dbPath)!;
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }


            var cb = new SqliteConnectionStringBuilder { DataSource = dbPath, Mode = SqliteOpenMode.ReadWriteCreate };


            return cb.ConnectionString;
        }

        /// <summary>
        ///     Determines the full file path for the SQLite database based on the current environment.
        ///     Suffixes the database name with the environment name (e.g., Development) if not in Production.
        /// </summary>
        /// <returns>The full path to the SQLite database file.</returns>
        public string GetSqliteDatabaseName()
        {
            var settingsDirectory = DatabaseDirectory;
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;

            string dbPath;
            if ("Production".Equals(env, StringComparison.OrdinalIgnoreCase))
            {
                dbPath = Path.Combine(settingsDirectory, "mlrb.db");
            }
            else
            {
                var dbName = $"mlrb-{env!.ToLower()}.db";
                dbPath = Path.Combine(settingsDirectory, dbName);
            }

            return dbPath;
        }

        /// <summary>
        ///     Returns the connection string for this instance.
        /// </summary>
        /// <returns>The database connection string.</returns>
        public override string ToString()
        {
            return GetSqliteConnectionString();
        }

        /// <summary>
        ///     Checks if the SQLite database file exists on disk.
        /// </summary>
        /// <returns><c>true</c> if the database file exists; otherwise, <c>false</c>.</returns>
        public bool DoesDatabaseExist()
        {
            return File.Exists(GetSqliteDatabaseName());
        }

        public async Task<SqliteConnection> OpenSqliteConnectionToFileAsync(string? file = null,
            CancellationToken cancellationToken = default)
        {
            file ??= GetSqliteConnectionString();
            var sb = new SqliteConnectionStringBuilder { DataSource = file, Mode = SqliteOpenMode.ReadWriteCreate };

            return await OpenSqliteConnectionAsync(sb.ConnectionString, cancellationToken);
        }
    }
}
