using Microsoft.Extensions.DependencyInjection;
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
        public const string DI_KEY = "firearm-projector";

        /// <summary>
        ///     A helper that will return true if the domain event is involved with associating the firearm with a range event.
        /// </summary>
        static readonly Func<IDomainEvent, bool> s_isAssociationEvent =
            evt => evt is FirearmAggregate.FirearmAssociatedWithRangeEvent
                       or FirearmAggregate.FirearmDisassociatedFromRangeEvent;

        readonly IEventSerializer _eventSerializer;
        readonly IProjector       _firearmNotesProjector;
        readonly IFirearmsService _firearmsService;
        readonly ILogger          _logger;

        public FirearmProjector(
            IFirearmsService firearmsService,
            ILogger          logger,
            IEventSerializer eventSerializer,
            [FromKeyedServices(FirearmNoteProjector.DI_KEY)]
            IProjector firearmNotesProjector)
        {
            _logger                = logger;
            _firearmsService       = firearmsService;
            _eventSerializer       = eventSerializer;
            _firearmNotesProjector = firearmNotesProjector;
        }

        /// <summary>
        ///     Load the event stream for the firearm and then project the aggregate onto the firearm table.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="streamId"></param>
        /// <param name="uncommittedDomainEvents">Ignored for now.</param>
        /// <returns>A successful Result if the projection succeeded, a failed Result if there was a problem.</returns>
        public async Task<Result> ProjectAggregateAsync(DapperCommandContext       context,
                                                        MlrbId                     streamId,
                                                        IEnumerable<IDomainEvent>? uncommittedDomainEvents = null)
        {
            (FirearmAggregate? fa, IDomainEvent[] allEvents) =
                await LoadFirearmAggregateIncludeNewEvents(context, streamId, uncommittedDomainEvents)
                   .ConfigureAwait(false);
            Result result = new();

            if (allEvents.Length > 0)
            {
                result.Reasons.Add(new FirearmEventStreamLoadedSuccess(streamId));
            }

            Result<Firearm> rProjectFirearm = await ProjectOntoFirearmsTable(context, streamId, allEvents)
                                                      .ConfigureAwait(false);

            if (!rProjectFirearm.IsSuccess)
            {
                Error err1 = new FirearmsTableUpdatedFromEventStreamError(streamId).Enrich(streamId);
                result.Reasons.Add(err1);
                return result;
            }

            Result rNewNotes = await _firearmNotesProjector
                                          .ProjectAggregateAsync(context, streamId, uncommittedDomainEvents)
                                          .ConfigureAwait(false);

            Result rMakeAssociations = await ProjectAssociations(context, streamId, allEvents)
                                           .ConfigureAwait(false);


            return  Result.Merge(result, rMakeAssociations, rNewNotes);
        }

        static IDomainEvent EventRowToIDomainEvent(IEventSerializer eventSerializer, EventRow row) =>
            (IDomainEvent)eventSerializer.Deserialize(row.EventType, row.DataJson);


        /// <summary>
        ///     This will create the appropriate association records between the firearm and other things.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="firearmId"></param>
        /// <param name="domainEvents"></param>
        /// <returns></returns>
        async Task<Result> ProjectAssociations(DapperCommandContext      context,
                                               MlrbId                    firearmId,
                                               IEnumerable<IDomainEvent> domainEvents)
        {
            List<Result> tasksResults = [];

            foreach (IDomainEvent evt in domainEvents.Where(s_isAssociationEvent))
            {
                switch (evt)
                {
                    case FirearmAggregate.FirearmAssociatedWithAsset e1:
                        tasksResults.Add(await AssociateAsset(context, firearmId, e1.AssetId));

                        break;

                    case FirearmAggregate.FirearmAssociatedWithRangeEvent e3:
                        tasksResults.Add(await AssociateRangeEvent(context, firearmId, e3.RangeEventId));

                        break;
                    case FirearmAggregate.FirearmDisassociatedFromAsset e4:
                        tasksResults.Add(await DisassociateAsset(context, firearmId, e4.AssetId));
                        break;
                    case FirearmAggregate.FirearmDisassociatedFromRangeEvent e2:
                        tasksResults.Add(await DisassociateRangeEvent(context, firearmId, e2.RangeEventId));

                        break;
                }
            }

            Result result = tasksResults.Merge();
            return result;
        }

        /// <summary>
        ///     This will play all the domain events onto a <c cref="Firearms" /> object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="firearmId"></param>
        /// <param name="domainEvents"></param>
        /// <param name="fa"></param>
        /// <returns></returns>
        async Task<Result<Firearm>> ProjectOntoFirearmsTable(DapperCommandContext      context,
                                                             MlrbId                    firearmId,
                                                             IEnumerable<IDomainEvent> domainEvents)
        {
            Firearm         f      = new() { Id = firearmId, Modified = DateTimeOffset.UtcNow };
            Result<Firearm> result = new Result<Firearm>().WithValue(f);

            foreach (IDomainEvent evt in domainEvents)
            {
                switch (evt)
                {
                    case FirearmAggregate.FirearmCreated e1:
                        f.Name     = e1.Name;
                        f.Created  = e1.OccurredUtc;
                        f.Modified = e1.OccurredUtc;
                        break;
                    case FirearmAggregate.FirearmActive:
                        f.IsActive = true;
                        break;

                    case FirearmAggregate.FirearmInactive:
                        f.IsActive = false;
                        break;

                    case FirearmAggregate.FirearmRoundCountAltered e2:
                        f.RoundsFired += e2.Rounds;
                        break;
                }
            }

            Result<EntityId> upsertResult = await _firearmsService.UpsertAsync(context, f);
            result.Reasons.AddRange(upsertResult.Reasons);

            if (upsertResult.IsSuccess)
            {
                f.RowId = upsertResult.Value.RowId;
                result.Reasons.Add(new FirearmsTableUpdatedFromEventStreamSuccess(f.Name, f.Id));
            }

            return result;
        }


        /// <summary>
        ///     Loads the event stream for the specified firearm aggregate and combines it with optional uncommitted domain events.
        /// </summary>
        /// <param name="context">The database context used to fetch the event stream.</param>
        /// <param name="firearmId">The ID of the firearm for which to load the event stream.</param>
        /// <param name="uncommittedDomainEvents">Optional uncommitted domain events to include in the stream.</param>
        /// <returns>A tuple containing the firearm aggregate and the combined list of event rows.</returns>
        async Task<(FirearmAggregate? stream, IDomainEvent[] streamEvents)>
            LoadFirearmAggregateIncludeNewEvents(
                DapperCommandContext       context,
                MlrbId                     firearmId,
                IEnumerable<IDomainEvent>? uncommittedDomainEvents = null)
        {
            IDomainEvent[]    allEvents;
            FirearmAggregate? fa;

            #region Combine the saved events with any new events.
            DapperCommandContext ctx = context with { Arguments = new { StreamId = firearmId } };
            IEnumerable<EventRow> rows = await EventSourcingCommands.s_getEventStreamByRowId
                                                                    .QueryAsync<EventRow>(ctx)
                                                                    .ConfigureAwait(false);
            IEnumerable<IDomainEvent> commitedDomainEvents =
                rows.Select((Func<EventRow, IDomainEvent>)(row => EventRowToIDomainEvent(_eventSerializer, row)));
            if (uncommittedDomainEvents is not null)
            {
                allEvents = commitedDomainEvents.Concat(uncommittedDomainEvents)
                                                .OrderBy(e => e.OccurredUtc)
                                                .ToArray();
            }
            else
            {
                allEvents = commitedDomainEvents.ToArray();
            }
            #endregion


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
                int l = await FirearmsService.Commands.s_addAssociationToAsset
                                             .ExecuteAsync(ctx).ConfigureAwait(false);
                return Result.Ok().WithSuccess(new FirearmAssociatedWithAssetSuccess(firearmId, assetId));
            }
            catch (Exception ex)
            {
                Error err = new FirearmAssociatedWithAssetError(firearmId, assetId).CausedBy(ex);
                return Result.Fail(err);
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
                int l = await FirearmsService.Commands.s_addAssociationToRangeEvent.ExecuteAsync(ctx)
                                             .ConfigureAwait(false);
                Success success = new FirearmAssociatedWithRangeEventSuccess(firearmId, rangeEventId);
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                Error err1 = new FirearmAssociatedToRangeEventError(firearmId, rangeEventId).CausedBy(ex);
                return Result.Fail(err1);
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
                int l = await FirearmsService.Commands.s_removeAssociationFromAsset.ExecuteAsync(ctx)
                                             .ConfigureAwait(false);
                return Result.Ok().WithSuccess(new FirearmDisassociatedFromAssetSuccess(firearmId, assetId));
            }
            catch (Exception ex)
            {
                return Result.Fail(new FirearmDisassociatedFromAssetError(firearmId, assetId).CausedBy(ex));
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

                int l = await FirearmsService.Commands.s_removeAssociationFromRangeEvent.ExecuteAsync(ctx)
                                             .ConfigureAwait(false);

                return Result.Ok().WithSuccess(new FirearmDisassociatedFromRangeEventSuccess(firearmId, rangeEventId));
            }
            catch (Exception ex)
            {
                return Result.Fail(new FirearmDisassociatedFromRangeEventError(firearmId, rangeEventId).CausedBy(ex));
            }
        }
    }
}