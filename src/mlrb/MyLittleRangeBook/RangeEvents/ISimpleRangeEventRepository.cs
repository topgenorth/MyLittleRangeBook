namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Provides an interface for managing and performing operations on simple range events
    ///     in a repository. This includes CRUD operations such as creating, updating, retrieving,
    ///     and deleting simple range events.
    /// </summary>
    public interface ISimpleRangeEventRepository
    {
        /// <summary>
        ///     Deletes the specified simple range event from the repository.
        /// </summary>
        /// <param name="simpleRangeEvent">The simple range event to be deleted.</param>
        /// <param name="cancellationToken">A token to cancel the operation if required.</param>
        /// <returns>A result indicating whether the deletion was successful.</returns>
        Task<Result> DeleteAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Adds or updates a simple range event. If necessary, a new Firearm record will also be created.
        /// </summary>
        /// <param name="simpleRangeEvent">The simple range event to add or update.</param>
        /// <param name="cancellationToken">A token to cancel the operation if required.</param>
        /// <returns>A result containing the ID of the added or updated record, or null if the operation fails.</returns>
        Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Retrieves a collection of simple range events.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result containing an enumerable of simple range events, or an error if the operation fails.</returns>
        Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(CancellationToken cancellationToken =
            default);

        /// <summary>
        ///     Retrieves a specific SimpleRangeEvent by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the SimpleRangeEvent to retrieve.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result containing the requested SimpleRangeEvent if successful or an error if the operation fails.</returns>
        Task<Result<SimpleRangeEvent>> GetAsync(string id, CancellationToken cancellationToken);
    }
}
