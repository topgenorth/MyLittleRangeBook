using ConsoleAppFramework;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Database
{
    [RegisterCommands("db")]
    [UsedImplicitly]
    public class SqliteDbMaintenanceCommand : MlrbSqliteCommandBase
    {
        public SqliteDbMaintenanceCommand(ILogger logger, ICliDisplay cliDisplay, ISqliteHelper sqliteHelper) :
            base(logger, cliDisplay, sqliteHelper) { }

        [Command("maintenance")]
        [UsedImplicitly]
        public async Task<int> SqliteMainteance(CancellationToken cancellationToken)
        {
            CliDisplay.PrintCommandHeader("SQLite Maintenance.");
            await using ScopedSqliteConnection scope =
                await SqliteHelper.GetScopedDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);

            CliDisplay.PrintInfo("WAL checkpoint");
            await SqliteHelper.CheckpointWalAsync(scope.Connection).ConfigureAwait(false);

            CliDisplay.PrintInfo("Vacuum ");
            await SqliteHelper.VacuumAync(scope.Connection).ConfigureAwait(false);

            CliDisplay.PrintInfo("Integrity check");
            IReadOnlyList<string> x = await SqliteHelper.IntegrityCheckAsync(scope.Connection).ConfigureAwait(false);
            Logger.Information("Database integrity check passed with result: {result}", x);

            CliDisplay.PrintSuccess("SQLite maintenance finished.");
            return ReturnCodes.SUCCESS;
        }
    }
}