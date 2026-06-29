using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Provides an interface for managing and performing operations on simple range events
    ///     in a repository. This includes CRUD operations such as creating, updating, retrieving,
    ///     and deleting simple range events.
    /// </summary>
    [Obsolete("Do we need this if we have the SimpleRangeEventService?")]
    public interface ISimpleRangeEventRepository
    {
        /// <summary>
        ///     Deletes the specified simple range event from the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="simpleRangeEvent">The simple range event to be deleted.</param>
        /// <returns>A result indicating whether the deletion was successful.</returns>
        Task<Result> DeleteAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent);

        /// <summary>
        ///     Retrieves a specific SimpleRangeEvent by its unique identifier.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id">The unique identifier of the SimpleRangeEvent to retrieve.</param>
        /// <returns>A result containing the requested SimpleRangeEvent if successful or an error if the operation fails.</returns>
        Task<Result<SimpleRangeEvent>> GetAsync(DapperCommandContext context, string id);

        /// <summary>
        ///     Retrieves a collection of simple range events.
        /// </summary>
        /// <returns>A result containing an enumerable of simple range events, or an error if the operation fails.</returns>
        Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(DapperCommandContext context);

        /// <summary>
        /// Adds or updates a simple range event within the provided DapperCommandContext.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="simpleRangeEvent"></param>
        /// <returns></returns>
        Task<Result<MlrbId>> UpsertAsync(DapperCommandContext context, SimpleRangeEvent  simpleRangeEvent);
    }
}
