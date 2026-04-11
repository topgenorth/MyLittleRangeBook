using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.GUI.Models;

namespace MyLittleRangeBook.GUI.Services
{
    public class SimpleRangeEventService : ISimpleRangeEventService
    {
        readonly ISqliteHelper _sqliteHelper;

        public SimpleRangeEventService(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }

        public async Task<bool> SaveRangeEventAsync(SimpleRangeEvent rangeEvent, CancellationToken cancellationToken)
        {
            await using SqliteConnection connection = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

            return await rangeEvent.SaveAsync(connection, cancellationToken);
        }

        public async Task<bool> DeleteRangeEvent(SimpleRangeEvent rangeEvent, CancellationToken cancellationToken)
        {
            await using SqliteConnection connection = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

            return await rangeEvent.DeleteAsync(connection, cancellationToken);
        }
    }
}
