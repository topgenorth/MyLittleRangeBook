using ConsoleAppFramework;
using Fisher;
using JasperFx.Events;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.RangeSessions
{
    /// <summary>
    ///     Allows us to create a new Range Event from the CLI, and optionally the FIT file that goes with it.
    /// </summary>
    [RegisterCommands("rangeevents")]
    [UsedImplicitly]
    public sealed class AddNewSimpleRangeEventCommand
    {
        readonly ICliDisplay      _cliDisplay;
        readonly ILogger          _logger;
        readonly IDocumentSession _session;

        public AddNewSimpleRangeEventCommand(ICliDisplay      cliDisplay,
                                             ILogger          logger,
                                             IDocumentSession session)
        {
            _cliDisplay = cliDisplay;
            _logger     = logger;
            _session    = session;
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


            Guid     id            = Guid.CreateVersion7();
            Guid     correlationId = Guid.CreateVersion7();
            Guid     causationId   = id;
            DateOnly ed            = eventDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            SimpleRangeSessionCreatedFromCli e = new(id,
                                                     correlationId,
                                                     causationId,
                                                     ed,
                                                     firearm.Trim(),
                                                     range.Trim(),
                                                     rounds,
                                                     ammo.Trim(), notes.Trim(), DateTimeOffset.UtcNow);

            StreamAction stream = _session.Events.StartStream<SimpleRangeSession>(id, e);
            try
            {
                await _session.SaveChangesAsync(cancellationToken);
                returnValue = ReturnCodes.SUCCESS;
                _cliDisplay.PrintSuccess("Simple range session created successfully.");
            }
            catch (Exception exception)
            {
                _cliDisplay.PrintFailure("Could not create the simple range session.");
                returnValue = ReturnCodes.SIMPLE_RANGE_SESSION_FAILED_TO_CREATE;
            }


            return returnValue;
        }
    }
}