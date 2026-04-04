using System.Threading.Tasks;
using MyLittleRangeBook.Database.Sqlite;
using MySimpleRangeLog.Models;

namespace MySimpleRangeLog.Services
{
    public class FirearmsService : IFirearmsService
    {
        readonly ISqliteHelper _sqliteHelper;

        public FirearmsService(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }

        public async Task<bool> SaveFirearmAsync(Firearm firearm)
        {
            return await firearm.SaveAsync(await _sqliteHelper.OpenSqliteConnectionToFileAsync());
        }

        public async Task<bool> DeleteFirearmEvent(Firearm firearm)
        {
            return await firearm.DeleteAsync(await _sqliteHelper.OpenSqliteConnectionToFileAsync());
        }
    }
}
