using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Gui.Services;

namespace MyLittleRangeBook.Gui.Database.Sqlite
{
    /// <summary>
    ///     Desktop-specific implementation of the database service.
    ///     Uses the OS-specific local application data folder (e.g., %LOCALAPPDATA% on Windows,
    ///     ~/.local/share on Linux, ~/Library/Application Support on macOS) to store the SQLite database.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class SqliteDbService : ISqliteHelper
    {
        readonly ISqliteHelper _sqliteHelper;

        public SqliteDbService(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }


        /// <summary>
        ///     Gets the absolute path to the SQLite database file on the local machine.
        ///     Creates the app-specific subdirectory in the local app data folder if it doesn't exist.
        /// </summary>
        /// <returns>The full path to the database file (e.g., C:\Users\Name\AppData\Local\SimpleRangeLog\fileName.db on Windows)</returns>
        public string GetConnectionString()
        {
            return _sqliteHelper.GetSqliteConnectionString();
        }


        public string GetSqliteConnectionString()
        {
            return _sqliteHelper.GetSqliteConnectionString();
        }

        public string GetSqliteDatabaseName()
        {
            return _sqliteHelper.GetSqliteDatabaseName();
        }

        public bool DoesDatabaseExist()
        {
            return _sqliteHelper.DoesDatabaseExist();
        }

        public Task<SqliteConnection> OpenSqliteConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            return _sqliteHelper.OpenSqliteConnectionAsync(connectionString, cancellationToken);
        }

        public Task<SqliteConnection> OpenSqliteConnectionToFileAsync(string? file = null, CancellationToken cancellationToken = default)
        {
            return _sqliteHelper.OpenSqliteConnectionToFileAsync(file, cancellationToken);
        }
    }
}
