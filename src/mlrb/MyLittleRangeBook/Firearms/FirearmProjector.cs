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

            if (allEvents.Length == 0)
            {
                return new Result().WithReasons([new FirearmEventStreamProjectionSuccess("unknown", streamId)]);
            }

            Result<Firearm> projectFirearmResult =
                await ProjectOntoFirearmsTable(context, streamId, allEvents, fa).ConfigureAwait(false);

            if (projectFirearmResult.IsSuccess)
            {
                Result associationResults = await ProjectAssociations(context, streamId, allEvents)
                                               .ConfigureAwait(false);
                Result projectNewNotes = await _firearmNotesProjector
                                              .ProjectAggregateAsync(context, streamId, uncommittedDomainEvents)
                                              .ConfigureAwait(false);

                Result result = Result.Merge(associationResults, projectNewNotes);

                if (result.IsSuccess)
                {
                    result.Reasons.Add(new FirearmEventStreamProjectionSuccess(fa!.Name, streamId)
                                          .Enrich(streamId));
                }
                else
                {
                    Error err2 =
                        new FailedToProjectFirearmStreamError(streamId, fa?.Name ?? "Unknown").Enrich(streamId);
                    result.Reasons.Add(err2);
                }

                return result;
            }

            Error err1 = new FailedToProjectFirearmStreamError(streamId, fa?.Name ?? "Unknown").Enrich(streamId);
            projectFirearmResult.Reasons.Add(err1);

            return Result.Fail(err1).WithReasons(projectFirearmResult.Reasons);
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
                                                             IEnumerable<IDomainEvent> domainEvents,
                                                             FirearmAggregate?         fa)
        {
            Firearm         f      = new() { Id = firearmId, Modified = DateTimeOffset.UtcNow };
            Result<Firearm> result = new Result<Firearm>().WithValue(f);
            if (fa is null)
            {
                f.Name = "INVALID";
                return result.WithError($"There is no event stream for the firearm ${firearmId}.");
            }


            foreach (IDomainEvent evt in domainEvents)
            {
                fa!.Apply(evt);
                switch (evt)
                {
                    case FirearmAggregate.FirearmCreated e1 :
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

                    default:
                        result.Reasons
                              .Add(new
                                       Success($"Don't know how to project the domain event {evt.GetType().Name} onto a firearm."));
                        break;
                }
            }

            Result<EntityId> upsertResult = await _firearmsService.UpsertAsync(context, f);
            result.Reasons.AddRange(upsertResult.Reasons);
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
            #region Combine the saved events with any new events.
            DapperCommandContext ctx = context with { Arguments = new { StreamId = firearmId } };
            IEnumerable<EventRow> rows = await EventSourcingCommands.s_getEventStreamByRowId
                                                                    .QueryAsync<EventRow>(ctx)
                                                                    .ConfigureAwait(false);
            IEnumerable<IDomainEvent> commitedDomainEvents =
                rows.Select((Func<EventRow, IDomainEvent>)(row => EventRowToIDomainEvent(_eventSerializer, row)));
            IDomainEvent[] allEvents;
            if (uncommittedDomainEvents is not null)
            {
                allEvents = commitedDomainEvents.Concat(uncommittedDomainEvents).OrderBy(e => e.OccurredUtc)
                                                .ToArray();
            }
            else
            {
                allEvents = commitedDomainEvents.ToArray();
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
                int l = await FirearmsService.Commands.s_addAssociationToAsset
                                             .ExecuteAsync(ctx).ConfigureAwait(false);
                Success success = new($"Associated firearm {firearmId} with asset {assetId} - {l}.");
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError("Failed to associate asset with firearm.").Enrich(firearmId));
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
                Success success = new($"Associated firearm {firearmId} with range event {rangeEventId} - {l}.");
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError("Failed to associate range event to firearm.").Enrich(firearmId));
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
                Success success = new($"Disassociated firearm {firearmId} with asset {assetId} - {l}.");
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError("Failed to disassociate asset from firearm.").Enrich(firearmId));
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
                Success success = new($"Disassociated firearm {firearmId} with range event {rangeEventId} - {l}.");
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError("Failed to disassociate firearm from range event").Enrich(firearmId));
            }
        }
    }
}