using System.Data;
using MyLittleRangeBook.Database;
using Npgsql;

namespace MyLittleRangeBook.PgSQL
{
    public class PostgresHelper : IDatabaseHelper, IPostgresHelper
    {
        readonly string _connectionString;

        public PostgresHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        async Task<IDbConnection> IDatabaseHelper.GetDatabaseConnectionAsync(CancellationToken cancellationToken)
        {
            return await GetDatabaseConnectionAsync(cancellationToken);
        }

        public async Task<NpgsqlConnection> GetDatabaseConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            return connection;
        }
    }
}
