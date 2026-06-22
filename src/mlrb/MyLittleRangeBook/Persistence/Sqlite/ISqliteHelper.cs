using Microsoft.Data.Sqlite;

namespace MyLittleRangeBook.Persistence.Sqlite
{
    public interface ISqliteHelper : ISqliteDatabaseInitializer
    {
        /// <summary>
        ///     Gets the path to the SQLite database file.
        ///     This property provides the location of the database file being used
        ///     by the implementation for database operations.
        /// </summary>
        public string DatabaseFile { get; }

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
        ///     Creates a new <see cref="SqliteConnection" /> and opens it for usage.
        /// </summary>
        /// <remarks>
        ///     Ensure that the connection is disposed of after use.
        /// </remarks>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="useTransaction">If true, a new <see cref="SqliteTransaction" /> will be created and exposed via the <see cref="ScopedSqliteConnection.Transaction" /> property.</param>
        /// <returns>The opened connection.</returns>
        Task<ScopedSqliteConnection> GetScopedDatabaseConnectionAsync(CancellationToken cancellationToken = default, bool useTransaction = false);

        /// <summary>
        ///     Runs a SQL script on the database.
        /// </summary>
        /// <param name="sqlFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> RunSqlOnDatabaseAsync(string sqlFile, CancellationToken cancellationToken = default);

        /// <summary>
        /// Optimizes the SQLite database for improved performance by running the "PRAGMA optimize;" command.
        /// </summary>
        /// <param name="connection">
        /// An open <see cref="SqliteConnection" /> instance to the database on which the optimization will be applied.
        /// Ensure the connection is valid and properly initialized before invoking this method.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OptimizeAsync(SqliteConnection connection);

        /// <summary>
        /// Performs an integrity check on the SQLite database using the PRAGMA integrity_check command.
        /// </summary>
        /// <param name="connection">The open SQLite connection used to perform the integrity check.</param>
        /// <returns>
        /// A read-only list of strings containing the results of the integrity check.
        /// If the database is consistent, the result will contain only "ok".
        /// </returns>
        Task<IReadOnlyList<string>> IntegrityCheckAsync(SqliteConnection connection);

        /// <summary>
        /// Performs a write-ahead logging (WAL) checkpoint operation on the SQLite database.
        /// </summary>
        /// <remarks>
        /// This method issues a checkpoint command to the SQLite database, ensuring that all
        /// changes in the WAL file are merged into the main database file. The WAL file is truncated as part of
        /// the operation to reclaim disk space.
        /// </remarks>
        /// <param name="connection">
        /// The active <see cref="SqliteConnection" /> instance to execute the checkpoint command on.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        Task CheckpointWalAsync(SqliteConnection connection);

        Task VacuumAync(SqliteConnection connection);
    }
}
