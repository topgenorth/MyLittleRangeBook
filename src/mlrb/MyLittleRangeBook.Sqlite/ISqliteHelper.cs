using FluentResults;

namespace MyLittleRangeBook.Database.Sqlite
{
    public interface ISqliteHelper
    {
        public string DatabaseFile { get; }

        /// <summary>
        ///    Writes the provided file contents to the specified table in the SQLite database.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="table"></param>
        /// <param name="fileName"></param>
        /// <param name="fileContents"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A tuple that will hold the Nanoid and the RowId of the inserted row.</returns>
        public Task<Result<(string id, long rowId)>> WriteFileToTableAsync(SqliteConnection conn,
            SqliteFileTable table,
            string fileName,
            byte[] fileContents,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Creates a new <see cref="SqliteConnection" /> and opens it for usage.
        /// </summary>
        /// <remarks>
        ///     Ensure that the connection is disposed of after use.
        /// </remarks>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The opened connection.</returns>
        Task<SqliteConnection> GetDatabaseConnectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Creates the SQLite database if it does not exist and apply migrations.
        /// </summary>
        /// <param name="sqliteDatabaseFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<bool>> CreateSqliteDatabaseAsync(string sqliteDatabaseFile,
            CancellationToken cancellationToken = default);

        Task<Result<bool>> ApplyDbupMigrationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Runs a SQL script on the database.
        /// </summary>
        /// <param name="sqlFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<bool>> RunSqlOnDatabaseAsync(string sqlFile, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Updates appsettings.json with a new database.
        /// </summary>
        /// <param name="sqliteDatabaseName"></param>
        /// <param name="appSettingsFile">Optional. If missing, then the default database name will be used.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<bool>> UpdateSqliteDatabasePathAsync(string sqliteDatabaseName,
            string? appSettingsFile = null,
            CancellationToken cancellationToken = default);
    }
}
