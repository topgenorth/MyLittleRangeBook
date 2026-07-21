using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Notes
{
    public interface INotesService
    {
        Task<Result> DeleteAsync(DapperCommandContext context, Note note);

        Task<Result<MlrbId>> UpsertAsync(DapperCommandContext context, Note note);

        Task<Result<Note>> GetAsync(DapperCommandContext context, MlrbId noteId);

        Task<Result<IEnumerable<Note>>> GetNotesAsync(DapperCommandContext context, string? noteType = null);
    }
}