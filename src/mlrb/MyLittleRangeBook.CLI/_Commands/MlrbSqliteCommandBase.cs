using MyLittleRangeBook.Database.Sqlite;

namespace MyLittleRangeBook.CLI
{
    public abstract class MlrbSqliteCommandBase : MlrbCommandBase
    {
        protected readonly ISqliteHelper SqliteHelper;

        protected MlrbSqliteCommandBase(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper): base(logger, display)
        {
            SqliteHelper = sqliteHelper;
        }
    }
}
