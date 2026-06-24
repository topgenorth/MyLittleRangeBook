using System.Globalization;
using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
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

        public UpdateFirearmsFromRangeEventsCommand(ILogger logger,
                                                    ICliDisplay display,
                                                    ISqliteHelper sqliteHelper,
                                                    IFirearmsService firearmsService,
                                                    IFirearmAggregateRepository firearmAggregateRepo,
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
                IEnumerable<NewFirearmNameFromSimpleRangeEventRow> firearms =
                    await Commands.NewFirearmNamesFromRangeEvents
                                  .QueryAsync<NewFirearmNameFromSimpleRangeEventRow>(context).ConfigureAwait(false);

                int count = 0;
                foreach (NewFirearmNameFromSimpleRangeEventRow f in firearms)
                {
                    MlrbId streamId = MlrbId.FromString(f.FirearmName);
                    Result x = await _firearmsProjector.ProjectAggregateAsync(context, streamId).ConfigureAwait(false);
                    if (x.IsFailed)
                    {
                        Logger.Warning("Failed to project firearm aggregate for {FirearmName}", f.FirearmName);
                        CliDisplay.PrintWarning($"Failed to added firearm {f.FirearmName} - skipped.");
                    }
                    else
                    {
                        count++;
                    }
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
            public DateTimeOffset CreatedUtc => DateTimeOffset.Parse(Created, null, DateTimeStyles.AssumeLocal);
        }

        static class Commands
        {
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
            ///     The results are grouped and ordered by firearm name.
            /// </summary>
            const string GetNewFirearmNamesFromRangeEventsSql = """
                                                                WITH UnassociatedOldest AS (
                                                                    SELECT s.id           AS SimpleRangeEventId,
                                                                           s.firearm_name AS FirearmName,
                                                                           s.event_date   AS Created,
                                                                           ROW_NUMBER() OVER (PARTITION BY s.firearm_name ORDER BY s.event_date, s.id) AS rn
                                                                    FROM simple_range_events s
                                                                    WHERE NOT EXISTS (SELECT 1
                                                                                      FROM events e
                                                                                      WHERE e.stream_type = 'firearm'
                                                                                        AND e.event_type = 'firearm-created'
                                                                                        AND JSON_EXTRACT(e.data_json, '$.name') = s.firearm_name)
                                                                       OR NOT EXISTS (SELECT 1
                                                                                      FROM firearms f
                                                                                      WHERE f.name = s.firearm_name)
                                                                )
                                                                SELECT SimpleRangeEventId,
                                                                       FirearmName,
                                                                       Created
                                                                FROM UnassociatedOldest
                                                                WHERE rn = 1
                                                                ORDER BY Created, FirearmName;
                                                                """;


            /// <summary>
            ///     A pre-defined database command that retrieves information about firearms from range events
            ///     that have not yet been associated with a created firearm in the system. Each result
            ///     includes the firearm name, the earliest event date, and the identifier for that event,
            ///     ensuring that only the first unassociated event per firearm is selected. Results are
            ///     ordered by the event date and firearm name.
            /// </summary>
            internal static readonly DapperCommand NewFirearmNamesFromRangeEvents =
                new(GetNewFirearmNamesFromRangeEventsSql);

            /// <summary>
            ///     A list of all range events (that are not associated with a firearm aggregrete.
            /// </summary>
            internal static readonly DapperCommand s_getSimpleRangeEventsNotAssociatedWithFirearms =
                new(GET_SIMPLE_RANGE_EVENTS_NOT_ASSOCIATED_WITH_FIREARMS_SQL);
        }
    }
}