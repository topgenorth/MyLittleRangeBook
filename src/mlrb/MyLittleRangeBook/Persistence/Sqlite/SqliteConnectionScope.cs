using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace MyLittleRangeBook.Persistence.Sqlite
{
    /// <summary>
    ///     Represents a scoped connection to a SQLite database.
    ///     Ensures proper disposal of the SQLite connection and optimizations during disposal.
    /// </summary>
    public sealed class ScopedSqliteConnection : IAsyncDisposable
    {
        SqliteConnection? _connection;
        bool _disposed;

        internal ScopedSqliteConnection(SqliteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public SqliteConnection Connection => _disposed || _connection is null
            ? throw new ObjectDisposedException(nameof(ScopedSqliteConnection))
            : _connection;

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

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
