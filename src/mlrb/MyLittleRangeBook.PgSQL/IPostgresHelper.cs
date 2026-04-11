using System.Data;
using Npgsql;

namespace MyLittleRangeBook.PgSQL
{
    public interface IPostgresHelper
    {
        Task<NpgsqlConnection> GetDatabaseConnectionAsync(CancellationToken cancellationToken = default);
    }
}
