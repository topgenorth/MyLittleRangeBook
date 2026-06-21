using System.Data;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Persistence
{
    /// <summary>
    ///    Provides a context for database operations, encapsulating the necessary components such as the database connection.
    /// </summary>
    /// <param name="Connection">The database connection.</param>
    /// <param name="Transaction">The database transaction.</param>
    /// <param name="CancellationToken">The cancellation token.</param>
    /// <param name="Arguments">The arguments for the command.</param>
    public record DapperCommandContext(IDbConnection Connection, IDbTransaction? Transaction = null, CancellationToken CancellationToken = default, object? Arguments = null)
    {
        public DapperCommandContext(ScopedSqliteConnection scopedConnection, IDbTransaction? Transaction = null, CancellationToken CancellationToken = default):
            this(scopedConnection.Connection, Transaction, CancellationToken)
        {

        }

        /// <summary>
        /// Represents a command context used for database interactions within the application,
        /// containing a database connection, an optional transaction, a cancellation token,
        /// and optional additional arguments.
        /// </summary>
        /// <param name="scopedSqliteConnection"></param>
        /// <param name="CancellationToken">A token to monitor for cancellation requests during execution of commands.</param>
        /// <param name="Arguments">Optional additional arguments required for command execution.</param>
        public DapperCommandContext(ScopedSqliteConnection scopedSqliteConnection,
                                    CancellationToken      CancellationToken = default)
            : this(scopedSqliteConnection.Connection, scopedSqliteConnection.Transaction, CancellationToken)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="DapperCommandContext"/> asynchronously.
        /// </summary>
        /// <param name="sqliteHelper">The SQLite helper used to acquire a scoped database connection.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation, if necessary.</param>
        /// <returns>An instance of <see cref="DapperCommandContext"/> configured with the obtained database connection.</returns>
        public static async Task<DapperCommandContext> NewAsync(ISqliteHelper     sqliteHelper,
                                                                CancellationToken cancellationToken = default)
        {
            var scopedConnection = await sqliteHelper
                                        .GetScopedDatabaseConnectionAsync(cancellationToken)
                                        .ConfigureAwait(false);
            var ctx = new DapperCommandContext(scopedConnection, cancellationToken);

            return ctx;
        }


    }
}
