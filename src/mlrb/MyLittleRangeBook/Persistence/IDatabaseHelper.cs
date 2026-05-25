using System.Data;

namespace MyLittleRangeBook.Persistence
{
    public interface IDatabaseHelper
    {
        public Task<IDbConnection> GetDatabaseConnectionAsync(CancellationToken cancellationToken);
    }
}
