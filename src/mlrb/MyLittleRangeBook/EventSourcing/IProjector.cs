using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.EventSourcing
{
    /// <summary>
    ///     Defines functionality for projecting domain events related to file imports into a storage system.
    /// </summary>
    public interface IProjector
    {
        /// <summary>
        ///     Projects a series of domain events into the event stream associated with a specific aggregate.
        /// </summary>
        /// <param name="context">The command context providing database connection, transaction, and other execution settings.</param>
        /// <param name="streamId">The unique identifier of the event stream (aggregate) to which the events belong.</param>
        /// <param name="uncommittedDomainEvents">
        ///     A collection of new domain events (i.e. not already in the events table) to project to the event stream. Can be null if no events are
        ///     provided.
        /// </param>
        /// <returns>A <see cref="Result" /> object indicating the outcome of the operation.</returns>
        Task<Result> ProjectAggregateAsync(DapperCommandContext       context,
                                           MlrbId                     streamId,
                                           IEnumerable<IDomainEvent>? uncommittedDomainEvents = null);
    }
}