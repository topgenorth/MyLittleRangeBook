using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Notes
{
    public class NotesService : INotesService
    {
        public async Task<Result> DeleteAsync(DapperCommandContext context, Note note)
        {
            if (note.RowId is null)
            {
                return Result.Ok()
                             .WithSuccess(new Success($"Nothing to delete; note `{note.Id}` does not exist.")
                                             .Enrich(note.Id, note.RowId));
            }

            try
            {
                DapperCommandContext ctx = context with { Arguments = new { note.Id } };
                await Commands.s_deleteCommand.ExecuteAsync(ctx).ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception e)
            {
                Error err = new Error($"Unexpected error trying to delete note `{note.Id}`: {e.Message}")
                           .CausedBy(e)
                           .Enrich(note.Id, note.RowId);

                return Result.Fail(err);
            }
        }

        public async Task<Result<MlrbId>> UpsertAsync(DapperCommandContext context, Note note)
        {
            try
            {
                note.ModifiedUtc = DateTimeOffset.UtcNow;
                var p = new
                        {
                            note.Id,
                            note.NoteType,
                            note.Content,
                            note.CreatedUtc,
                            note.ModifiedUtc,
                        };
                DapperCommandContext ctx = context with { Arguments = p };
                long rowId = await Commands.s_upsertCommand.ExecuteScalarAsync<long>(ctx).ConfigureAwait(false);

                note.RowId = rowId;

                Success success = new($"Note `{note.Id}` saved.");
                success.Enrich(note.Id, note.RowId);

                return Result.Ok((MlrbId)note.Id!).WithSuccess(success);
            }
            catch (Exception e)
            {
                Error err = e.ToError().Enrich(note.Id, note.RowId);
                return Result.Fail(err);
            }
        }

        public async Task<Result<Note>> GetAsync(DapperCommandContext context, MlrbId noteId)
        {
            try
            {
                DapperCommandContext ctx = context with { Arguments = new { Id = (string)noteId } };
                Note? note = await Commands.s_selectByIdCommand.QuerySingleOrDefaultAsync<Note>(ctx)
                                           .ConfigureAwait(false);

                return note is null
                           ? Result.Fail<Note>(new Error($"Note with id `{noteId}` not found").Enrich(noteId))
                           : Result.Ok(note);
            }
            catch (Exception e)
            {
                return Result.Fail<Note>(e.ToError().Enrich(noteId));
            }
        }

        public async Task<Result<IEnumerable<Note>>> GetNotesAsync(DapperCommandContext context,
                                                                   string?              noteType = null)
        {
            try
            {
                DapperCommand        cmd;
                DapperCommandContext ctx;

                if (noteType is not null)
                {
                    cmd = Commands.s_selectByNoteTypeCommand;
                    ctx = context with { Arguments = new { NoteType = noteType } };
                }
                else
                {
                    cmd = Commands.s_selectAllCommand;
                    ctx = context;
                }

                IEnumerable<Note> notes = await cmd.QueryAsync<Note>(ctx).ConfigureAwait(false);
                return Result.Ok(notes);
            }
            catch (Exception e)
            {
                Error err = new($"Could not retrieve notes from database: {e.Message}");
                err.CausedBy(e);

                return Result.Fail(err);
            }
        }

        static class Commands
        {
            const string DELETE_SQL = "DELETE FROM notes WHERE id = @Id";

            const string SELECT_ALL_SQL = """
                                        SELECT
                                            row_id       AS RowId,
                                            id           AS Id,
                                            note_type    AS NoteType,
                                            content      AS Content,
                                            created_utc  AS CreatedUtc,
                                            modified_utc AS ModifiedUtc
                                        FROM notes
                                        ORDER BY modified_utc DESC;
                                        """;

            const string SELECT_BY_ID_SQL = """
                                         SELECT
                                             row_id       AS RowId,
                                             id           AS Id,
                                             note_type    AS NoteType,
                                             content      AS Content,
                                             created_utc  AS CreatedUtc,
                                             modified_utc AS ModifiedUtc
                                         FROM notes
                                         WHERE id = @Id;
                                         """;

            const string SELECT_BY_NOTE_TYPE_SQL = """
                                               SELECT
                                                   row_id       AS RowId,
                                                   id           AS Id,
                                                   note_type    AS NoteType,
                                                   content      AS Content,
                                                   created_utc  AS CreatedUtc,
                                                   modified_utc AS ModifiedUtc
                                               FROM notes
                                               WHERE note_type = @NoteType
                                               ORDER BY modified_utc DESC;
                                               """;

            const string UPSERT_SQL = """
                                     INSERT INTO notes (id, note_type, content, created_utc, modified_utc)
                                     VALUES (@Id, @NoteType, @Content, @CreatedUtc, @ModifiedUtc)
                                     ON CONFLICT(id) DO
                                         UPDATE SET content      = @Content,
                                                    note_type    = @NoteType,
                                                    modified_utc = @ModifiedUtc
                                     RETURNING row_id
                                     """;

            internal static readonly DapperCommand s_deleteCommand           = new(DELETE_SQL);
            internal static readonly DapperCommand s_upsertCommand           = new(UPSERT_SQL);
            internal static readonly DapperCommand s_selectAllCommand        = new(SELECT_ALL_SQL);
            internal static readonly DapperCommand s_selectByIdCommand       = new(SELECT_BY_ID_SQL);
            internal static readonly DapperCommand s_selectByNoteTypeCommand = new(SELECT_BY_NOTE_TYPE_SQL);
        }
    }
}