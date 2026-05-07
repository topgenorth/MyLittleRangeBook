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
    public class SimpleRangeEventCommands
    {
        readonly ICliDisplay _cliDisplay;
        readonly ILogger _logger;
        readonly ISimpleRangeEventListPrinter _printer;
        readonly ISimpleRangeEventRepository _repo;

        public SimpleRangeEventCommands(ICliDisplay cliDisplay,
            ILogger logger,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeEventRepository repo,
            ISimpleRangeEventListPrinter printer)
        {
            _cliDisplay = cliDisplay;
            _repo = repo;
            _printer = printer;
            _logger = logger;
        }


        /// <summary>
        /// List all the range events in the database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list")]
        [UsedImplicitly]
        public async Task<int> ListRangeEvents(CancellationToken cancellationToken)
        {
            _cliDisplay.PrintAppInfo();
            Result<IEnumerable<SimpleRangeEvent>> rangeEvents = await _repo.GetSimpleRangeEventsAsync(cancellationToken)
                .ConfigureAwait(false);
            if (rangeEvents.IsFailed)
            {
                _cliDisplay.PrintFailure("Could not retrieve the list.");
                _logger.Warning("Failed to retrieve list from database.");

                return FAILURE;
            }

            await _printer.Start().ConfigureAwait(false);
            foreach (SimpleRangeEvent sre in rangeEvents.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Warning("Operation cancelled by user.");
                    _cliDisplay.PrintFailure("Operation cancelled.");
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
