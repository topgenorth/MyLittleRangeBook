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
        /// <param name="streamId"></param>
        /// <param name="uncommittedDomainEvents">Ignored for now.</param>
        /// <returns>A successful Result if the projection succeeded, an failed Result if there was a problem.</returns>
        public async Task<Result> ProjectAggregateAsync(DapperCommandContext       context,
                                                        MlrbId                     streamId,
                                                        IEnumerable<IDomainEvent>? uncommittedDomainEvents = null)
        {
            if (uncommittedDomainEvents is null)
            {
                return new Result().WithReasons([new Success("No domain events to project.")]);
            }

            IDomainEvent[] domainEvents = uncommittedDomainEvents as IDomainEvent[] ??
                                          uncommittedDomainEvents.ToArray();
            if (domainEvents.Length == 0)
            {
                return new Result().WithReasons([new Success("No domain events to project.")]);
            }

            List<IReason> reasons = [];

            try
            {
                Firearm? f = await LoadFirearmFromDatabase(context, streamId, domainEvents).ConfigureAwait(false);
                if (f is null)
                {
                    return Result.Fail("Unable to project the domain events to a firearm record.");
                }

                List<Task> tasksToExecute = [];
                foreach (IDomainEvent evt in domainEvents)
                {
                    switch (evt)
                    {
                        case FirearmAggregate.FirearmActive:
                            f.IsActive = true;
                            break;
                        case FirearmAggregate.FirearmBarrelChanged:
                            // TODO [20260714] Add a note...
                            break;
                        case FirearmAggregate.FirearmCleaned:
                            // TODO [20260714] Add a note...
                            break;

                        case FirearmAggregate.FirearmCreated:
                            // TODO [20260714] Add a note...
                            break;
                        case FirearmAggregate.FirearmInactive:
                            f.IsActive = false;
                            break;
                        case FirearmAggregate.FirearmModified:
                            // TODO [20260714] Add a note...
                            break;

                        case FirearmAggregate.FirearmRoundCountAltered e5:
                            f.RoundsFired += e5.Rounds;
                            break;
                        case FirearmAggregate.FirearmNoteAdded:
                            // TODO [20260714] Add a note...
                            break;

                        case FirearmAggregate.FirearmAssociatedWithAsset e1:
                            DapperCommandContext ctx1 = context with
                                                        {
                                                            Arguments = new { FirearmId = streamId, e1.AssetId },
                                                        };
                            tasksToExecute.Add(Commands.s_addAssociationToAsset.ExecuteAsync(ctx1));
                            break;

                        case FirearmAggregate.FirearmSightingSystemChanged:
                            // TODO [20260714] Add a note...
                            break;

                        case FirearmAggregate.FirearmDisassociatedFromAsset e4:
                            DapperCommandContext ctx4 = context with
                                                        {
                                                            Arguments = new { FirearmId = streamId, e4.AssetId },
                                                        };
                            await Commands.s_removeAssociationFromAsset.ExecuteAsync(ctx4).ConfigureAwait(false);
                            break;
                        case FirearmAggregate.FirearmDisassociatedFromRangeEvent e2:
                            DapperCommandContext ctx2 = context with
                                                        {
                                                            Arguments = new
                                                                        {
                                                                            FirearmId          = streamId,
                                                                            SimpleRangeEventId = e2.RangeEventId,
                                                                        },
                                                        };
                            tasksToExecute.Add(Commands.s_removeAssociationFromRangeEvent.ExecuteAsync(ctx2));
                            break;
                        case FirearmAggregate.FirearmAssociatedWithRangeEvent e3:
                            DapperCommandContext ctx3 = context with
                                                        {
                                                            Arguments = new
                                                                        {
                                                                            FirearmId          = streamId,
                                                                            SimpleRangeEventId = e3.RangeEventId,
                                                                        },
                                                        };
                            tasksToExecute.Add(Commands.s_addAssociationToRangeEvent.ExecuteAsync(ctx3));
                            break;

                        default:
                            reasons.Add(new Error($"Unknown domain event {evt.GetType().Name}."));
                            break;
                    }
                }

                Task<Result<EntityId>> upsertFirearmTask = _firearmsService.UpsertAsync(context, f!);
                tasksToExecute.Add(upsertFirearmTask);
                // Result<EntityId>       upsertFirearmResult                = await upsertFirearmTask.ConfigureAwait(true);
                await Task.WhenAll(tasksToExecute).ConfigureAwait(false);

                return new Result().WithReasons(reasons);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unexpected error trying to project the firearm aggregate.");
                Error err = e.ToError().Enrich(streamId);
                return Result.Fail(err);
            }
        }

        /// <summary>
        ///     Will try and load a record from the firearm table.
        /// </summary>
        /// <remarks>
        ///     If we detect that the firearm isn't in the firearms table, then we will created a new <c cref="Firearm" />
        ///     object but only if we have a <c cref="FirearmAggregate.FirearmCreated" /> event.
        /// </remarks>
        /// <param name="context"></param>
        /// <param name="firearmId"></param>
        /// <param name="domainEvents"></param>
        /// <returns>A <c cref="Firearm" /> instance from the firearms table, or an instance for a new record in the table.</returns>
        async Task<Firearm?> LoadFirearmFromDatabase(DapperCommandContext context,
                                                     MlrbId               firearmId,
                                                     IDomainEvent[]       domainEvents)
        {
            Result<Firearm> firearmResult = await _firearmsService
                                                 .GetFirearmAsync(context, firearmId)
                                                 .ConfigureAwait(false);
            if (firearmResult.IsSuccess)
            {
                return firearmResult.Value;
            }

            if (!firearmResult.HasError<FirearmDoesNotExistError>())
            {
                return null;
            }

            FirearmAggregate.FirearmCreated? created =
                domainEvents.OfType<FirearmAggregate.FirearmCreated>()
                            .Cast<FirearmAggregate.FirearmCreated?>()
                            .FirstOrDefault();

            if (created is null || string.IsNullOrWhiteSpace(created.Value.Name))
            {
                return null;
            }

            return Firearm.New(created.Value.Name);
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