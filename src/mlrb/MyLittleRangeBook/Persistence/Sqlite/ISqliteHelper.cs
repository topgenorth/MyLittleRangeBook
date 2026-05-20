using FluentResults;
using Microsoft.Data.Sqlite;

namespace MyLittleRangeBook.Persistence.Sqlite
{
    public interface ISqliteHelper
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
        ///     Applies the required database migrations to the SQLite database using DbUp.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A token to observe while waiting for the task to complete, allowing the operation to be canceled.
        /// </param>
        /// <returns>
        ///     A result indicating the success or failure of applying the migrations. The result contains a boolean,
        ///     where true indicates the migrations were successfully applied, and false indicates a failure.
        /// </returns>
        Task<Result<bool>> ApplyDbupMigrationsAsync(CancellationToken cancellationToken = default);


        /// <summary>
        ///     Runs a SQL script on the database.
        /// </summary>
        /// <param name="sqlFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<bool>> RunSqlOnDatabaseAsync(string sqlFile, CancellationToken cancellationToken = default);
    }
}
