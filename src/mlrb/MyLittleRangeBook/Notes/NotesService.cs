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
                await Commands.DeleteCommand.ExecuteAsync(ctx).ConfigureAwait(false);
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
            note.ModifiedUtc = DateTimeOffset.UtcNow;

            try
            {
                var p = new
                        {
                            note.Id,
                            note.NoteType,
                            note.Content,
                            note.CreatedUtc,
                            note.ModifiedUtc,
                        };
                DapperCommandContext ctx = context with { Arguments = p };
                long rowId = await Commands.UpsertCommand.ExecuteScalarAsync<long>(ctx).ConfigureAwait(false);

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
                Note? note = await Commands.SelectByIdCommand.QuerySingleOrDefaultAsync<Note>(ctx)
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
                    cmd = Commands.SelectByNoteTypeCommand;
                    ctx = context with { Arguments = new { NoteType = noteType } };
                }
                else
                {
                    cmd = Commands.SelectAllCommand;
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
            const string DeleteSql = "DELETE FROM notes WHERE id = @Id";

            const string SelectAllSql = """
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

            const string SelectByIdSql = """
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

            const string SelectByNoteTypeSql = """
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

            const string UpsertSql = """
                                     INSERT INTO notes (id, note_type, content, created_utc, modified_utc)
                                     VALUES (@Id, @NoteType, @Content, @CreatedUtc, @ModifiedUtc)
                                     ON CONFLICT(id) DO
                                         UPDATE SET content      = @Content,
                                                    note_type    = @NoteType,
                                                    modified_utc = @ModifiedUtc
                                     RETURNING row_id
                                     """;

            internal static readonly DapperCommand DeleteCommand           = new(DeleteSql);
            internal static readonly DapperCommand UpsertCommand           = new(UpsertSql);
            internal static readonly DapperCommand SelectAllCommand        = new(SelectAllSql);
            internal static readonly DapperCommand SelectByIdCommand       = new(SelectByIdSql);
            internal static readonly DapperCommand SelectByNoteTypeCommand = new(SelectByNoteTypeSql);
        }
    }
}