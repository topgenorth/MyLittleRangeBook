using System.Threading.Tasks;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Gui.Models;

namespace MyLittleRangeBook.Gui.Services
{
    public class SimpleRangeEventService : ISimpleRangeEventService
    {
        readonly ISqliteHelper _sqliteHelper;

        public SimpleRangeEventService(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }

        public async Task<bool> SaveRangeEventAsync(SimpleRangeEvent rangeEvent)
        {
            await using var connection = await _sqliteHelper.OpenSqliteConnectionToFileAsync();
            return await rangeEvent.SaveAsync(connection);
        }

        public async Task<bool> DeleteRangeEvent(SimpleRangeEvent rangeEvent)
        {
            await using var connection = await _sqliteHelper.OpenSqliteConnectionToFileAsync();
            return await rangeEvent.DeleteAsync(connection);
        }
    }
}
