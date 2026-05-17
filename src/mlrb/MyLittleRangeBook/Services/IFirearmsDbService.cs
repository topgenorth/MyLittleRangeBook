using System.Data;
using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Services
{
    public interface IFirearmsDbService
    {
        /// <summary>
        ///     Delete a firearm from the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="firearm"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<bool>> DeleteAsync(IDbConnection connection,
            Firearm firearm,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Add a firearm to the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="firearm"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>An <c cref="EntityId">EntityId</c> that holds the Nanoid and the RowId of the firearm in the database.</returns>
        Task<Result<EntityId>> UpsertAsync(IDbConnection connection,
            Firearm firearm,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Get a list of firearms in the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="activeOnly">Set to false to retrieve all firearms.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<IEnumerable<Firearm>>> GetFirearmsAsync(IDbConnection connection,
            bool activeOnly = true,
            CancellationToken cancellationToken = default);

        Task<Result<Firearm>> GetFirearmAsync(IDbConnection connection,
            string id,
            CancellationToken cancellationToken = default);
    }
}
