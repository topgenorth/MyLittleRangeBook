namespace MyLittleRangeBook.Database.Sqlite
{
    public interface ISqliteHelper
    {
        /// <summary>
        ///     Generates a SQLite connection string based on the current environment and file path settings.
        ///     Ensures the parent directory for the database file exists.
        /// </summary>
        /// <returns>A connection string configured for the local database file.</returns>
        string GetSqliteConnectionString();

        /// <summary>
        ///     Determines the full file path for the SQLite database based on the current environment.
        ///     Suffixes the database name with the environment name (e.g., Development) if not in Production.
        /// </summary>
        /// <returns>The full path to the SQLite database file.</returns>
        string GetSqliteDatabaseName();

        /// <summary>
        ///     Returns the connection string for this instance.
        /// </summary>
        /// <returns>The database connection string.</returns>
        string ToString();

        bool DoesDatabaseExist();

        /// <summary>
        ///     Creates a new <see cref="SqliteConnection" /> and opens it for usage.
        /// </summary>
        /// <remarks>
        ///     Ensure that the connection is disposed of after use.
        /// </remarks>
        /// <param name="connectionString">Optional connection string to use. If null, the default connection string is generated.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The opened connection.</returns>
        Task<SqliteConnection> OpenSqliteConnectionAsync(string connectionString,
            CancellationToken cancellationToken = default);

        Task<SqliteConnection> OpenSqliteConnectionToFileAsync(string? file = null,
            CancellationToken cancellationToken = default);

    }
}
