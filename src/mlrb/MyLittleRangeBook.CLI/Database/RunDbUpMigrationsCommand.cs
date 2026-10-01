using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.ReturnCodes;

namespace MyLittleRangeBook.Database
{
    [RegisterCommands("db")]
    [UsedImplicitly]
    public class RunDbUpMigrationsCommand : MlrbSqliteCommandBase
    {
        public RunDbUpMigrationsCommand(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper) :
            base(logger, display, sqliteHelper) { }


        async Task RunMigrations(CancellationToken ct)
        {
            Result<bool> migrations = await SqliteHelper.ApplyDbupMigrationsAsync(ct).ConfigureAwait(false);
            if (migrations.IsFailed)
            {
                Logger.Warning("There was a problem running the migrations: {Error}",
                               migrations.Errors.FirstOrDefault()?.Message);
            }
        }

        /// <summary>
        ///     Ensures that all database schema migrations have been applied.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("migrate")]
        [UsedImplicitly]
        public async Task<int> MigrateSchemaAsync(CancellationToken ct = default)
        {
            CliDisplay.PrintCommandHeader("Applying Migrations");
            int returnCode = -1;
            if (!File.Exists(SqliteHelper.DatabaseFile))
            {
                Logger.Warning("SQLite database {file} not found.", SqliteHelper.DatabaseFile);
                CliDisplay.PrintFailure($"Could not find the SQLite database '{SqliteHelper.DatabaseFile}'.");

                returnCode = SQL_FAILED_TO_APPLY_MIGRATIONS;
                goto ExitMethod;
            }

            Result<bool> migrationResult = await SqliteHelper.ApplyDbupMigrationsAsync(ct).ConfigureAwait(false);
            if (migrationResult.IsSuccess)
            {
                Logger.Information("Migrations applied.");
                CliDisplay.PrintSuccess("Migrations applied.");

                returnCode = SUCCESS;
                goto ExitMethod;
            }

            string? msg = migrationResult.Errors.FirstOrDefault()?.Message;
            Logger.Error("Failed to apply migrations: {Error}", msg);
            CliDisplay.PrintFailure($"Failed to apply migrations: {msg}");

            returnCode = SQL_FAILED_TO_APPLY_MIGRATIONS;

            ExitMethod:
            if (returnCode != SUCCESS)
            {
                PressEnterToContinue();
            }

            return returnCode;
        }
    }
}