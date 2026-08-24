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

        public static readonly Func<IDomainEvent, bool> IsNoteEvent = evt => evt is Firearm.FirearmActive or
                                                                                 Firearm.FirearmBarrelChanged
                                                                               or
                                                                                 Firearm.FirearmCleaned or
                                                                                 Firearm.FirearmInactive or
                                                                                 Firearm.FirearmModified or
                                                                                 Firearm.FirearmNoteAdded or
                                                                                 Firearm
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
                    case Firearm.FirearmBarrelChanged e:
                        postUpsertTasks.Add(AddFirearmNoteAsync(context, streamId,
                                                                "barrel-change",
                                                                $"Barrel changed from '{e.OldBarrel}' to '{e.NewBarrel}'.",
                                                                e.OccurredUtc));
                        break;
                    case Firearm.FirearmCleaned e7:
                        postUpsertTasks.Add(AddFirearmNoteAsync(context, streamId,
                                                                "cleaned",
                                                                "Firearm cleaned.",
                                                                e7.OccurredUtc));
                        break;
                    case Firearm.FirearmModified e10:
                        postUpsertTasks.Add(AddFirearmNoteAsync(context, streamId,
                                                                "modified",
                                                                $"Firearm modified: {e10.Description}",
                                                                e10.OccurredUtc));
                        break;
                    case Firearm.FirearmNoteAdded e6:
                        postUpsertTasks.Add(AddFirearmNoteAsync(context, streamId,
                                                                "note",
                                                                e6.Text,
                                                                e6.OccurredUtc));
                        break;
                    case Firearm.FirearmSightingSystemChanged e11:
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
            MlrbId               firearmId,
            string               noteType,
            string               content,
            DateTimeOffset       occurredUtc)
        {
            Note   note   = new() { NoteType = noteType, Content = content, CreatedUtc = occurredUtc };
            Result result = new();
            try
            {
                Result<MlrbId> upsertResult = await _notesService.UpsertAsync(context, note).ConfigureAwait(false);
                result.Reasons.AddRange(upsertResult.Reasons);
            }
            catch (Exception ex)
            {
                return
                    Result.Fail(ex.ToError($"Failed to add note {note.Id} of type {noteType} to firearm {firearmId}."));
            }

            try
            {
                DapperCommandContext ctx = context with { Arguments = new { FirearmId = firearmId, NoteId = note.Id } };
                await FirearmsService.Commands.s_associateNoteWithFirearm.ExecuteAsync(ctx).ConfigureAwait(false);
                result.Reasons.Add(new FirearmAssociatedWithNoteSuccess(firearmId, note.Id));
            }
            catch (Exception ex2)
            {
                result.Reasons.Add(new FirearmAssociatedWithNoteError(firearmId, note.Id).CausedBy(ex2));
            }

            return result;
        }
    }
}