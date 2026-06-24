using System.Globalization;
using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    [RegisterCommands("firearms")]
    public class UpdateFirearmsFromRangeEventsCommand : MlrbFirearmsCommandBase
    {
        readonly IProjector _firearmsProjector;

        public UpdateFirearmsFromRangeEventsCommand(ILogger                     logger,
                                                    ICliDisplay                 display,
                                                    ISqliteHelper               sqliteHelper,
                                                    IFirearmsService            firearmsService,
                                                    IFirearmAggregateRepository firearmAggregateRepo,
                                                    [FromKeyedServices(FirearmProjector.DI_KEY)]
                                                    IProjector firearmsProjector) : base(logger, display, sqliteHelper,
            firearmsService, firearmAggregateRepo) =>
            _firearmsProjector = firearmsProjector;


        /// <summary>
        ///     This is a maintenance task. It will update the Firearms table based on what is in the SimpleRangeEvents table.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("import-from-range-events")]
        [UsedImplicitly]
        public async Task<int> ImportFirearmsFromRangeEvents(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Import new firearms from range events.");
            int returnCode;

            await using DapperCommandContext context = await DapperCommandContext
                                                            .NewAsync(SqliteHelper, cancellationToken, true)
                                                            .ConfigureAwait(false);

            try
            {
                IEnumerable<NewFirearmNameFromSimpleRangeEventRow> newFirearmNames =
                    await Commands.s_newFirearmNamesFromRangeEvents
                                  .QueryAsync<NewFirearmNameFromSimpleRangeEventRow>(context).ConfigureAwait(false);

                int count = 0;
                foreach (NewFirearmNameFromSimpleRangeEventRow f in newFirearmNames)
                {
                    #region Step 1: Get or create a new firearm aggregate
                    Result<FirearmAggregate?> fetchResult =
                        await FirearmAggregateRepository.GetAsync(context, f.FirearmId)
                                                        .ConfigureAwait(false);
                    if (fetchResult is { IsSuccess: true, Value: not null })
                    {
                        var r = await _firearmsProjector.ProjectAggregateAsync(context, f.FirearmId)
                                                        .ConfigureAwait(false);
                        CliDisplay.PrintInfo($"Created firearm '{f.FirearmName}' from the existing event stream.");
                        count++;
                        continue;
                    }

                    FirearmAggregate fa = FirearmAggregate.New(f.FirearmName, f.CreatedUtc);
                    fa.AppendToNotes($"Imported from simple_range_event {f.SimpleRangeEventId}.", DateTimeOffset.UtcNow);

                    // var r = await CreateNewFirearmAggregate(context, f.FirearmName).ConfigureAwait(false);
                    CliDisplay
                       .PrintWarning($"Will have to create the firearm '{f.FirearmName}' event stream from the range events");
                    #endregion

                    #region Step 2: Get round counts from simple range events, and update the aggregate.
                    List<(string FirearmId, string SimpleRangeEventId)> associations = [];
                    DapperCommandContext ctx = context with { Arguments = new { f.FirearmName } };
                    IEnumerable<RoundCountEventRow> shotsFired =
                        await Commands.s_rangeEventRoundCountsForFirearm.QueryAsync<RoundCountEventRow>(ctx)
                                      .ConfigureAwait(false);
                    foreach (RoundCountEventRow r in shotsFired)
                    {
                        fa.MoreRoundsFired(r.RoundsFired, r.CreatedUtc);
                        fa.AssociateWithSimpleRangeEvent(r.SimpleRangeEventId, r.CreatedUtc);
                        associations.Add((fa.Id, r.SimpleRangeEventId));
                    }

                    Result upsertEventStreamResult = await FirearmAggregateRepository
                                                          .UpsertAsync(context, fa)
                                                                          .ConfigureAwait(false);
                    if (upsertEventStreamResult.IsFailed)
                    {
                        Logger.Warning("Failed to create the firearm aggregate for '{FirearmName}' - skipped",
                                       f.FirearmName);
                        CliDisplay
                           .PrintWarning($"Failed to create the firearm aggregate for '{f.FirearmName}' - skipped.");
                        continue;
                    }

                    CliDisplay.PrintInfo($"Created the event stream for '{f.FirearmName}'.");
                    #endregion


                    #region Step 3: Upsert the firearm.
                    Firearm f2 = fa.ToFirearm();
                    Result<EntityId> firearmUpsertResult =
                        await FirearmsService.UpsertAsync(context, f2).ConfigureAwait(false);
                    if (firearmUpsertResult.IsFailed)
                    {
                        Logger.Warning("Failed to upsert firearm '{FirearmName}' - skipped.", f2.Name);
                        CliDisplay.PrintWarning($"Failed to upsert firearm '{f2.Name}' - skipped.");
                    }
                    else
                    {
                        count++;
                        CliDisplay.PrintInfo($"Save '{f2.Name}' in `firearms` table.");
                    }
                    #endregion


                    #region Step 4: Associate the range event to the firearm.
                    foreach (DapperCommandContext? ctx2 in associations.Select(r => context with
                                                                               {
                                                                                   Arguments = new
                                                                                       {
                                                                                           r.FirearmId,
                                                                                           r.SimpleRangeEventId,
                                                                                       },
                                                                               }))
                    {
                        long result = await Commands.s_associateFirearmWithRangeEvent.ExecuteScalarAsync<long>(ctx2)
                                                    .ConfigureAwait(false);
                    }
                    #endregion
                }

                returnCode = ReturnCodes.SUCCESS;
                CliDisplay.PrintSuccess($"Imported {count} firearms.");
            }
            catch (Exception e)
            {
                returnCode = ReturnCodes.FAILURE;
                Logger.Error(e, "Unexpected exception trying to import firearms from range events.");
            }

            if (returnCode != ReturnCodes.SUCCESS)
            {
                await context.RollbackAsync().ConfigureAwait(false);
            }
            else
            {
                await context.CommitAsync().ConfigureAwait(false);
            }

            PressEnterToContinue();

            return returnCode;
        }

        /// <summary>
        ///     Represents a record containing the name of a firearm extracted from a SimpleRangeEvent
        ///     in cases where no associated FirearmAggregate exists. It is used to process and handle
        ///     events where firearm names need to be retrieved or linked to an aggregate.
        /// </summary>
        /// <remarks>
        ///     The <see cref="NewFirearmNameFromSimpleRangeEventRow" /> struct stores the essential data
        ///     from SimpleRangeEvents for firearms that have not yet been linked with their respective
        ///     aggregates in the system. It includes the event ID, firearm name, and creation timestamp.
        /// </remarks>
        /// <param name="SimpleRangeEventId">
        ///     The unique identifier for the oldest SimpleRangeEvent in the database.
        /// </param>
        /// <param name="FirearmName">
        ///     The name of the firearm retrieved from the SimpleRangeEvent.
        /// </param>
        /// <param name="Created">
        ///     The creation timestamp of the SimpleRangeEvent in string format.
        /// </param>
        readonly record struct NewFirearmNameFromSimpleRangeEventRow(
            string SimpleRangeEventId,
            string FirearmName,
            string Created)
        {
            public MlrbId FirearmId => MlrbId.FromString(FirearmName);
            public DateTimeOffset CreatedUtc => DateTimeOffset.Parse(Created, null, DateTimeStyles.AssumeLocal);
        }

        readonly record struct RoundCountEventRow(
            string SimpleRangeEventId,
            string FirearmName,
            int    RoundsFired,
            string Created)
        {
            public MlrbId         FirearmId  => MlrbId.FromString(FirearmName);
            public DateTimeOffset CreatedUtc => DateTimeOffset.Parse(Created, null, DateTimeStyles.AssumeLocal);
        }

        static class Commands
        {
            const string UPSERT_FIREARM_RANGE_EVENT_SQL = """
                                                          INSERT INTO firearms_simple_range_events (firearm_id, simple_range_event_id)
                                                          VALUES (@FirearmId, @SimpleRangeEventId)
                                                          ON CONFLICT(firearm_id, simple_range_event_id) DO NOTHING
                                                          RETURNING row_id;
                                                          """;

            const string GET_FIRST_RANGE_EVENT_FOR_EACH_FIREARM_SQL = """
                                                                      WITH FirearmNamesInSimpleRangeEvents AS (
                                                                          SELECT s.id           AS SimpleRangeEventId,
                                                                                 s.firearm_name AS FirearmName,
                                                                                 s.event_date   AS Created,
                                                                                 ROW_NUMBER() OVER (PARTITION BY s.firearm_name ORDER BY s.event_date, s.id) AS rn
                                                                          FROM simple_range_events s
                                                                      )
                                                                      SELECT SimpleRangeEventId,
                                                                             FirearmName,
                                                                             Created
                                                                      FROM FirearmNamesInSimpleRangeEvents
                                                                      WHERE rn = 1
                                                                      ORDER BY Created, FirearmName;
                                                                      """;

            const string GET_RANGE_EVENT_ROUND_COUNTS_FOR_FIREARM_SQL = """
                                                                        SELECT s.id As SimpleRangeEventId,
                                                                               s.firearm_name As FirearmName,
                                                                               s.rounds_fired AS RoundsFired,
                                                                               s.event_date As Created
                                                                        FROM simple_range_events s
                                                                        WHERE s.firearm_name = @FirearmName AND s.rounds_fired > 0
                                                                        ORDER BY s.event_date, s.id;
                                                                        """;

            const string GET_SIMPLE_RANGE_EVENTS_NOT_ASSOCIATED_WITH_FIREARMS_SQL = """
                SELECT s.id AS SimpleRangeEventId,
                       s.firearm_name AS FirearmName,
                       s.rounds_fired AS RoundsFired,
                       s.event_date AS EventDate
                FROM simple_range_events s
                WHERE NOT EXISTS (SELECT 1
                                  FROM events e
                                  WHERE e.stream_type = 'firearm'
                                    AND e.event_type = 'range-event-associated-with-firearm'
                                    AND JSON_EXTRACT(e.data_json, '$.rangeEventId') = s.id)
                AND s.firearm_name = @FirearmName
                ORDER BY s.event_date, s.firearm_name;
                """;

            /// <summary>
            ///     Retrieves the names of firearms and their associated total rounds fired,
            ///     based on range events, for firearms that are not yet present in the firearms table.
            /// </summary>
            const string GET_NEW_FIREARM_NAMES_FROM_RANGE_EVENTS_SQL = """
                                                                       WITH UnprojectedFirearmNames AS (
                                                                           SELECT s.row_id AS RowId,
                                                                                  s.id As SimpleRangeEventId,
                                                                                  s.firearm_name AS FirearmName,
                                                                                  s.event_date   AS Created,
                                                                                  ROW_NUMBER() OVER (PARTITION BY s.firearm_name ORDER BY s.row_id) AS rn
                                                                           FROM simple_range_events s
                                                                           WHERE NOT EXISTS (SELECT 1 FROM firearms f WHERE f.name = s.firearm_name)
                                                                           ORDER BY s.row_id
                                                                       )
                                                                       SELECT ufn.SimpleRangeEventId,
                                                                              ufn.FirearmName,
                                                                              ufn.Created
                                                                       FROM UnprojectedFirearmNames ufn
                                                                       WHERE ufn.rn = 1
                                                                       ORDER BY ufn.RowId;
                                                                       """;

            internal static readonly DapperCommand s_associateFirearmWithRangeEvent =
                new(UPSERT_FIREARM_RANGE_EVENT_SQL);

            internal static DapperCommand s_firstRangeEventForEachFirearm =
                new(GET_FIRST_RANGE_EVENT_FOR_EACH_FIREARM_SQL);

            internal static readonly DapperCommand s_rangeEventRoundCountsForFirearm =
                new(GET_RANGE_EVENT_ROUND_COUNTS_FOR_FIREARM_SQL);


            /// <summary>
            ///     A pre-defined database command that retrieves information about firearms from range events
            ///     that have not yet been associated with a created firearm in the system. Each result
            ///     includes the firearm name, the earliest event date, and the identifier for that event,
            ///     ensuring that only the first unassociated event per firearm is selected. Results are
            ///     ordered by the event date and firearm name.
            /// </summary>
            internal static readonly DapperCommand s_newFirearmNamesFromRangeEvents =
                new(GET_NEW_FIREARM_NAMES_FROM_RANGE_EVENTS_SQL);

            /// <summary>
            ///     A list of all range events (that are not associated with a firearm aggregrete.
            /// </summary>
            internal static readonly DapperCommand s_getSimpleRangeEventsNotAssociatedWithFirearms =
                new(GET_SIMPLE_RANGE_EVENTS_NOT_ASSOCIATED_WITH_FIREARMS_SQL);
        }
    }
}