using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    ///     Defines the contract for a service that manages firearms in the application, including operations for adding,
    ///     deleting, editing, or associating firearms with assets.
    /// </summary>
    public interface IFirearmsService
    {
        /// <summary>
        ///     Deletes the Firearm record from the database.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="firearmTableRow"></param>
        /// <returns></returns>
        Task<Result> DeleteAsync(FirearmTableRow firearmTableRow);

        Task<Result> DeleteAsync(MlrbId firearmId);

        Task<Result<FirearmTableRow>> GetFirearmAsync(MlrbId id);

        Task<Result<IEnumerable<FirearmTableRow>>> GetFirearmsAsync(bool activeOnly = true);

        Task<Result<MlrbId>> UpsertAsync(FirearmTableRow firearmTableRow);
    }
}