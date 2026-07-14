using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    ///     Update the round count for a given firearm.
    /// </summary>
    public class FirearmProjector : IProjector
    {
        public const string           DI_KEY = "firearm-projector";
        readonly     IEventSerializer _eventSerializer;
        readonly     IFirearmsService _firearmsService;
        readonly     ILogger          _logger;

        public FirearmProjector(
            IFirearmsService firearmsService,
            ILogger          logger,
            IEventSerializer eventSerializer)
        {
            _firearmsService = firearmsService;
            _logger          = logger;
            _eventSerializer = eventSerializer;
        }

        /// <summary>
        ///     Load the event stream for the firearm and then project the aggregate onto the firearm table.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="firearmId"></param>
        /// <param name="uncommittedDomainEvents">Ignored for now.</param>
        /// <returns></returns>
        public async Task<Result> ProjectAggregateAsync(DapperCommandContext       context,
                                                        MlrbId                     firearmId,
                                                        IEnumerable<IDomainEvent>? uncommittedDomainEvents = null)
        {
            try
            {
                List<IReason> reasons = [];
                foreach (IDomainEvent evt in uncommittedDomainEvents)
                {
                    switch (evt)
                    {
                        case FirearmAggregate.FirearmAssociatedWithAsset e1:
                            DapperCommandContext ctx1 = context with
                                                        {
                                                            Arguments = new { FirearmId = firearmId, e1.AssetId },
                                                        };
                            await Commands.s_addAssociationToAsset.ExecuteAsync(ctx1).ConfigureAwait(false);
                            ;
                            break;
                        case FirearmAggregate.FirearmDisassociatedFromAsset e4:
                            DapperCommandContext ctx4 = context with
                                                        {
                                                            Arguments = new { FirearmId = firearmId, e4.AssetId },
                                                        };
                            await Commands.s_removeAssociationFromAsset.ExecuteAsync(ctx4).ConfigureAwait(false);
                            break;
                        case FirearmAggregate.FirearmDisassociatedFromRangeEvent e2:
                            DapperCommandContext ctx2 = context with
                                                        {
                                                            Arguments = new
                                                                        {
                                                                            FirearmId = firearmId, e2.RangeEventId,
                                                                        },
                                                        };
                            await Commands.s_removeAssociationFromRangeEvent.ExecuteAsync(ctx2).ConfigureAwait(false);
                            break;
                        case FirearmAggregate.FirearmAssociatedWithRangeEvent e3:
                            DapperCommandContext ctx3 = context with
                                                        {
                                                            Arguments = new
                                                                        {
                                                                            FirearmId = firearmId, e3.RangeEventId,
                                                                        },
                                                        };
                            await Commands.s_addAssociationToRangeEvent.ExecuteAsync(ctx3).ConfigureAwait(false);
                            break;

                        default:
                            reasons.Add(new Error($"Unknown domain event {evt.GetType().Name}."));
                            break;
                    }
                }

                return new Result().WithReasons(reasons);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unexpected error trying to project the firearm aggregate.");
                Error err = e.ToError().Enrich(firearmId);
                return Result.Fail(err);
            }
        }

        /// <summary>
        ///     Loads the event stream for the specified firearm aggregate and combines it with optional uncommitted domain events.
        /// </summary>
        /// <param name="context">The database context used to fetch the event stream.</param>
        /// <param name="uncommittedDomainEvents">Optional uncommitted domain events to include in the stream.</param>
        /// <returns>A tuple containing the firearm aggregate and the combined list of event rows.</returns>
        async Task<(FirearmAggregate? stream, IEnumerable<IDomainEvent> events)> LoadEventStreamIncludeNewEvents(
            DapperCommandContext       context
          , IEnumerable<IDomainEvent>? uncommittedDomainEvents = null)
        {
            #region Combine the saved events with any new events.
            IEnumerable<EventRow> rows = await EventSourcingCommands.s_getEventStreamByRowId
                                                                    .QueryAsync<EventRow>(context)
                                                                    .ConfigureAwait(false);
            IEnumerable<IDomainEvent> commitedDomainEvents =
                rows.Select(row => (IDomainEvent)_eventSerializer.Deserialize(row.EventType,
                                                                              row.DataJson));
            IEnumerable<IDomainEvent> allEvents;
            if (uncommittedDomainEvents is not null)
            {
                allEvents = commitedDomainEvents.Concat(uncommittedDomainEvents).OrderBy(e => e.OccurredUtc);
            }
            else
            {
                allEvents = commitedDomainEvents;
            }
            #endregion


            FirearmAggregate? fa;
            try
            {
                EventStreamRow es = await EventSourcingCommands.s_getEventStream
                                                               .QuerySingleAsync<EventStreamRow>(context)
                                                               .ConfigureAwait(false);
                fa = FirearmAggregate.Create(es);
            }
            catch (InvalidOperationException)
            {
                fa = null;
            }

            return (fa, allEvents);
        }


        static class Commands
        {
            const string UPSERT_ASSET_FILES_FIREARMS_SQL = """
                                                           INSERT INTO asset_files_firearms (firearm_id, asset_id)
                                                           VALUES (@FirearmId, @AssetId)
                                                           ON CONFLICT DO NOTHING
                                                           RETURNING row_id;
                                                           """;

            const string UPSERT_FIREARM_SIMPLE_RANGE_EVENTS_SQL = """
                                                                  INSERT INTO firearms_simple_range_events (firearm_id, simple_range_event_id)
                                                                  VALUES (@FirearmId, @SimpleRangeEventId)
                                                                  ON CONFLICT DO NOTHING
                                                                  RETURNING row_id;
                                                                  """;

            const string DISASSOCIATE_FIREARM_FROM_ASSET_SQL = """
                                                               DELETE FROM asset_files_firearms
                                                               WHERE firearm_id = @FirearmId AND asset_id = @AssetId
                                                               """;

            const string DISASSOCIATE_FIREARM_FROM_RANGE_EVENT_SQL = """
                                                                     DELETE FROM firearms_simple_range_events
                                                                     WHERE firearm_id = @FirearmId AND simple_range_event_id = @SimpleRangeEventId
                                                                     """;

            internal static readonly DapperCommand s_addAssociationToRangeEvent =
                new(UPSERT_FIREARM_SIMPLE_RANGE_EVENTS_SQL);

            internal static readonly DapperCommand s_addAssociationToAsset =
                new(UPSERT_ASSET_FILES_FIREARMS_SQL);

            internal static readonly DapperCommand s_removeAssociationFromAsset =
                new(DISASSOCIATE_FIREARM_FROM_ASSET_SQL);

            internal static readonly DapperCommand s_removeAssociationFromRangeEvent =
                new(DISASSOCIATE_FIREARM_FROM_RANGE_EVENT_SQL);
        }
    }
}