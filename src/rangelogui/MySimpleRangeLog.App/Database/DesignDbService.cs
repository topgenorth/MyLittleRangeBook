using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MySimpleRangeLog.Services;

namespace MySimpleRangeLog.Database
{
    public class DesignDbService : IDatabaseService
    {
        /// <inheritdoc />
        public string GetConnectionString()
        {
            var cb = new SqliteConnectionStringBuilder
            {
                DataSource = ":memory:", Mode = SqliteOpenMode.ReadWriteCreate
            };

            // For the designer, we use an in-memory DB.
            // See: https://www.sqlite.org/inmemorydb.html 
            return cb.ConnectionString;
        }

        /// <inheritdoc />
        public Task SaveAsync()
        {
            // The designer will not save anything.
            return Task.CompletedTask;
        }
    }
}
