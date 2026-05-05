using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;

namespace MyLittleRangeBook.Sqlite
{
    /// <summary>
    ///     Base class for SQLite stuff.  Creates a temporary SQLite database on disk for use in tests, and runs migrations
    ///     creating an empty DB.  Deletes the database once the test is done.
    /// </summary>
    public abstract class SqliteConnectionTestBase
    {
        readonly string _sqliteDbFileName = Path.GetTempFileName();

        protected SqliteConnectionTestBase()
        {
            Logger = Substitute.For<ILogger>();
            SqliteHelper = new SqliteHelper(Logger, $"Data Source={_sqliteDbFileName}");
        }

        /// <summary>
        ///     An instance of the SqliteHelper class for interacting with the temporary SQLite database.
        /// </summary>
        protected SqliteHelper SqliteHelper { get; }

        /// <summary>
        ///     A mocked ILogger.
        /// </summary>
        protected ILogger Logger { get; }

        ~SqliteConnectionTestBase()
        {
            if (File.Exists(_sqliteDbFileName))
            {
                File.Delete(_sqliteDbFileName);
            }
        }

        /// <summary>
        ///     Opens a connection to a temporary database, and runs the Migrations on it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
