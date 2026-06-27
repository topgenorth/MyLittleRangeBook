using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.EventSourcing
{
    public interface IEventSourcingService
    {
        /// <summary>
        /// Retrieves the domain events associated with the specified stream ID from the event store.
        /// </summary>
        /// <param name="context">
        /// The command context containing the database connection, transaction (if any), cancellation token,
        /// and additional arguments required for executing the query.
        /// </param>
        /// <param name="streamId">
        /// The unique identifier of the event stream for which domain events are to be retrieved.
        /// </param>
        /// <returns>
        /// A collection of <see cref="IDomainEvent" /> items representing the domain events associated with the
        /// specified stream ID.
        /// </returns>
        Task<IEnumerable<IDomainEvent>> GetDomainEvents(DapperCommandContext context, MlrbId streamId);

        /// <summary>
        ///     Retrieves the event stream associated with the specified stream ID from the database.
        /// </summary>
        /// <param name="context">
        ///     The command context containing the database connection, transaction (if any), cancellation token,
        ///     and additional arguments required for executing the query.
        /// </param>
        /// <param name="streamId">
        ///     The unique identifier of the event stream to retrieve.
        /// </param>
        /// <returns>
        ///     An <see cref="EventStreamRow" /> representing the event stream if it exists, or null if no stream is found.
        /// </returns>
        Task<EventStreamRow?> GetEventStream(DapperCommandContext context, MlrbId streamId);

        /// <summary>
        ///     Inserts a domain event into the event stream with the specified parameters and persists it to storage.
        /// </summary>
        /// <param name="context">
        ///     The command context containing the database connection, transaction (if any), cancellation token,
        ///     and optional arguments required for executing the database operation.
        /// </param>
        /// <param name="streamId">
        ///     The unique identifier of the event stream for which the event is being added.
        /// </param>
        /// <param name="streamType">
        ///     The type of the event stream, used to differentiate between different kinds of event streams.
        /// </param>
        /// <param name="domainEvent">
        ///     The domain event instance containing the event data and the time of occurrence.
        /// </param>
        /// <param name="version">
        ///     The expected version of the event stream after insertion of the new event.
        /// </param>
        /// <param name="metadataJson">
        ///     An optional JSON string containing metadata to associate with the domain event. If null or empty, a default empty
        ///     JSON object is used.
        /// </param>
        /// <returns>
        ///     A task that completes when the domain event has been successfully inserted into the event stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the insertion of the domain event fails, either due to database errors
        ///     or if it does not seem a record was added.
        /// </exception>
        Task InsertDomainEvent(DapperCommandContext context,
                               MlrbId               streamId,
                               string               streamType,
                               IDomainEvent         domainEvent,
                               int                  version,
                               string?              metadataJson = null
        );

        /// <summary>
        ///     Inserts or updates the event stream entry in the database for the specified aggregate.
        /// </summary>
        /// <param name="context">
        ///     The command context containing the database connection, transaction (if any), cancellation token,
        ///     and additional arguments required for executing the query.
        /// </param>
        /// <param name="aggregate">
        ///     The aggregate whose event stream entry is to be upserted, including its identifier and version.
        /// </param>
        /// <param name="streamType">
        ///     The type of the event stream associated with the specified aggregate.
        /// </param>
        /// <param name="version"></param>
        /// <param name="metadataJson">
        ///     Optional metadata in JSON format to associate with the event stream. Defaults to an empty JSON object ("{}") if not
        ///     provided.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the event stream update operation fails, or the number of affected rows in the database is not exactly 1.
        /// </exception>
        Task UpsertEventStream(DapperCommandContext context,
                               Aggregate            aggregate,
                               string               streamType,
                               int                  version,
                               string               metadataJson = "{}");
    }
}