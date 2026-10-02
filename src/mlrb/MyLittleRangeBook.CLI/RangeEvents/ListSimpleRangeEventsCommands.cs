using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using static MyLittleRangeBook.ReturnCodes;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     This class will display all the simple range events in a table to the console.
    /// </summary>
    [RegisterCommands("rangeevents")]
    [UsedImplicitly]
    public sealed class ListSimpleRangeEventsCommands
    {
        readonly ICliDisplay                  _cliDisplay;
        readonly ILogger                      _logger;
        readonly ISimpleRangeEventListPrinter _printer;
        readonly ISimpleRangeSessionService     _simpleRangeSessionService;

        public ListSimpleRangeEventsCommands(ICliDisplay                  cliDisplay,
                                             ISimpleRangeSessionService     simpleRangeSessionService,
                                             ILogger                      logger,
                                             ISimpleRangeEventListPrinter printer)
        {
            _cliDisplay                   = cliDisplay;
            _simpleRangeSessionService = simpleRangeSessionService;
            _logger                       = logger;
            _printer                      = printer;
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
            int returnCode;
            _cliDisplay.PrintCommandHeader("List range events.");
            Result<IEnumerable<SimpleRangeSession>> rangeEvents = await _simpleRangeSessionService
                                                                     .GetSimpleRangeEventsAsync(cancellationToken)
                                                                   .ConfigureAwait(false);
            if (rangeEvents.IsFailed)
            {
                _cliDisplay.PrintFailure("Could not retrieve the list.");
                _logger.Warning("Failed to retrieve list from database.");

                returnCode = FAILURE;
                goto ExitCommand;
            }

            await _printer.Start().ConfigureAwait(false);
            foreach (SimpleRangeSession sre in rangeEvents.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Warning("Operation cancelled by user.");
                    _cliDisplay.PrintFailure("Operation cancelled.");
                    await _printer.Finish().ConfigureAwait(false);

                    returnCode = COMMAND_CANCELLED;
                    goto ExitCommand;
                }

                await _printer.AddRow(sre).ConfigureAwait(false);
            }

            await _printer.Finish().ConfigureAwait(false);
            returnCode = SUCCESS;

            ExitCommand:
            System.Console.ReadKey();

            return returnCode;
        }
    }
}