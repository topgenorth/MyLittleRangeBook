using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.ReturnCodes;

namespace MyLittleRangeBook.Database
{
    /// <summary>
    ///     This class provides functionality for managing SQLite database migrations.
    /// </summary>
    // [RegisterCommands("db")]
    [UsedImplicitly]
    public class SqliteMigrationCommands : MlrbCommandBase
    {
        const    string        MIGRATIONS_SQL = "SELECT * FROM SchemaVersions ORDER BY Applied DESC";
        readonly ISqliteHelper _sqliteHelper;

        public SqliteMigrationCommands(ILogger logger, ICliDisplay cliDisplay, ISqliteHelper sqliteHelper) :
            base(logger, cliDisplay) =>
            _sqliteHelper = sqliteHelper;

        /// <summary>
        ///     Will return all the migrations that have been applied to the database.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("versions")]
        [UsedImplicitly]
        public async Task<int> DisplayMigrationVersionsToConsoleAsync(CancellationToken ct = default)
        {
            CliDisplay.PrintCommandHeader("Show migration versions");
            await RunMigrations(ct).ConfigureAwait(false);

            try
            {
                await using SqliteConnection connection = await _sqliteHelper
                                                               .GetDatabaseConnectionAsync(ct)
                                                               .ConfigureAwait(false);
                await using SqliteCommand    cmd = new(MIGRATIONS_SQL, connection);
                await using SqliteDataReader rdr = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

                Table table = new Table().Expand().BorderColor(Color.White);

                table.AddColumn("Row ID", col => col.Width(6).Centered());
                table.AddColumn("Script Name");
                table.AddColumn("Date Applied");

                while (await rdr.ReadAsync(ct).ConfigureAwait(false))
                {
                    string? schemaVersionId = rdr.IsDBNull(0) ? string.Empty : rdr.GetValue(0).ToString();
                    string  scriptName      = rdr.IsDBNull(1) ? string.Empty : rdr.GetString(1);
                    string applied =
                        rdr.IsDBNull(2) ? string.Empty : rdr.GetValue(2).ToString() ?? string.Empty;
                    table.AddRow(schemaVersionId!, scriptName, applied);
                }

                CliDisplay.Console.Write(table);
                CliDisplay.PrintSuccess("Migration Versions listed.");

                return SUCCESS;
            }
            catch (SqliteException sqlex)
            {
                if ("SQLite Error 1: 'no such table: SchemaVersions'.".Equals(sqlex.Message))
                {
                    CliDisplay.PrintSuccess("No DBup migrations have been applied.");
                    Logger.Verbose(sqlex, "Failed to display migrations.");
                    return SUCCESS;
                }

                CliDisplay.Console.WriteException(sqlex);
                Logger.Error(sqlex, "Failed to display migrations.");

                return SQL_FAILED_TO_APPLY_MIGRATIONS;
            }
            catch (Exception e)
            {
                CliDisplay.Console.WriteException(e);
                Logger.Error(e, "Failed to display migrations.");

                return SQL_FAILED_TO_APPLY_MIGRATIONS;
            }
        }

        async Task RunMigrations(CancellationToken ct)
        {
            Result<bool> migrations = await _sqliteHelper.ApplyDbupMigrationsAsync(ct).ConfigureAwait(false);
            if (migrations.IsFailed)
            {
                Logger.Warning("There was a problem running the migrations: {Error}",
                               migrations.Errors.FirstOrDefault()?.Message);
            }
        }


    }
}