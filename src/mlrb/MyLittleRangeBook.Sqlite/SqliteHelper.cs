using System.Data;
using NanoidDotNet;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///     A helper class for managing SQLite database connections, initialization, and configuration.
    ///     Provides methods for setting up the database environment, getting connections, and generating connection strings.
    /// </summary>
    public class SqliteHelper : ISqliteHelper, IDatabaseHelper
    {
        readonly string _connectionString;

        public SqliteHelper(string connectionString)
        {
            var builder = new SqliteConnectionStringBuilder(connectionString) { Mode = SqliteOpenMode.ReadWriteCreate };
            _connectionString = builder.ConnectionString;
        }

        /// <summary>
        ///     Creates a new <see cref="SqliteConnection" /> and opens it for usage.
        /// </summary>
        /// <remarks>
        ///     Ensure that the connection is disposed of after use.
        /// </remarks>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The opened connection.</returns>
        public async Task<SqliteConnection> GetDatabaseConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SqliteConnection(_connectionString);
            connection.CreateFunction("nanoid", () => Nanoid.Generate());
            connection.CreateFunction("utcnow", () => DateTimeOffset.UtcNow.ToString("O"));
            await connection.OpenAsync(cancellationToken);

            return connection;
        }

        async Task<IDbConnection> IDatabaseHelper.GetDatabaseConnectionAsync(CancellationToken cancellationToken=default)
        {
            return await GetDatabaseConnectionAsync(cancellationToken);

        }
    }
}
