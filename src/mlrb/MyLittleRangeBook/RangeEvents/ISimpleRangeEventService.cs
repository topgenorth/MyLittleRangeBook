using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Defines methods for managing simple range events. Provides functionality for
    ///     creating, retrieving, updating, deleting, and exporting simple range event records.
    /// </summary>
    public interface ISimpleRangeEventService
    {
        /// <summary>
        ///     Deletes a record in the simple_range_event table.
        /// </summary>
        /// <param name="context">The command context containing connection, transaction, and other configurations.</param>
        /// <param name="simpleRangeEvent">The simple range event to be deleted.</param>
        /// <returns>A task that represents the asynchronous operation, containing the result of the delete operation.</returns>
        Task<Result> DeleteAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent);

        /// <summary>
        ///     Retrieves a simple range event by its identifier from the database.
        /// </summary>
        /// <param name="context">The command context containing the database connection, transaction, and other configurations.</param>
        /// <param name="simpleRangeEventId">The identifier of the simple range event to retrieve.</param>
        /// <returns>
        ///     A task representing the asynchronous operation, containing the result of the retrieval operation with the
        ///     simple range event data.
        /// </returns>
        Task<Result<SimpleRangeEvent>> GetAsync(DapperCommandContext context, MlrbId simpleRangeEventId);

        /// <summary>
        ///     Insert or update a record in the simple_range_event table.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="simpleRangeEvent"></param>
        /// <returns></returns>
        Task<Result<MlrbId>> UpsertAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent);

        /// <summary>
        ///     Fetches all simple range events
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(DapperCommandContext context);

        /// <summary>
        ///     Exports range event records to a CSV file.
        /// </summary>
        /// <param name="context">The command context containing connection and transaction details.</param>
        /// <param name="csvFileName">The name of the CSV file to which the range event data will be exported.</param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        Task<Result> ExportToCsv(DapperCommandContext context, string csvFileName);

        /// <summary>
        ///     Retrieves a collection of ammo descriptions that were used in simple range trips.
        /// </summary>
        /// <param name="context">The command context containing database connection, transaction, and other configurations.</param>
        /// <returns>A task that represents the asynchronous operation, containing the result of a collection of ammo descriptions.</returns>
        Task<Result<IEnumerable<string>>> GetAmmoDescriptions(DapperCommandContext context);
    }
}