using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MySimpleRangeLog.Helper;
using MySimpleRangeLog.Services;
using Serilog;

namespace MySimpleRangeLog.Database
{
    /// <summary>
    ///     Desktop-specific implementation of the database service.
    ///     Uses the OS-specific local application data folder (e.g., %LOCALAPPDATA% on Windows,
    ///     ~/.local/share on Linux, ~/Library/Application Support on macOS) to store the SQLite database.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class SQLiteDbService : IDatabaseService
    {
        /// <summary>
        ///     Name of the database for "production".
        /// </summary>
        public const string DATABASE_NAME = "simplerangelog.db";

        /// <summary>
        ///     Gets the absolute path to the SQLite database file on the local machine.
        ///     Creates the app-specific subdirectory in the local app data folder if it doesn't exist.
        /// </summary>
        /// <returns>The full path to the database file (e.g., C:\Users\Name\AppData\Local\SimpleRangeLog\fileName.db on Windows)</returns>
        public string GetConnectionString()
        {
            var settingsDirectory = JsonSettingsFileStorageService.SettingsDirectory;

            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
                Log.Logger.Verbose("Creating folder for database {SettingsDirectory}.", settingsDirectory);
            }

            string dbPath;
            if (EnvironmentHelper.IsProduction)
            {
                dbPath = Path.Combine(settingsDirectory, DATABASE_NAME);
            }
            else
            {
                var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;
                var dbName = $"simplerangelog-{env!.ToLower()}.db";
                dbPath = Path.Combine(settingsDirectory, dbName);
            }


            var cb = new SqliteConnectionStringBuilder { DataSource = dbPath, Mode = SqliteOpenMode.ReadWriteCreate };


            return cb.ConnectionString;
        }

        /// <summary>
        ///     Saves database changes.
        ///     Currently, a no-op implementation as SQLite operations are handled directly
        ///     by the database connection and don't require explicit save operations.
        /// </summary>
        public Task SaveAsync()
        {
            return Task.CompletedTask;
        }
    }
}
