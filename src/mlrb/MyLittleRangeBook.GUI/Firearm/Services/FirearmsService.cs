using System.Threading;
using System.Threading.Tasks;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.GUI.Models;

namespace MyLittleRangeBook.GUI.Services
{
    public class FirearmsService : IFirearmsService
    {
        readonly ISqliteHelper _sqliteHelper;

        public FirearmsService(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }

        public async Task<bool> SaveFirearmAsync(Firearm firearm, CancellationToken cancellationToken)
        {
            return await firearm.SaveAsync(await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken),
                cancellationToken);
        }

        public async Task<bool> DeleteFirearmEvent(Firearm firearm, CancellationToken cancellationToken)
        {
            return await firearm.DeleteAsync(await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken));
        }
    }
}
