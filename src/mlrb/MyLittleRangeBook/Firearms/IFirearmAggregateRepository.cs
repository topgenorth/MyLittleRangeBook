using System.Globalization;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    public interface IFirearmAggregateRepository: ISqliteAggregateRepository<FirearmAggregate>
    {
        /// <summary>
        ///     Retrieves a firearm aggregate by its name or creates a new one if it does not exist.
        /// </summary>
        /// <param name="ctx">
        ///     The command context used for database operations.
        /// </param>
        /// <param name="firearmName">
        ///     The name of the firearm to retrieve or create.
        /// </param>
        /// <param name="createUtc">
        ///     The optional creation timestamp for the firearm if a new one is created.
        /// </param>
        /// <returns>
        ///     A result containing the firearm aggregate found or created.
        /// </returns>
        Task<Result<FirearmAggregate>> GetOrCreateByNameAsync(DapperCommandContext ctx, string firearmName,
                                                              DateTimeOffset?      createUtc = null);


        /// <summary>
        ///     Saves the specified firearm aggregate to the repository.
        /// </summary>
        /// <param name="aggregate">The firearm aggregate to be saved.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result indicating success or failure of the save operation.</returns>
        Task<Result> SaveAsync(FirearmAggregate aggregate, CancellationToken cancellationToken = default);

    }
}