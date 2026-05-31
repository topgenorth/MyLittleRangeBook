using System.Data.Common;
using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    [RegisterCommands("firearms")]
    public class FirearmCommands : MlrbCommandBase
    {
        readonly IFirearmsService _firearmsService;
        readonly FirearmsTablePrinter _printer;
        readonly ISqliteHelper _sqliteHelper;

        public FirearmCommands(ILogger logger,
            ICliDisplay cliDisplay,
            [FromKeyedServices(SqliteHelperExtensions.DI_KEYS_SQLITE)] IFirearmsService firearmsService,
            ISqliteHelper sqliteHelper) : base(logger, cliDisplay)
        {
            _firearmsService = firearmsService;
            _sqliteHelper = sqliteHelper;
            _printer = new FirearmsTablePrinter();
        }


        /// <summary>
        /// This is a maintenance task. It will update the Firearms table based on what is in the SimpleRangeEvents table.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("update-from-rangeevents")]
        [UsedImplicitly]
        public async Task<int> UpdateFirearmsFromRangeEvents(CancellationToken cancellationToken = default)
        {
            AnsiConsole.Console.PrintAppInfo();
            AnsiConsole.Console.WriteLine("Updating firearms from range events...");


            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            await using DbTransaction trans = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var ctx = new DapperCommandContext(conn, trans, cancellationToken);
                Result r = await _firearmsService.UpdateFirearmsFromRangeEventsAsync(ctx).ConfigureAwait(false);

                if (r.IsFailed)
                {
                    CliDisplay.PrintFailure("Failed to update firearms from range events.");
                }
                else
                {
                    CliDisplay.PrintSuccess("Updated firearms from range events.");
                }

                foreach (IReason reason in r.Reasons)
                {
                    CliDisplay.Console.WriteLine($"* {reason.Message}");
                }

                await trans.CommitAsync(cancellationToken).ConfigureAwait(false);
                PressAnyKeyToContinue();
                return ReturnCodes.SUCCESS;
            }
            catch (Exception ex)
            {
                CliDisplay.PrintFailure("something bad happened.");
                Logger.Error(ex, "Failed to update firearms from range events");
                await trans.RollbackAsync(cancellationToken).ConfigureAwait(false);

                PressAnyKeyToContinue();
                return ReturnCodes.FAILURE;
            }
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
            AnsiConsole.Console.PrintAppInfo();
            AnsiConsole.Console.WriteLine("Retrieving firearms...");

            await using SqliteConnection conn = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            Result<IEnumerable<Firearm>> firearms = await _firearmsService
                .GetFirearmsAsync(conn, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (firearms.IsFailed)
            {
                Logger.Warning("Failed to retrieve firearms.");
                AnsiConsole.Console.PrintProblem("Failed to retrieve firearms.");

                return ReturnCodes.FAILURE;
            }

            if (!firearms.Value.Any())
            {
                AnsiConsole.Console.PrintWarning("No firearms found.");

                return ReturnCodes.SUCCESS;
            }

            _printer.SetFirearms(firearms.Value).Print(AnsiConsole.Console);
            AnsiConsole.Console.PrintSuccess("Firearms retrieved.");

            return ReturnCodes.SUCCESS;
        }
    }
}
