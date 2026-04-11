using System.Data;

namespace MyLittleRangeBook.Database
{
    public interface IDatabaseHelper
    {
        public Task<IDbConnection> GetDatabaseConnectionAsync(CancellationToken cancellationToken);
    }
}
