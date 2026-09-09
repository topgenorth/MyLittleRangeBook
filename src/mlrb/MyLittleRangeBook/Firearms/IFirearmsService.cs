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
        [Obsolete("Not in use.")]
        Task<Result> DeleteAsync(FirearmTableRow firearmTableRow);

        [Obsolete("Not in use.")]
        Task<Result> DeleteAsync(MlrbId firearmId);

        [Obsolete("Not in use.")]
        Task<Result<FirearmTableRow>> GetFirearmAsync(MlrbId id);

        [Obsolete("Not in use.")]
        Task<Result<IEnumerable<FirearmTableRow>>> GetFirearmsAsync(bool activeOnly = true);

        [Obsolete("Not in use.")]
        Task<Result<MlrbId>> UpsertAsync(FirearmTableRow firearmTableRow);

        /// <summary>
        ///     This will append the contents of a Garmin shotview file to the event stream for the firearm.
        /// </summary>
        /// <param name="firearmName"></param>
        /// <param name="fileContents"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> AddGarminShotviewCsv(string            firearmName, string fileContents,
                                          CancellationToken cancellationToken = default);

        /// <summary>
        ///     Try to get the stream ID for the firearm name.  If it doesn't exist, then create the stream.
        /// </summary>
        /// <param name="firearmName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<Guid>> FetchStreamIdForFirearm(string                       firearmName,
                                                   CancellationToken cancellationToken);

        /// <summary>
        /// Adds a new reloading recipe to the specified firearm's event stream.
        /// </summary>
        /// <param name="firearmName">The name of the firearm to which the recipe is being added.</param>
        /// <param name="cartridgeName">The name of the cartridge used in the recipe.</param>
        /// <param name="ammoDescription">A description of the ammunition used in the recipe.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Result"/> representing the outcome of the operation.</returns>
        Task<Result> AddNewRecipe(string                       firearmName, string cartridgeName, string ammoDescription,
                                  CancellationToken cancellationToken = default);
    }
}