using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    ///     Defines the contract for a service that manages firearms in the application, including operations for adding,
    ///     deleting, editing, or associating firearms with assets.
    /// </summary>
    public interface IFirearmsService
    {
        Task<Result> AssociateWithRangeEvent(DapperCommandContext context, MlrbId firearmId, MlrbId rangeEventId);
        Task<Result> AssociateWithAsset(DapperCommandContext      context, MlrbId firearmId, MlrbId assetId);

        /// <summary>
        ///     Deletes the Firearm record from the database.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="firearm"></param>
        /// <returns></returns>
        Task<Result<bool>> DeleteAsync(DapperCommandContext context, Firearm firearm);

        Task<Result> DisassociateFromRangeEvent(DapperCommandContext context, MlrbId firearmId,
                                                MlrbId               rangeEventId);

        Task<Result> DisassociateWithAsset(DapperCommandContext context, MlrbId firearmId, MlrbId assetId);

        Task<Result<Firearm>> GetFirearmAsync(DapperCommandContext context, string id);


        Task<Result<IEnumerable<Firearm>>> GetFirearmsAsync(DapperCommandContext context,
                                                            bool                 activeOnly = true);


        /// <summary>
        ///     Update a row in the firearms table using the aggregate.
        /// </summary>
        /// <param name="firearm"></param>
        /// <param name="context"></param>
        /// <returns>An <c cref="EntityId">EntityId</c> that holds the Nanoid and the RowId of the firearm in the database.</returns>
        Task<Result<EntityId>> UpsertAsync(DapperCommandContext context, Firearm firearm);

        /// <summary>
        ///     Add a firearm to the database.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="firearmAggregate"></param>
        /// <returns>An <c cref="EntityId">EntityId</c> that holds the Nanoid and the RowId of the firearm in the database.</returns>
        Task<Result<EntityId>> UpsertAsync(DapperCommandContext context, FirearmAggregate firearmAggregate);
    }
}