namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Defines methods for managing simple range events. Provides functionality for
    ///     creating, retrieving, updating, deleting, and exporting simple range event records.
    /// </summary>
    [Obsolete("Trying to change this to use events.", true)]
    public interface ISimpleRangeSessionService
    {
        /// <summary>
        ///     Delete a SimpleRangeEvent document.
        /// </summary>
        /// <param name="simpleRangeSession">
        ///     The simple range event to delete.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token to observe while waiting for the task to complete. This is optional and defaults to None.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous deletion operation. It contains the result indicating the success or
        ///     failure of the operation.
        /// </returns>
        Task<Result> DeleteAsync(SimpleRangeSession simpleRangeSession, CancellationToken cancellationToken = default);


        /// <summary>
        ///     Retrieves a simple range event by its identifier from the database.
        /// </summary>
        /// <param name="simpleRangeEventId">The identifier of the simple range event to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task representing the asynchronous operation, containing the result of the retrieval operation with the
        ///     simple range event data.
        /// </returns>
        Task<Result<SimpleRangeSession>> GetAsync(Guid              simpleRangeEventId,
                                                CancellationToken cancellationToken = default);

        /// <summary>
        ///     Insert or update a SimpleRangeEvent document. If the document exists, it will be updated;
        ///     otherwise, a new document will be created.
        /// </summary>
        /// <param name="simpleRangeSession">
        ///     The simple range event to insert or update.
        /// </param>
        /// <param name="isNew"></param>
        /// <param name="cancellationToken">
        ///     A cancellation token to observe while waiting for the task to complete. This is optional and defaults to None.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation. It contains the result, including the unique
        ///     identifier of the inserted or updated SimpleRangeEvent.
        /// </returns>
        Task<Result<Guid>> UpsertAsync(SimpleRangeSession  simpleRangeSession,
                                       bool              isNew             = false,
                                       CancellationToken cancellationToken = default);


        /// <summary>
        ///     Retrieves a collection of SimpleRangeEvent objects asynchronously.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A cancellation token to observe while waiting for the task to complete. This is optional and defaults to None.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation. It contains a result object which includes a collection of
        ///     SimpleRangeEvent objects or an error indicating the failure of the operation.
        /// </returns>
        Task<Result<IEnumerable<SimpleRangeSession>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes the SimpleRangeEvents documents to a CSV file.
        /// </summary>
        /// <param name="csvFileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> ExportToCsv(string csvFileName, CancellationToken cancellationToken = default);
    }
}