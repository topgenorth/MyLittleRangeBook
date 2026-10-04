using ConsoleAppFramework;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook.RangeSessions
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

        public ListSimpleRangeEventsCommands(ICliDisplay                  cliDisplay,
                                             ILogger                      logger,
                                             ISimpleRangeEventListPrinter printer)
        {
            _cliDisplay                   = cliDisplay;
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
            throw new NotImplementedException();
            // int returnCode;
            // _cliDisplay.PrintCommandHeader("List range events.");
            // Result<IEnumerable<SimpleRangeSession>> rangeEvents = await _simpleRangeSessionService
            //                                                          .GetSimpleRangeEventsAsync(cancellationToken)
            //                                                        .ConfigureAwait(false);
            // if (rangeEvents.IsFailed)
            // {
            //     _cliDisplay.PrintFailure("Could not retrieve the list.");
            //     _logger.Warning("Failed to retrieve list from database.");
            //
            //     returnCode = FAILURE;
            //     goto ExitCommand;
            // }
            //
            // await _printer.Start().ConfigureAwait(false);
            // foreach (SimpleRangeSession sre in rangeEvents.Value)
            // {
            //     if (cancellationToken.IsCancellationRequested)
            //     {
            //         _logger.Warning("Operation cancelled by user.");
            //         _cliDisplay.PrintFailure("Operation cancelled.");
            //         await _printer.Finish().ConfigureAwait(false);
            //
            //         returnCode = COMMAND_CANCELLED;
            //         goto ExitCommand;
            //     }
            //
            //     await _printer.AddRow(sre).ConfigureAwait(false);
            // }
            //
            // await _printer.Finish().ConfigureAwait(false);
            // returnCode = SUCCESS;
            //
            // ExitCommand:
            // System.Console.ReadKey();
            //
            // return returnCode;
        }
    }
}