using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.ReturnCodes;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Allows us to create a new Range Event from the CLI, and optionally the FIT file that goes with it.
    /// </summary>
    [RegisterCommands("rangeevent"), UsedImplicitly]
    public class SimpleRangeEventCommandAddToSqlite : MlrbSqliteCommandBase
    {
        readonly ISimpleRangeEventDataProcessor _rangeEventDataProcessor;
        readonly ISimpleRangeEventPrinter       _simpleRangeEventPrinter;
        readonly ISimpleRangeEventService       _simpleRangeEventService;

        public SimpleRangeEventCommandAddToSqlite(ILogger                        logger,
                                                  ICliDisplay                    cliDisplay,
                                                  ISimpleRangeEventDataProcessor simpleRangeEventProcessor,
                                                  ISqliteHelper                  sqliteHelper,
                                                  ISimpleRangeEventPrinter       simpleRangeEventPrinter,
                                                  ISimpleRangeEventService       simpleRangeEventService) :
            base(logger, cliDisplay, sqliteHelper)
        {
            _simpleRangeEventPrinter = simpleRangeEventPrinter;
            _simpleRangeEventService = simpleRangeEventService;
            _rangeEventDataProcessor = simpleRangeEventProcessor;
        }

        /// <summary>
        ///     Add a new range trip.
        /// </summary>
        /// <param name="firearm">
        ///     The name of the firearm. If this is omitted, then the CLI will promot for values based on what is
        ///     in the database already.
        /// </param>
        /// <param name="rounds">How many rounds were used. Required. Must be zero or greater.</param>
        /// <param name="range">The name of the shooting range.</param>
        /// <param name="ammo">A description of the ammo used. The recommended format is PROJECTILE[,|;]POWDER[</param>
        /// <param name="notes">Any notes or comments.  Optional</param>
        /// <param name="eventDate">The eventDate of the range trip in YYYY-MM-DD format. Default to today if omitted.</param>
        /// <param name="quiet">If this parameter is provided, then the command will display minimal output to the console.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("add"), UsedImplicitly]
        public async Task<int> AddSimpleRangeEventAsync(string                          firearm,
                                                        int                             rounds,
                                                        string                          range,
                                                        string                          ammo              = "",
                                                        string                          notes             = "",
                                                        [RangeTripDateParser] DateOnly? eventDate         = null,
                                                        bool                            quiet             = false,
                                                        CancellationToken               cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Process range event data.");
            await using DapperCommandContext context =
                await DapperCommandContext.NewAsync(SqliteHelper, cancellationToken, true)
                                          .ConfigureAwait(false);

            Result<MlrbId> rProcess = await _rangeEventDataProcessor
                                     .ProcessSimpleRangeEventData(context, firearm, rounds, range, ammo, notes,
                                                                  eventDate)
                                     .ConfigureAwait(false);

            int returnValue;
            if (rProcess.IsSuccess)
            {
                await context.CommitAsync().ConfigureAwait(false);
                returnValue = SUCCESS;
                Result<SimpleRangeEvent> sre = await _simpleRangeEventService.GetAsync(context, rProcess.Value)
                                                                             .ConfigureAwait(false);

                _simpleRangeEventPrinter.Print(AnsiConsole.Console, sre.Value, quiet);

                CliDisplay.PrintSuccess("Finished.");
            }
            else
            {
                await context.RollbackAsync().ConfigureAwait(false);
                returnValue = RANGE_EVENT_FAILED_TO_CREATE;
                CliDisplay.PrintFailure("Things didn't work.");
            }

            PressEnterToContinue();
            return returnValue;
        }
    }
}