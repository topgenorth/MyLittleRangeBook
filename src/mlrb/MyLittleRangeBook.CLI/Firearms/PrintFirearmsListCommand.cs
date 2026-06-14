using ConsoleAppFramework;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    [RegisterCommands("firearms")]
    public class PrintFirearmsListCommand : MlrbFirearmsCommandBase
    {
        private readonly FirearmsTablePrinter _printer = new();


        public PrintFirearmsListCommand(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper,
            IFirearmsService firearmsService, IFirearmAggregateRepository firearmAggregateRepo) : base(logger, display,
            sqliteHelper, firearmsService, firearmAggregateRepo)
        {
        }

        /// <summary>
        ///     List all the active firearms.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list")]
        [UsedImplicitly]
        public async Task<int> PrintFirearmsToConsole(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("List firearms");

            await using var scopedConn = await SqliteHelper
                .GetScopedDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            var ctx = new DapperCommandContext(scopedConn.Connection, null, cancellationToken);

            var firearms = await FirearmsService
                .GetFirearmsAsync(ctx)
                .ConfigureAwait(false);

            if (firearms.IsFailed)
            {
                Logger.Warning("Failed to retrieve firearms.");
                CliDisplay.PrintFailure("Failed to retrieve list of firearms.");

                return ReturnCodes.FAILURE;
            }

            if (!firearms.Value.Any())
            {
                CliDisplay.PrintWarning("No firearms to list.");

                return ReturnCodes.SUCCESS;
            }

            _printer.SetFirearms(firearms.Value).Print(AnsiConsole.Console);
            CliDisplay.PrintSuccess("Firearms listed.");

            return ReturnCodes.SUCCESS;
        }
    }
}