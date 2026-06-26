using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        public const string                      DI_KEY = "firearm-projector";
        readonly     IEventSerializer            _eventSerializer;
        readonly     IFirearmsService            _firearmsService;
        readonly     ILogger                     _logger;
        readonly     IFirearmAggregateRepository _repo;

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
        ///     Load the event stream for the firearm, and then project the aggregate onto the firearms table.
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
                string               fid = firearmId.ToString();
                DapperCommandContext ctx = context with { Arguments = new { StreamId = fid } };
                (FirearmAggregate fa, IEnumerable<IDomainEvent> events) =
                    await LoadEventStreamIncludeNewEvents(ctx).ConfigureAwait(false);


                List<DapperCommandContext> upserts = [];
                foreach (IDomainEvent evt in events)
                {
                    fa.Apply(evt);
                    if (evt is FirearmAggregate.FirearmAssociatedWithRangeEvent @event)
                    {
                        var p = new { FirearmId = fid, SimpleRangeEventId= @event.RangeEventId };
                        upserts.Add(context with { Arguments = p });
                    }
                }

                Result<EntityId> r1 = await _firearmsService.UpsertAsync(context, fa);

                var reasons = new List<IReason>(r1.Reasons);

                if (!r1.IsSuccess)
                {
                    return new Result().WithReasons(reasons);
                }

                foreach (DapperCommandContext u in upserts)
                {
                    dynamic args         = u.Arguments!;
                    MlrbId  rangeEventId = args.SimpleRangeEventId;
                    try
                    {
                        int l = await Commands.s_addAssociationToRangeEvent.ExecuteAsync(u).ConfigureAwait(false);
                        if (l != 1)
                        {
                            reasons.Add(new Error($"Failed to associate the firearm {firearmId} and the event {rangeEventId}"));
                        }
                    }
                    catch (Exception e)
                    {
                        reasons.Add(e.ToError($"Unexpected exception trying to associate the firearm {firearmId} and the event {rangeEventId}"));
                        _logger.Error(e, "Unexpected exception try to associate the firearm {firearmId} and the event {rangeEventId}.", firearmId, rangeEventId);
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

        public static IServiceCollection Register(IServiceCollection services)
        {
            services.TryAddKeyedTransient<IProjector, FirearmProjector>(DI_KEY);
            return services;
        }

        /// <summary>
        ///     Loads the event stream for the specified firearm aggregate and combines it with optional uncommitted domain events.
        /// </summary>
        /// <param name="context">The database context used to fetch the event stream.</param>
        /// <param name="uncommittedDomainEvents">Optional uncommitted domain events to include in the stream.</param>
        /// <returns>A tuple containing the firearm aggregate and the combined list of event rows.</returns>
        async Task<(FirearmAggregate stream, IEnumerable<IDomainEvent> events)> LoadEventStreamIncludeNewEvents(
            DapperCommandContext       context
          , IEnumerable<IDomainEvent>? uncommittedDomainEvents = null)
        {
            #region Combine the saved events with any new events.
            IEnumerable<EventRow> rows = await EventSourcingCommands.s_getEventStreamByRowId
                                                                      .QueryAsync<EventRow>(context)
                                                                      .ConfigureAwait(false);
            var commitedDomainEvents = rows.Select(row => (IDomainEvent)_eventSerializer.Deserialize(row.EventType,
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


            EventStreamRow es = await EventSourcingCommands.s_getEventStream
                                                           .QuerySingleAsync<EventStreamRow>(context)
                                                           .ConfigureAwait(false);

            FirearmAggregate fa = FirearmAggregate.Create(es);
            return (fa, allEvents);
        }


        static class Commands
        {
            const string UPSERT_FIREARM_SIMPLE_RANGE_EVENTS_SQL = """
                                                                  INSERT INTO firearms_simple_range_events (firearm_id, simple_range_event_id)
                                                                  VALUES (@FirearmId, @SimpleRangeEventId)
                                                                  ON CONFLICT DO NOTHING
                                                                  RETURNING row_id;
                                                                  """;

            internal static readonly DapperCommand s_addAssociationToRangeEvent =
                new(UPSERT_FIREARM_SIMPLE_RANGE_EVENTS_SQL);
        }
    }
}