using System.Reflection;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Notes;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    ///     Update the round count for a given firearm.
    /// </summary>
    public partial class FirearmProjector : IProjector
    {
        public const string           DI_KEY = "firearm-projector";
        readonly     IEventSerializer _eventSerializer;
        readonly     IFirearmsService _firearmsService;
        readonly     ILogger          _logger;
        readonly     INotesService    _notesService;

        public FirearmProjector(
            IFirearmsService firearmsService,
            INotesService    notesService,
            ILogger          logger,
            IEventSerializer eventSerializer)
        {
            _logger          = logger;
            _firearmsService = firearmsService;
            _notesService    = notesService;
            _eventSerializer = eventSerializer;
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
            (FirearmAggregate? fa, IEnumerable<IDomainEvent> allEvents) =
                await LoadFirearmAggregateIncludeNewEvents(context, streamId, uncommittedDomainEvents)
                   .ConfigureAwait(false);

            IDomainEvent[] allDomainEvents = allEvents
                                            .OrderBy(evt => evt.OccurredUtc)
                                            .ToArray();

            IDomainEvent? latestRangeEventAssociationEvent = allDomainEvents
                                                            .Where(evt => evt is FirearmAggregate
                                                                                 .FirearmAssociatedWithRangeEvent
                                                                           or FirearmAggregate
                                                                                 .FirearmDisassociatedFromRangeEvent)
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

            List<IReason>      reasons         = [];
            Firearm            f               = new() { Id = streamId };
            string?            firearmName     = null;
            List<Result>       tasksResults    = [];
            List<Task<Result>> postUpsertTests = [];
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
                            tasksResults.Add(await AssociateRangeEvent(context, streamId, e3.RangeEventId)
                                                .ConfigureAwait(false));

                            break;

                        case FirearmAggregate.FirearmBarrelChanged e:
                            postUpsertTests.Add(AddFirearmNoteAsync(context, streamId,
                                                                    "barrel-change",
                                                                    $"Barrel changed from '{e.OldBarrel}' to '{e.NewBarrel}'.",
                                                                    e.OccurredUtc));
                            break;
                        case FirearmAggregate.FirearmCleaned e7:
                            postUpsertTests.Add(AddFirearmNoteAsync(context, streamId,
                                                                    "cleaned",
                                                                    "Firearm cleaned.",
                                                                    e7.OccurredUtc));
                            break;

                        case FirearmAggregate.FirearmCreated e8:
                            // tasksResults.Add(await AddFirearmNoteAsync(context, streamId,
                            //     GetNoteType(e8.GetType()),
                            //     $"Firearm '{e8.Name}' was created.",
                            //     e8.OccurredUtc).ConfigureAwait(false));
                            firearmName = e8.Name;
                            f.Name      = e8.Name;
                            break;

                        case FirearmAggregate.FirearmDisassociatedFromAsset e4:
                            tasksResults.Add(await DisassociateAsset(context, streamId, e4.AssetId)
                                                .ConfigureAwait(false));
                            break;
                        case FirearmAggregate.FirearmDisassociatedFromRangeEvent e2:
                            tasksResults.Add(await DisassociateRangeEvent(context, streamId, e2.RangeEventId)
                                                .ConfigureAwait(false));

                            break;

                        case FirearmAggregate.FirearmInactive e9:
                            postUpsertTests.Add(AddFirearmNoteAsync(context, streamId,
                                                                    "inactive",
                                                                    "Firearm marked as inactive.",
                                                                    e9.OccurredUtc));
                            f.IsActive = false;
                            break;

                        case FirearmAggregate.FirearmModified e10:
                            postUpsertTests.Add(AddFirearmNoteAsync(context, streamId,
                                                                    "modified",
                                                                    $"Firearm modified: {e10.Description}",
                                                                    e10.OccurredUtc));
                            break;
                        case FirearmAggregate.FirearmNoteAdded e6:
                            postUpsertTests.Add(AddFirearmNoteAsync(context, streamId,
                                                                    "note",
                                                                    e6.Text,
                                                                    e6.OccurredUtc));
                            break;

                        case FirearmAggregate.FirearmRoundCountAltered e5:
                            f.RoundsFired += e5.Rounds;
                            break;

                        case FirearmAggregate.FirearmSightingSystemChanged e11:
                            postUpsertTests.Add(AddFirearmNoteAsync(context, streamId,
                                                                    "sighting-system-changed",
                                                                    $"Sighting system changed from '{e11.OldAimingSystem}' to '{e11.NewAimingSystem}'.",
                                                                    e11.OccurredUtc));
                            break;

                        default:
                            _logger.Debug("Unknown domain event {0} for a firearm.", evt.GetType().Name);
                            reasons.Add(new Success($"Unknown domain event {evt.GetType().Name} for a firearm."));
                            break;
                    }
                }

                Result<EntityId> upsertResult = await _firearmsService.UpsertAsync(context, f);
                reasons.AddRange(upsertResult.Reasons);
                if (upsertResult.IsSuccess)
                {
                    Result[] postUpsertResults = await Task.WhenAll(postUpsertTests);
                    reasons.AddRange(Result.Merge(postUpsertResults).Reasons);
                }

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


        static string GetNoteType(Type eventType)
            => eventType.GetCustomAttribute<EventTypeAttribute>()?.Name ?? "note";

        async Task<Result> AddFirearmNoteAsync(
            DapperCommandContext context,
            MlrbId               eventId,
            string               noteType,
            string               content,
            DateTimeOffset       occurredUtc)
        {
            // [TO20260723] We need a deterministic ID for the node. Hopefully this is strong enough.
            MlrbId noteId = MlrbId.FromString($"{eventId}:{noteType}:{occurredUtc.UtcTicks}");
            try
            {
                Note note = new()
                            {
                                Id = noteId, NoteType = noteType, Content = content, CreatedUtc = occurredUtc,
                            };

                Result<MlrbId> upsertResult = await _notesService.UpsertAsync(context, note).ConfigureAwait(false);
                if (upsertResult.IsFailed)
                {
                    return upsertResult.ToResult();
                }

                DapperCommandContext ctx = context with { Arguments = new { FirearmId = eventId, NoteId = noteId } };
                await Commands.s_associateNoteWithFirearm.ExecuteAsync(ctx).ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError($"Failed to add note {noteId} of type {noteType} to firearm {eventId}."));
            }
        }
    }
}