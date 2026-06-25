using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.ReturnCodes;
using static MyLittleRangeBook.Persistence.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Allows us to create a new Range Event from the CLI, and optionally the FIT file that goes with it.
    /// </summary>
    [RegisterCommands("rangeevent")]
    [UsedImplicitly]
    public class SimpleRangeEventCommandAddToSqlite : MlrbSqliteCommandBase
    {
        readonly ISimpleRangeEventRepository _simpleRangeEventRepo;
        readonly ISimpleRangeEventPrinter    _simpleRangeEventPrinter;

        public SimpleRangeEventCommandAddToSqlite(ILogger logger,
                                                  ICliDisplay cliDisplay,
                                                  [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeEventRepository simpleRangeEventRepo,
                                                  ISqliteHelper sqliteHelper,
                                                  ISimpleRangeEventPrinter simpleRangeEventPrinter) :
            base(logger, cliDisplay, sqliteHelper)
        {
            _simpleRangeEventRepo                    = simpleRangeEventRepo;
            _simpleRangeEventPrinter = simpleRangeEventPrinter;
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
        [Command("add")]
        [UsedImplicitly]
        public async Task<int> AddSimpleRangeEventAsync(string                          firearm,
                                                        int                             rounds,
                                                        string                          range,
                                                        string                          ammo              = "",
                                                        string                          notes             = "",
                                                        [RangeTripDateParser] DateOnly? eventDate         = null,
                                                        bool                            quiet             = false,
                                                        CancellationToken               cancellationToken = default)
        {
            int returnValue;
            CliDisplay.PrintCommandHeader("Add range event");
            DateOnly occurredDate = GetEventDate(eventDate);

            await using DapperCommandContext context =
                await DapperCommandContext.NewAsync(SqliteHelper, cancellationToken, true)
                                          .ConfigureAwait(false);


            try
            {
                SimpleRangeEvent sre = SimpleRangeEvent.New(
                                                            RemoveSurroundingQuotes(firearm),
                                                            rounds,
                                                            RemoveSurroundingQuotes(range),
                                                            RemoveSurroundingQuotes(ammo),
                                                            RemoveSurroundingQuotes(notes),
                                                            occurredDate);

                Result<MlrbId> result = await _simpleRangeEventRepo.UpsertAsync(context, sre).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    _simpleRangeEventPrinter.Print(CliDisplay.Console, sre, quiet);
                    CliDisplay.PrintSuccess("Range trip added successfully.");

                    returnValue = SUCCESS;
                    goto ExitFunction;
                }

                CliDisplay.PrintFailure("Failed to add range trip.");

                returnValue = RANGE_EVENT_FAILED_TO_CREATE;
            }
            catch (TaskCanceledException tce)
            {
                Logger.Warning(tce, "AddSimpleRangeEventAsync was cancelled.s");
                CliDisplay.PrintFailure("AddSimpleRangeEventAsync was cancelled.");

                returnValue = COMMAND_CANCELLED;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unexpected error trying to add SimpleRangeEvent");
                CliDisplay.PrintFailure($"Unexpected error trying to add SimpleRangeEvent: {e.Message}");

                returnValue = RANGE_EVENT_FAILED_TO_CREATE;
            }

            // [TO20260529] This makes me vomit; but works for now.
            ExitFunction:
            if (returnValue == SUCCESS)
            {
                await context.CommitAsync().ConfigureAwait(false);
            }
            else
            {
                await context.RollbackAsync().ConfigureAwait(false);
            }

            PressEnterToContinue();
            return returnValue;
        }

        static DateOnly GetEventDate(DateOnly? eventDate)
        {
            DateOnly dateOnly;
            if (eventDate is null)
            {
                DateTime d = DateTime.Now;
                dateOnly = DateOnly.FromDateTime(d);
            }
            else
            {
                dateOnly = eventDate.Value;
            }

            return dateOnly;
        }

        /// <summary>
        ///     Will strip the double quotes from the start and end of to the string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static string RemoveSurroundingQuotes(string value) =>
            value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"')
                ? value[1..^1]
                : value;
    }
}