using System.Globalization;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    public interface IFirearmAggregateRepository: ISqliteAggregateRepository<FirearmAggregate>
    {
        /// <summary>
        ///     Retrieves a list of firearm names in SimpleRangeEvents that do not have an associated FirearmAggregrate.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [Obsolete("Use a projector", true)]
        Task<Result<IEnumerable<NewFirearmNameFromSimpleRangeEventRow>>> GetNewFirearmNamesFromSimpleRangeEventsAsync(
            DapperCommandContext context);


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
        ///     Retrieves a list of simple range events associated with a firearm, identified by its name.
        /// </summary>
        /// <param name="context">The context for the database command, providing connection and transaction details.</param>
        /// <param name="name">The name of the firearm for which to retrieve range events.</param>
        /// <returns>A task representing the asynchronous operation, returning a list of range event round count rows.</returns>
        [Obsolete("Use a projector", true)]
        Task<IEnumerable<RangeEventRoundCountRow>> GetSimpleRangeEventRoundCountsByFirearmNameAsync(
            DapperCommandContext context, string name);

        /// <summary>
        ///     Saves the specified firearm aggregate to the repository.
        /// </summary>
        /// <param name="aggregate">The firearm aggregate to be saved.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result indicating success or failure of the save operation.</returns>
        Task<Result> SaveAsync(FirearmAggregate aggregate, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Represents a record containing the name of a firearm extracted from a SimpleRangeEvent
        ///     in cases where no associated FirearmAggregate exists. It is used to process and handle
        ///     events where firearm names need to be retrieved or linked to an aggregate.
        /// </summary>
        /// <remarks>
        ///     The <see cref="NewFirearmNameFromSimpleRangeEventRow" /> struct stores the essential data
        ///     from SimpleRangeEvents for firearms that have not yet been linked with their respective
        ///     aggregates in the system. It includes the event ID, firearm name, and creation timestamp.
        /// </remarks>
        /// <param name="SimpleRangeEventId">
        ///     The unique identifier for the oldest SimpleRangeEvent in the database.
        /// </param>
        /// <param name="FirearmName">
        ///     The name of the firearm retrieved from the SimpleRangeEvent.
        /// </param>
        /// <param name="Created">
        ///     The creation timestamp of the SimpleRangeEvent in string format.
        /// </param>
        readonly record struct NewFirearmNameFromSimpleRangeEventRow(
            string SimpleRangeEventId,
            string FirearmName,
            string Created)
        {
            public DateTimeOffset CreatedUtc => DateTimeOffset.Parse(Created, null, DateTimeStyles.AssumeLocal);
        }

        readonly record struct RangeEventThatIsNotAssociatedWithFirearmRow(
            string SimpleRangeEventId,
            string FirearmName,
            string Created)
        {
            public DateTimeOffset CreatedUtc => DateTimeOffset.Parse(Created, null, DateTimeStyles.AssumeLocal);
        }

        /// <summary>
        ///     Represents a record containing data about the round count for a specific range event.
        ///     This record is utilized to track the number of rounds fired during a range event
        ///     and capture the associated event details.
        /// </summary>
        /// <remarks>
        ///     The <see cref="RangeEventRoundCountRow" /> struct is primarily used to process and store
        ///     information about simple range events, including the unique event identifier, the
        ///     number of rounds fired, and the event's date. It includes a helper property to
        ///     convert the event date string into a <see cref="DateTimeOffset" /> instance for further use.
        /// </remarks>
        /// <param name="SimpleRangeEventId">
        ///     The unique identifier for the range event.
        /// </param>
        /// <param name="RoundsFired">
        ///     The total number of rounds fired during the range event.
        /// </param>
        /// <param name="EventDate">
        ///     The date of the range event, represented as a string.
        /// </param>
        readonly record struct RangeEventRoundCountRow(
            string SimpleRangeEventId,
            string FirearmName,
            int    RoundsFired,
            string EventDate)
        {
            public DateTimeOffset CreatedUtc => DateTimeOffset.Parse(EventDate, null, DateTimeStyles.AssumeLocal);
        }
    }
}