using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.EventSourcing
{
    public class SqliteEventSourcingService : IEventSourcingService
    {
        readonly IEventSerializer _eventSerializer;

        public SqliteEventSourcingService(IEventSerializer eventSerializer) => _eventSerializer = eventSerializer;

        public async Task<IEnumerable<IDomainEvent>> GetDomainEvents(DapperCommandContext context, MlrbId streamId)
        {
            IEnumerable<EventRow> rows = await GetEventRows(context, streamId).ConfigureAwait(false);
            return rows.Select(r => r.ToDomainEvent(_eventSerializer));
        }

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
        public async Task<EventStreamRow?> GetEventStream(DapperCommandContext context, MlrbId streamId)
        {
            DapperCommandContext ctx = context with { Arguments = new { StreamId = streamId.ToString() } };

            EventStreamRow? es;
            try
            {
                es = await EventSourcingCommands.s_selectStream.QuerySingleAsync<EventStreamRow>(ctx)
                                                .ConfigureAwait(false);
            }
            catch (InvalidOperationException e1)
            {
                // [TO20260530] This means that the stream doesn't exist in the database; return null.
                if ("Sequence contains no elements".Equals(e1.Message, StringComparison.OrdinalIgnoreCase))
                {
                    es = null;
                }
                else
                {
                    throw;
                }
            }

            return es;
        }

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
        /// <returns>
        ///     A task that completes when the domain event has been successfully inserted into the event stream.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the insertion of the domain event fails, either due to database errors
        ///     or if it does not seem a record was added.
        /// </exception>
        public async Task InsertDomainEvent(DapperCommandContext context,
                                            MlrbId               streamId,
                                            string               streamType,
                                            IDomainEvent         domainEvent,
                                            int                  version
        )
        {
            string  dataJson     = _eventSerializer.Serialize(domainEvent);
            string? metadataJson = domainEvent.MetadataJson;
            var args = new
                       {
                           StreamId   = streamId,
                           Id         = new MlrbId(domainEvent.OccurredUtc),
                           StreamType = streamType,
                           domainEvent.EventType,
                           Version = version,
                           domainEvent.OccurredUtc,
                           DataJson     = dataJson,
                           MetadataJson = metadataJson,
                       };
            DapperCommandContext ctx = context with { Arguments = args };
            int                  rowCount;
            try
            {
                rowCount = await EventSourcingCommands.s_insertEvent.ExecuteAsync(ctx).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new
                    InvalidOperationException($"Failed to insert event of type '{domainEvent.EventType}' for stream '{streamId}'.",
                                              ex);
            }

            if (rowCount != 1)
            {
                throw new
                    InvalidOperationException($"Failed to insert event of type '{domainEvent.EventType}' for stream '{streamId}'.");
            }
        }

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
        public async Task UpsertEventStream(DapperCommandContext context,
                                            Aggregate            aggregate,
                                            string               streamType,
                                            int                  version,
                                            string?              metadataJson = null)
        {
            var args = new
                       {
                           StreamId = aggregate.Id,
                           Type     = streamType,
                           aggregate.Version,
                           MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson,
                       };
            DapperCommandContext ctx = context with { Arguments = args };
            int                  rowCount;
            Exception?           innerEx = null;
            try
            {
                rowCount = await EventSourcingCommands.s_upsertEventStream.ExecuteAsync(ctx).ConfigureAwait(false);
            }
            catch (Exception e1)
            {
                innerEx  = e1;
                rowCount = 0;
            }

            if (rowCount != 1)
            {
                if (innerEx is null)
                {
                    throw new InvalidOperationException($"Failed to update event stream version for {aggregate.Id}.");
                }

                throw new InvalidOperationException($"Failed to update event stream version for {aggregate.Id}.",
                                                    innerEx);
            }
        }

        /// <summary>
        ///     Retrieves a collection of event rows associated with the specified stream ID from the database.
        /// </summary>
        /// <param name="context">
        ///     The command context containing the database connection, transaction (if any), cancellation token,
        ///     and additional arguments required for executing the query.
        /// </param>
        /// <param name="streamId">
        ///     The unique identifier of the event stream whose associated event rows are to be retrieved.
        /// </param>
        /// <returns>
        ///     A collection of <see cref="EventRow" /> objects representing the events associated with the specified stream ID.
        /// </returns>
        public async Task<IEnumerable<EventRow>> GetEventRows(DapperCommandContext context, MlrbId streamId)
        {
            DapperCommandContext ctx = context with { Arguments = new { StreamId = streamId.ToString() } };
            IEnumerable<EventRow> rows = await EventSourcingCommands.s_selectEventsCommand.QueryAsync<EventRow>(ctx)
                                                                    .ConfigureAwait(false);
            return rows;
        }
    }
}