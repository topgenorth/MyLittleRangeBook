using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;

namespace MyLittleRangeBook.Sqlite
{
    public abstract class SqliteConnectionTestBase
    {
        readonly string _sqliteDbFileName = Path.GetTempFileName();

        protected SqliteConnectionTestBase()
        {
            Logger = Substitute.For<ILogger>();
            SqliteHelper = new SqliteHelper(Logger, $"Data Source={_sqliteDbFileName}");
        }

        protected SqliteHelper SqliteHelper { get; }

        protected ILogger Logger { get; }

        ~SqliteConnectionTestBase()
        {
            if (File.Exists(_sqliteDbFileName))
            {
                File.Delete(_sqliteDbFileName);
            }
        }

        protected async Task<SqliteConnection> GetSqliteConnectionAsync()
        {
            SqliteConnection? conn = null;
            try
            {
                Result<bool> migrationResult = await SqliteHelper.ApplyDbupMigrationsAsync();
                if (migrationResult.IsFailed)
                {
                    throw new Exception("Migration failed");
                }

                conn = await SqliteHelper.GetDatabaseConnectionAsync();

                return conn;
            }
            catch
            {
                if (conn != null)
                {
                    await conn.DisposeAsync();
                }

                throw;
            }
        }
    }
}
