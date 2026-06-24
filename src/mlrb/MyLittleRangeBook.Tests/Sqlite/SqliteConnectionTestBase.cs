using System.Diagnostics;
using Dapper;
using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Sqlite
{
    /// <summary>
    ///     Base class for SQLite stuff.  Creates a temporary SQLite database on disk for use in tests, and runs migrations
    ///     creating an empty DB.  Deletes the database once the test is done.
    /// </summary>
    public abstract class SqliteConnectionTestBase
    {
        readonly string _sqliteDbFileName = Path.GetTempFileName();

        static SqliteConnectionTestBase()
        {
            SqliteHelperExtensions.SetSqlite3ProviderAndInit();
            SqlMapper.AddTypeHandler(typeof(DateTimeOffset), new SqliteDateTimeOffsetHandler());
            SqlMapper.AddTypeHandler(typeof(DateTimeOffset?), new SqliteDateTimeOffsetHandler());
            SqlMapper.AddTypeHandler(typeof(MlrbId), new SqliteMlrbIdHandler());
        }

        protected SqliteConnectionTestBase()
        {
            Logger = Substitute.For<ILogger>();
            var tempDb = $"Data Source={_sqliteDbFileName}";
            SqliteHelper = new SqliteHelper(Logger, tempDb);
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
                try
                {
                    File.Delete(_sqliteDbFileName);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to delete {sqliteDbFileName}. ", _sqliteDbFileName);
                }
            }
        }

        protected async Task EnsureDatabaseExistsAsync()
        {
            await using ScopedSqliteConnection conn = await GetSqliteConnectionAsync();
            await conn.Connection.CloseAsync();

            Debug.Assert(File.Exists(_sqliteDbFileName));
        }

        /// <summary>
        ///     Opens a connection to a temporary database, and runs the Migrations on it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected async Task<ScopedSqliteConnection> GetSqliteConnectionAsync()
        {
            ScopedSqliteConnection? conn = null;
            try
            {
                Result<bool> migrationResult = await SqliteHelper.ApplyDbupMigrationsAsync();
                if (migrationResult.IsFailed)
                {
                    throw new Exception("Migration failed");
                }

                conn = await SqliteHelper.GetScopedDatabaseConnectionAsync().ConfigureAwait(false);

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
