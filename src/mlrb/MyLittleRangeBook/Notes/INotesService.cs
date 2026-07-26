using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Notes
{
    /// <summary>
    /// Provides operations for managing notes.
    /// </summary>
    public interface INotesService
    {
        /// <summary>
        /// Delete a single note from the notes table.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        Task<Result> DeleteAsync(DapperCommandContext context, Note note);

        /// <summary>
        /// Upsert a single note in the notes table.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        Task<Result<MlrbId>> UpsertAsync(DapperCommandContext context, Note note);

        /// <summary>
        /// Retrieve a single note by its ID.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="noteId"></param>
        /// <returns></returns>
        Task<Result<Note>> GetAsync(DapperCommandContext context, MlrbId noteId);

        /// <summary>
        /// Retrieve notes, optionally filtered by type.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="noteType"></param>
        /// <returns></returns>
        Task<Result<IEnumerable<Note>>> GetNotesAsync(DapperCommandContext context, string? noteType = null);
    }
}