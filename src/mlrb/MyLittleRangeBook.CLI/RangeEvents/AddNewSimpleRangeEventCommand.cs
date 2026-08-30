using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Allows us to create a new Range Event from the CLI, and optionally the FIT file that goes with it.
    /// </summary>
    [RegisterCommands("rangeevent")]
    [UsedImplicitly]
    public class AddNewSimpleRangeEventCommand
    {
        readonly ICliDisplay              _cliDisplay;
        readonly ISimpleRangeEventService _service;

        public AddNewSimpleRangeEventCommand(ICliDisplay              cliDisplay,
                                             ISimpleRangeEventService service)
        {
            _cliDisplay = cliDisplay;
            _service    = service;
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
            int returnValue = -1;
            _cliDisplay.PrintCommandHeader("Add a range event.");
            SimpleRangeEvent sre = SimpleRangeEvent.New(firearm.Trim(), rounds, range.Trim(), ammo.Trim(), notes.Trim(),
                                                        eventDate ?? DateOnly.FromDateTime(DateTime.UtcNow));


            Result<Guid> rAdd = await _service.UpsertAsync(sre, cancellationToken).ConfigureAwait(false);

            if (rAdd.IsSuccess)
            {
                returnValue = ReturnCodes.SUCCESS;
                _cliDisplay.PrintSuccess("Saved the range event.");
                SimpleRangeEventPrinter2 p = new();
                p.Print(_cliDisplay.Console,sre, false);
            }
            else
            {
                returnValue = ReturnCodes.RANGE_EVENT_FAILED_TO_CREATE;
                _cliDisplay.PrintFailure("Failed to save range event.");
            }


            return returnValue;
        }
    }
}