using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using static MyLittleRangeBook.CLI.ReturnCodes;
using static MyLittleRangeBook.Database.Sqlite.SqliteHelperExtensions;


namespace MyLittleRangeBook.CLI.Console
{
    [RegisterCommands("rangeevent")]
    public class SimpleRangeEventCommands : MlrbCommandBase
    {
        readonly ISimpleRangeEventListPrinter _printer;
        readonly ISimpleRangeEventRepository _repo;

        public SimpleRangeEventCommands(ILogger logger,
            ICliDisplay cliDisplay,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeEventRepository repo,
            ISimpleRangeEventListPrinter printer) : base(logger, cliDisplay)
        {
            _repo = repo;
            _printer = printer;
        }

        /// <summary>
        ///     Display a single range event.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quiet">If set to true, then less verbose, single line output.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("show")]
        [UsedImplicitly]
        public async Task<int> DisplayOneRangeEvent(string id, bool quiet = false, CancellationToken ct = default)
        {
            if (quiet)
            {
                CliDisplay.PrintCommandHeader();
            }
            else
            {
                CliDisplay.PrintCommandHeader($"Show range event {id}");
            }

            int returnCode;

            try
            {
                Result<SimpleRangeEvent> result = await _repo.GetAsync(id, ct).ConfigureAwait(false);

                if (result.IsFailed)
                {
                    Logger.Warning("Could not find simple range event {id} for display.", id);
                    CliDisplay.PrintFailure("Could not find the request range event.");
                    returnCode = FAILURE;
                }
                else
                {
                    var p = new SimpleRangeEventPrinter2();
                    p.Print(CliDisplay.Console, result.Value!, quiet);
                    CliDisplay.PrintSuccess("Range event displayed successfully.");
                    returnCode = SUCCESS;
                }
            }
            catch (Exception e)
            {
                returnCode = FAILURE;
                Logger.Error(e, e.Message);
                CliDisplay.PrintFailure("An error occurred while displaying the range event.");
            }

#if DEBUG
            System.Console.ReadKey();
#endif
            return returnCode;
        }

        /// <summary>
        ///     List all the range events in the database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list")]
        [UsedImplicitly]
        public async Task<int> ListRangeEvents(CancellationToken cancellationToken)
        {
            CliDisplay.PrintCommandHeader("List range events.");
            Result<IEnumerable<SimpleRangeEvent>> rangeEvents = await _repo.GetSimpleRangeEventsAsync(cancellationToken)
                .ConfigureAwait(false);
            if (rangeEvents.IsFailed)
            {
                CliDisplay.PrintFailure("Could not retrieve the list.");
                Logger.Warning("Failed to retrieve list from database.");

                return FAILURE;
            }

            await _printer.Start().ConfigureAwait(false);
            foreach (SimpleRangeEvent sre in rangeEvents.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Warning("Operation cancelled by user.");
                    CliDisplay.PrintFailure("Operation cancelled.");
                    await _printer.Finish().ConfigureAwait(false);

                    return COMMAND_CANCELLED;
                }

                await _printer.AddRow(sre).ConfigureAwait(false);
            }

            await _printer.Finish().ConfigureAwait(false);

#if DEBUG
            // [TO20260507] Need this when testing in Rider.  Without it the console window closes too fast.
            System.Console.ReadKey();
#endif

            return SUCCESS;
        }
    }
}
