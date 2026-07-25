using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Notes;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    ///     This will only add new notes for a firearm; no checks are made to see if this is an existing note.
    /// </summary>
    public class FirearmNoteProjector : IProjector
    {
        public const string DI_KEY = "firearm-note-projector";

        public static readonly Func<IDomainEvent, bool> IsNoteEvent = evt => evt is FirearmAggregate.FirearmActive or
                                                                                 FirearmAggregate.FirearmBarrelChanged
                                                                               or
                                                                                 FirearmAggregate.FirearmCleaned or
                                                                                 FirearmAggregate.FirearmInactive or
                                                                                 FirearmAggregate.FirearmModified or
                                                                                 FirearmAggregate.FirearmNoteAdded or
                                                                                 FirearmAggregate
                                                                                    .FirearmSightingSystemChanged;

        readonly INotesService _notesService;

        public FirearmNoteProjector(INotesService notesService) => _notesService = notesService;

        public async Task<Result> ProjectAggregateAsync(DapperCommandContext       context, MlrbId streamId,
                                                        IEnumerable<IDomainEvent>? uncommittedDomainEvents = null)

        {
            if (uncommittedDomainEvents is null)
            {
                return Result.Ok().WithReason(new Success("Nothing to do."));
            }

            IOrderedEnumerable<IDomainEvent> noteEvents =
                uncommittedDomainEvents.Where(IsNoteEvent).OrderBy(e => e.OccurredUtc);
            List<Task<Result>> postUpsertTasks = [];
            List<IReason>      reasons         = [];
            foreach (IDomainEvent evt in noteEvents)
            {
                switch (evt)
                {
                    case FirearmAggregate.FirearmBarrelChanged e:
                        postUpsertTasks.Add(AddFirearmNoteAsync(context, streamId,
                                                                "barrel-change",
                                                                $"Barrel changed from '{e.OldBarrel}' to '{e.NewBarrel}'.",
                                                                e.OccurredUtc));
                        break;
                    case FirearmAggregate.FirearmCleaned e7:
                        postUpsertTasks.Add(AddFirearmNoteAsync(context, streamId,
                                                                "cleaned",
                                                                "Firearm cleaned.",
                                                                e7.OccurredUtc));
                        break;
                    case FirearmAggregate.FirearmModified e10:
                        postUpsertTasks.Add(AddFirearmNoteAsync(context, streamId,
                                                                "modified",
                                                                $"Firearm modified: {e10.Description}",
                                                                e10.OccurredUtc));
                        break;
                    case FirearmAggregate.FirearmNoteAdded e6:
                        postUpsertTasks.Add(AddFirearmNoteAsync(context, streamId,
                                                                "note",
                                                                e6.Text,
                                                                e6.OccurredUtc));
                        break;
                    case FirearmAggregate.FirearmSightingSystemChanged e11:
                        postUpsertTasks.Add(AddFirearmNoteAsync(context, streamId,
                                                                "sighting-system-changed",
                                                                $"Sighting system changed from '{e11.OldAimingSystem}' to '{e11.NewAimingSystem}'.",
                                                                e11.OccurredUtc));
                        break;

                    default:
                        reasons.Add(new
                                        Success($"Don't know how to project the domain event {evt.GetType().Name} to a note."));
                        break;
                }
            }

            Result result = (await Task.WhenAll(postUpsertTasks).ConfigureAwait(false))
                           .Merge()
                           .WithReasons(reasons);
            return result;
        }

        async Task<Result> AddFirearmNoteAsync(
            DapperCommandContext context,
            MlrbId               eventId,
            string               noteType,
            string               content,
            DateTimeOffset       occurredUtc)
        {
            Note note = new() { NoteType = noteType, Content = content, CreatedUtc = occurredUtc };
            try
            {
                Result<MlrbId> upsertResult = await _notesService.UpsertAsync(context, note).ConfigureAwait(false);
                if (upsertResult.IsFailed)
                {
                    return upsertResult.ToResult();
                }

                DapperCommandContext ctx = context with { Arguments = new { FirearmId = eventId, NoteId = note.Id } };
                await FirearmsService.Commands.s_associateNoteWithFirearm.ExecuteAsync(ctx).ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return
                    Result.Fail(ex.ToError($"Failed to add note {note.Id} of type {noteType} to firearm {eventId}."));
            }
        }
    }
}