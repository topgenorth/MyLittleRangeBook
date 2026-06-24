using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.EventSourcing
{
    public interface ISqliteAggregateRepository<TAggregate> where TAggregate : Aggregate
    {
        /// <summary>
        ///     Retrieves an aggregate by its unique identifier from the event sourcing store. Does not load any prior events.
        /// </summary>
        /// <param name="context">The database command context, including connection, transaction (if any), and cancellation token.</param>
        /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
        /// <returns>A result containing the aggregate if found, or <c>null</c> if no events exist for the specified identifier.</returns>
        Task<Result<TAggregate?>> GetAsync(DapperCommandContext context, MlrbId id);

        /// <summary>
        ///     Retrieves a collection of domain events associated with the specified stream identifier.
        /// </summary>
        /// <param name="context">The database command context that provides the SQLite connection and transaction.</param>
        /// <param name="streamId">The identifier of the event stream whose domain events are to be retrieved.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of domain events.</returns>
        Task<Result<IEnumerable<IDomainEvent>>> GetDomainEvents(
            DapperCommandContext context, MlrbId streamId);

        /// <summary>
        ///     Upserts the specified aggregate into the database using the provided Dapper command context.
        ///     If the aggregate does not exist, it will be inserted; if it exists, it will be updated.
        /// </summary>
        /// <param name="context">The Dapper command context containing the database connection and transaction information.</param>
        /// <param name="aggregate">The aggregate entity to upsert.</param>
        /// <param name="metadataJson"></param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        Task<Result> UpsertAsync(DapperCommandContext context, TAggregate aggregate, string? metadataJson = "{}");
    }
}