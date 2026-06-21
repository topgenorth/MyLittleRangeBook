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


    }
}
