using System.Data;
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
        // TODO [TO20260528] Allow for an IDbTransaction.
        /// <summary>
        ///     Delete a firearm from the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="firearm"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<bool>> DeleteAsync(IDbConnection connection,
            Firearm firearm,
            CancellationToken cancellationToken = default)
        {
            var ctx = new DapperCommandContext(connection, null, cancellationToken);
            return DeleteAsync(ctx, firearm);
        }

        Task<Result<bool>> DeleteAsync(DapperCommandContext context, Firearm firearm);

        /// <summary>
        ///     Add a firearm to the database.
        /// </summary>
        /// <param name="firearm"></param>
        /// <param name="connection"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>An <c cref="EntityId">EntityId</c> that holds the Nanoid and the RowId of the firearm in the database.</returns>
        Task<Result<EntityId>> UpsertAsync(Firearm firearm,
            IDbConnection connection,
            CancellationToken cancellationToken = default)
        {
            var ctx = new DapperCommandContext(connection, null, cancellationToken);

            return UpsertAsync(ctx, firearm);
        }

        /// <summary>
        ///     Get a list of firearms in the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="activeOnly">Set to false to retrieve all firearms.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<IEnumerable<Firearm>>> GetFirearmsAsync(IDbConnection connection,
            bool activeOnly = true,
            CancellationToken cancellationToken = default)
        {
            var ctx = new DapperCommandContext(connection, null, cancellationToken);
            return GetFirearmsAsync(ctx, activeOnly);
        }

        Task<Result<IEnumerable<Firearm>>> GetFirearmsAsync(DapperCommandContext context,
            bool activeOnly = true);


        /// <summary>
        ///     Retrieve a firearm from the database by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the firearm to retrieve.</param>
        /// <param name="connection">The database connection to be used for the query.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A <c cref="Result{Firearm}" /> containing the firearm if found, or an error if not.</returns>
        Task<Result<Firearm>> GetFirearmAsync(string id,
            IDbConnection connection,
            CancellationToken cancellationToken = default)
        {
            var ctx = new DapperCommandContext(connection, null, cancellationToken);
            return GetFirearmAsync(ctx, id);
        }

        Task<Result<Firearm>> GetFirearmAsync(DapperCommandContext context, string id);


        /// <summary>
        ///     Associate a range asset with the specified firearm.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="firearmId"></param>
        /// <param name="assetId"></param>
        /// <returns></returns>
        Task<Result> AssociateAssetWithFirearm(DapperCommandContext context, string firearmId,
            string assetId);

        /// <summary>
        /// Copies new firearm names from the SimpleRangeEvent table into the Firearms table.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<Result> UpdateFirearmsFromRangeEventsAsync(DapperCommandContext context);

        /// <summary>
        ///     Add a firearm to the database.
        /// </summary>
        /// <param name="firearm"></param>
        /// <param name="context"></param>
        /// <returns>An <c cref="EntityId">EntityId</c> that holds the Nanoid and the RowId of the firearm in the database.</returns>
        Task<Result<EntityId>> UpsertAsync(DapperCommandContext context, Firearm firearm);

    }
}
