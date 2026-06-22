using System.Data;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Persistence
{
    using System.Data.Common;

    /// <summary>
    ///    Provides a context for database operations, encapsulating the necessary components such as the database connection.
    /// </summary>
    /// <param name="Connection">The database connection.</param>
    /// <param name="Transaction">The database transaction.</param>
    /// <param name="CancellationToken">The cancellation token.</param>
    /// <param name="Arguments">The arguments for the command.</param>
    public record DapperCommandContext(SqliteConnection Connection, IDbTransaction? Transaction = null, CancellationToken CancellationToken = default, object? Arguments = null, IAsyncDisposable? Scope = null) : IDisposable, IAsyncDisposable
    {
        private bool _wasRolledBack;

        public DapperCommandContext(ScopedSqliteConnection scopedConnection, IDbTransaction? Transaction = null, CancellationToken CancellationToken = default):
            this(scopedConnection.Connection, Transaction, CancellationToken, Scope: scopedConnection)
        {

        }

        /// <summary>
        /// Represents a command context used for database interactions within the application,
        /// containing a database connection, an optional transaction, a cancellation token,
        /// and optional additional arguments.
        /// </summary>
        /// <param name="scopedSqliteConnection"></param>
        /// <param name="CancellationToken">A token to monitor for cancellation requests during execution of commands.</param>
        public DapperCommandContext(ScopedSqliteConnection scopedSqliteConnection,
                                    CancellationToken      CancellationToken = default)
            : this(scopedSqliteConnection.Connection, scopedSqliteConnection.Transaction, CancellationToken, Scope: scopedSqliteConnection)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="DapperCommandContext"/> asynchronously.
        /// </summary>
        /// <param name="sqliteHelper">The SQLite helper used to acquire a scoped database connection.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation, if necessary.</param>
        /// <param name="arguments"></param>
        /// <param name="withTransaction"></param>
        /// <returns>An instance of <see cref="DapperCommandContext"/> configured with the obtained database connection.</returns>
        public static async Task<DapperCommandContext> NewAsync(ISqliteHelper     sqliteHelper,
                                                                CancellationToken cancellationToken = default,
                                                                object? arguments =null,
                                                                bool withTransaction = false)
        {
            var scopedConnection = await sqliteHelper
                                        .GetScopedDatabaseConnectionAsync(cancellationToken, useTransaction: withTransaction)
                                        .ConfigureAwait(false);

            var ctx = new DapperCommandContext(scopedConnection, cancellationToken);
            if (arguments is not null)
            {
                return ctx with { Arguments = arguments };
            }

            return ctx;
        }

        public async Task RollbackAsync()
        {
            if (Transaction != null && Transaction.Connection != null)
            {
                if (Transaction is DbTransaction dbTrans)
                {
                    await dbTrans.RollbackAsync(CancellationToken).ConfigureAwait(false);
                }
                else
                {
                    Transaction.Rollback();
                }

                _wasRolledBack = true;
            }
        }

        public void Rollback()
        {
            if (Transaction != null && Transaction.Connection != null)
            {
                Transaction.Rollback();
                _wasRolledBack = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Transaction != null)
            {
                if (Transaction.Connection != null && !_wasRolledBack)
                {
                    if (Transaction is DbTransaction dbTrans)
                    {
                        await dbTrans.CommitAsync(CancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        Transaction.Commit();
                    }
                }

                if (Transaction is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    Transaction.Dispose();
                }
            }

            if (Scope != null)
            {
                await Scope.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            if (Transaction != null)
            {
                if (Transaction.Connection != null && !_wasRolledBack)
                {
                    Transaction.Commit();
                }

                Transaction.Dispose();
            }

            if (Scope != null)
            {
                if (Scope is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                else
                {
                    Scope.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }
            }
        }


    }
}
