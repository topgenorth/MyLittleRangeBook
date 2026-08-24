using ConsoleAppFramework;
using Fisher;
using FluentResults;
using JasperFx.Events;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Allows us to create a new Range Event from the CLI, and optionally the FIT file that goes with it.
    /// </summary>
    [RegisterCommands("rangeevent")]
    [UsedImplicitly]
    public class SimpleRangeEventCommandAddToSqlite : MlrbSqliteCommandBase
    {
        /// <summary>
        ///     A list of <c ref="IReason" />'s that should be ignored when decided to commit the changes from the simple range
        ///     event.
        /// </summary>
        public static readonly Func<IReason, bool> s_reasonsThatDontCount = evt => evt is FirearmAssociatedWithNoteError
                                                                                        or
                                                                                          FirearmDisassociatedWithNoteError or
                                                                                          FirearmAssociatedToRangeEventError
                                                                                        or
                                                                                          FirearmDisassociatedFromRangeEventError
                                                                                        or
                                                                                          FirearmAssociatedWithAssetError or
                                                                                          FirearmDisassociatedFromAssetError;

        readonly ISimpleRangeEventDataProcessor   _rangeEventDataProcessor;
        readonly IDocumentSession                 _session;
        readonly ISimpleRangeEventDocumentService _simpleRangeEventDocumentService;
        readonly ISimpleRangeEventPrinter         _simpleRangeEventPrinter;

        public SimpleRangeEventCommandAddToSqlite(ILogger                          logger,
                                                  ICliDisplay                      cliDisplay,
                                                  ISimpleRangeEventDataProcessor   simpleRangeEventProcessor,
                                                  ISqliteHelper                    sqliteHelper,
                                                  ISimpleRangeEventPrinter         simpleRangeEventPrinter,
                                                  ISimpleRangeEventDocumentService simpleRangeEventDocumentService,
                                                  IDocumentSession                 session) :
            base(logger, cliDisplay, sqliteHelper)
        {
            _simpleRangeEventPrinter         = simpleRangeEventPrinter;
            _simpleRangeEventDocumentService = simpleRangeEventDocumentService;
            _session                         = session;
            _rangeEventDataProcessor         = simpleRangeEventProcessor;
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
            CliDisplay.PrintCommandHeader("Add a range event.");

            SimpleRangeEvent sre = SimpleRangeEvent.New(firearm.Trim(), rounds, range.Trim(), ammo.Trim(), notes.Trim(),
                                                        eventDate ?? DateOnly.FromDateTime(DateTime.UtcNow));
            MlrbId firearmId = MlrbId.FromString(sre.FirearmName);
            IEventStream<Firearm> f = await _session.Events
                                                    .FetchForWriting<Firearm>((Guid) firearmId, cancellationToken)
                                                    .ConfigureAwait(false);


            List<object> firearmevents =
            [
                new Firearm.FirearmActive(firearmId, sre.OccurredUtc),
                new Firearm.FirearmRoundCountAltered(firearmId, sre.RoundsFired, sre.OccurredUtc),
                new Firearm.FirearmAssociatedWithRangeEvent(firearmId, sre.Id, sre.OccurredUtc),
                new Firearm.FirearmNoteAdded(firearmId, notes.Trim(), sre.OccurredUtc),
            ];
            _session.Store(sre);
            f.AppendMany(firearmevents);

            await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            PressEnterToContinue();
            return returnValue;
        }
    }
}