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
            _logger          = logger;
            _firearmsService = firearmsService;
            _eventSerializer = eventSerializer;
        }

        /// <summary>
        ///     Load the event stream for the firearm and then project the aggregate onto the firearm table.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="streamId"></param>
        /// <param name="uncommittedDomainEvents">Ignored for now.</param>
        /// <returns>A successful Result if the projection succeeded, an failed Result if there was a problem.</returns>
        public async Task<Result> ProjectAggregateAsync(DapperCommandContext       context,
                                                        MlrbId                     streamId,
                                                        IEnumerable<IDomainEvent>? uncommittedDomainEvents = null)
        {
            (FirearmAggregate? fa, IEnumerable<IDomainEvent> allEvents) =
                await LoadFirearmAggregateIncludeNewEvents(context, streamId, uncommittedDomainEvents)
                   .ConfigureAwait(false);

            IDomainEvent[] allDomainEvents = allEvents
                                            .OrderBy(evt => evt.OccurredUtc)
                                            .ToArray();

            IDomainEvent? latestRangeEventAssociationEvent = allDomainEvents
                                                            .Where(evt => evt is FirearmAggregate.FirearmAssociatedWithRangeEvent
                                                                       or FirearmAggregate.FirearmDisassociatedFromRangeEvent)
                                                            .MaxBy(evt => evt.OccurredUtc);

            IDomainEvent[] domainEvents = allDomainEvents
                                         .Where(evt => evt is not FirearmAggregate.FirearmAssociatedWithRangeEvent
                                                    and not FirearmAggregate.FirearmDisassociatedFromRangeEvent
                                                    || ReferenceEquals(evt, latestRangeEventAssociationEvent))
                                         .ToArray();

            if (domainEvents.Length == 0)
            {
                return new Result().WithReasons([new FirearmEventStreamProjectionSuccess("unknown", streamId)]);
            }

            List<IReason> reasons      = [];
            Firearm       f            = new() { Id = streamId };
            string?       firearmName  = null;
            List<Result>  tasksResults = [];
            try
            {
                foreach (IDomainEvent evt in domainEvents)
                {
                    fa!.Apply(evt);
                    switch (evt)
                    {
                        case FirearmAggregate.FirearmActive:
                            f.IsActive = true;
                            break;
                        case FirearmAggregate.FirearmAssociatedWithAsset e1:
                            tasksResults.Add(await AssociateAsset(context, streamId, e1.AssetId).ConfigureAwait(false));

                            break;

                        case FirearmAggregate.FirearmAssociatedWithRangeEvent e3:
                            tasksResults.Add(await AssociateRangeEvent(context, streamId, e3.RangeEventId).ConfigureAwait(false));

                            break;

                        case FirearmAggregate.FirearmBarrelChanged:
                            // TODO [20260714] Add a note...
                            break;
                        case FirearmAggregate.FirearmCleaned e7:
                            // TODO [20260714] Add a note...
                            break;

                        case FirearmAggregate.FirearmCreated e8:
                            // TODO [20260714] Add a note...
                            firearmName = e8.Name;
                            f.Name      = e8.Name;
                            break;

                        case FirearmAggregate.FirearmDisassociatedFromAsset e4:
                            tasksResults.Add(await DisassociateAsset(context, streamId, e4.AssetId).ConfigureAwait(false));
                            break;
                        case FirearmAggregate.FirearmDisassociatedFromRangeEvent e2:
                            tasksResults.Add(await DisassociateRangeEvent(context, streamId, e2.RangeEventId).ConfigureAwait(false));

                            break;

                        case FirearmAggregate.FirearmInactive:
                            f.IsActive = false;
                            break;
                        case FirearmAggregate.FirearmModified:
                            // TODO [20260714] Add a note...
                            break;
                        case FirearmAggregate.FirearmNoteAdded e6:
                            break;

                        case FirearmAggregate.FirearmRoundCountAltered e5:
                            f.RoundsFired += e5.Rounds;
                            break;

                        case FirearmAggregate.FirearmSightingSystemChanged:
                            // TODO [20260714] Add a note...
                            break;

                        default:
                            _logger.Debug("Unknown domain event {0} for a firearm.", evt.GetType().Name);
                            reasons.Add(new Success($"Unknown domain event {evt.GetType().Name} for a firearm."));
                            break;
                    }
                }

                var x = await _firearmsService.UpsertAsync(context, f);


                reasons.Add(new FirearmEventStreamProjectionSuccess(firearmName!, streamId).Enrich(streamId));
                return new Result().WithReasons(reasons);
            }
            catch (Exception e)
            {
                Error err1 = new FailedToProjectFirearmStreamError(streamId, firearmName).Enrich(streamId);
                _logger.Error(e, err1.Message);
                Error err2 = e.ToError().Enrich(streamId);
                return Result.Fail(err1).WithError(err2);
            }
        }


        /// <summary>
        ///     Loads the event stream for the specified firearm aggregate and combines it with optional uncommitted domain events.
        /// </summary>
        /// <param name="context">The database context used to fetch the event stream.</param>
        /// <param name="firearmId">The ID of the firearm for which to load the event stream.</param>
        /// <param name="uncommittedDomainEvents">Optional uncommitted domain events to include in the stream.</param>
        /// <returns>A tuple containing the firearm aggregate and the combined list of event rows.</returns>
        async Task<(FirearmAggregate? stream, IEnumerable<IDomainEvent> streamEvents)>
            LoadFirearmAggregateIncludeNewEvents(
                DapperCommandContext       context,
                MlrbId                     firearmId,
                IEnumerable<IDomainEvent>? uncommittedDomainEvents = null)
        {
            #region Combine the saved events with any new events.
            DapperCommandContext ctx = context with { Arguments = new { StreamId = firearmId } };
            IEnumerable<EventRow> rows = await EventSourcingCommands.s_getEventStreamByRowId
                                                                    .QueryAsync<EventRow>(ctx)
                                                                    .ConfigureAwait(false);
            Func<EventRow, IDomainEvent> selector = row => (IDomainEvent)_eventSerializer.Deserialize(row.EventType,
                                                        row.DataJson);
            IEnumerable<IDomainEvent> commitedDomainEvents = rows.Select(selector);
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
                DapperCommandContext ctx2 = context with { Arguments = new { StreamId = firearmId } };
                EventStreamRow es = await EventSourcingCommands.s_getEventStream
                                                               .QuerySingleAsync<EventStreamRow>(ctx2)
                                                               .ConfigureAwait(false);
                fa = FirearmAggregate.Create(es);
            }
            catch (InvalidOperationException)
            {
                fa = null;
            }

            return (fa, allEvents);
        }

        async Task<Result> AssociateAsset(DapperCommandContext context, MlrbId firearmId, MlrbId assetId)
        {
            try
            {
                DapperCommandContext ctx = context with
                                           {
                                               Arguments = new { FirearmId = firearmId, AssetId = assetId },
                                           };
                int     l       = await Commands.s_addAssociationToAsset.ExecuteAsync(ctx).ConfigureAwait(false);
                Success success = new($"Associated firearm {firearmId} with asset {assetId} - {l}.");
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError("Failed to associate asset with firearm."));
            }
        }

        async Task<Result> AssociateRangeEvent(DapperCommandContext context, MlrbId firearmId, MlrbId rangeEventId)
        {
            try
            {
                DapperCommandContext ctx = context with
                                           {
                                               Arguments = new
                                                           {
                                                               FirearmId          = firearmId,
                                                               SimpleRangeEventId = rangeEventId,
                                                           },
                                           };
                int     l       = await Commands.s_addAssociationToRangeEvent.ExecuteAsync(ctx).ConfigureAwait(false);
                Success success = new($"Associated firearm {firearmId} with range event {rangeEventId} - {l}.");
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError("Failed to associate range event to firearm."));
            }
        }

        async Task<Result> DisassociateAsset(DapperCommandContext context, MlrbId firearmId, MlrbId assetId)
        {
            try
            {
                DapperCommandContext ctx = context with
                                           {
                                               Arguments = new { FirearmId = firearmId, AssetId = assetId },
                                           };
                int     l       = await Commands.s_removeAssociationFromAsset.ExecuteAsync(ctx).ConfigureAwait(false);
                Success success = new($"Disassociated firearm {firearmId} with asset {assetId} - {l}.");
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError("Failed to disassociate asset from firearm."));
            }
        }

        async Task<Result> DisassociateRangeEvent(DapperCommandContext context,
                                                  MlrbId               firearmId,
                                                  MlrbId               rangeEventId)
        {
            try
            {
                var args = new { FirearmId = firearmId.ToString(), SimpleRangeEventId = rangeEventId.ToString() };
                DapperCommandContext ctx = context with { Arguments = args };

                int     l = await Commands.s_removeAssociationFromRangeEvent.ExecuteAsync(ctx).ConfigureAwait(false);
                Success success = new($"Disassociated firearm {firearmId} with range event {rangeEventId} - {l}.");
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError("Failed to disassociate firearm from range event"));
            }
        }


        static class Commands
        {
            const string ASSOCIATE_FIREARM_WITH_ASSET_SQL = """
                                                            INSERT INTO asset_files_firearms (firearm_id, asset_id)
                                                            VALUES (@FirearmId, @AssetId)
                                                            ON CONFLICT DO NOTHING
                                                            RETURNING row_id;
                                                            """;

            const string ASSOCIATE_FIREARM_WITH_SIMPLE_RANGE_EVENTS_SQL = """
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
                new(ASSOCIATE_FIREARM_WITH_SIMPLE_RANGE_EVENTS_SQL);

            internal static readonly DapperCommand s_addAssociationToAsset =
                new(ASSOCIATE_FIREARM_WITH_ASSET_SQL);

            internal static readonly DapperCommand s_removeAssociationFromAsset =
                new(DISASSOCIATE_FIREARM_FROM_ASSET_SQL);

            internal static readonly DapperCommand s_removeAssociationFromRangeEvent =
                new(DISASSOCIATE_FIREARM_FROM_RANGE_EVENT_SQL);
        }
    }
}