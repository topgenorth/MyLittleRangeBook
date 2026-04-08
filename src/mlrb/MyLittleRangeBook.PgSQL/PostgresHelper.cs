using System.Data;
using MyLittleRangeBook.Database;

namespace MyLittleRangeBook.PgSQL
{
    public class PostgresHelper : IDatabaseHelper
    {
        public Task<IDbConnection> GetDatabaseConnectionAsync(CancellationToken cancellationToken=default)
        {
            throw new NotImplementedException();
        }
    }
}
