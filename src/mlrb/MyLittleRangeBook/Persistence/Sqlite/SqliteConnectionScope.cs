using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.Persistence.Sqlite
{
    /// <summary>
    ///     Represents a scoped connection to a SQLite database.
    ///     Ensures proper disposal of the SQLite connection and optimizations during disposal.
    /// </summary>
    public sealed class ScopedSqliteConnection : IAsyncDisposable
    {
        SqliteConnection? _connection;
        SqliteTransaction? _transaction;
        bool _disposed;

        internal ScopedSqliteConnection(SqliteConnection connection, bool useTransaction = false)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            if (useTransaction)
            {
                _transaction = _connection.BeginTransaction();
            }
        }

        public SqliteConnection Connection => _disposed || _connection is null
            ? throw new ObjectDisposedException(nameof(ScopedSqliteConnection))
            : _connection;

        public SqliteTransaction? Transaction => _transaction;

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_transaction is not null)
            {
                try
                {
                    if (_transaction.Connection is not null)
                    {
                        await _transaction.CommitAsync();
                    }
                }
                finally
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }

            SqliteConnection? connection = _connection;
            _connection = null;

            if (connection is null)
            {
                return;
            }

            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.ExecuteAsync("PRAGMA optimize;");
                }
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
    }
}
