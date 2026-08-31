using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    public abstract class MlrbFirearmsCommandBase : MlrbSqliteCommandBase
    {
        protected IFirearmsService FirearmsService { get;  }
         protected MlrbFirearmsCommandBase(ILogger logger, ICliDisplay display,
            ISqliteHelper sqliteHelper,
            IFirearmsService firearmsService) : base(logger, display, sqliteHelper)
        {
            ArgumentNullException.ThrowIfNull(firearmsService);

            FirearmsService = firearmsService;
        }

    }
}