using System.Reflection;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.EventSourcing
{
    /// <summary>
    ///     Generic SQLite-backed repository for any <see cref="Aggregate" /> subclass. It persists
    ///     uncommitted events to the <c>events</c> table and upserts the corresponding row in the
    ///     <c>event_streams</c> table.
    /// </summary>
    /// <typeparam name="TAggregate">The concrete aggregate type.</typeparam>
    public abstract class SqliteAggregateRepository<TAggregate> : ISqliteAggregateRepository<TAggregate> where TAggregate : Aggregate
    {
        readonly           Func<EventStreamRow, TAggregate> _createFromStream;
        readonly           IEventSerializer              _eventSerializer;
        readonly           string                        _streamType;
        protected readonly ISqliteHelper                 SqliteHelper;

        /// <param name="sqliteHelper">SQLite connection factory.</param>
        /// <param name="eventSerializer">Serializer used to (de)serialize <see cref="IDomainEvent" /> instances.</param>
        /// <param name="streamType">The stream type identifier used in the <c>event_streams</c>/<c>events</c> tables.</param>
        /// <param name="createFromStream">Factory invoked when rehydrating an aggregate from an existing event stream.</param>
        protected SqliteAggregateRepository(ISqliteHelper                 sqliteHelper,
                                            IEventSerializer              eventSerializer,
                                            string                        streamType,
                                            Func<EventStreamRow, TAggregate> createFromStream)
        {
            SqliteHelper      = sqliteHelper;
            _eventSerializer  = eventSerializer;
            _streamType       = streamType;
            _createFromStream = createFromStream;
        }

        /// <summary>
        ///     Retrieves an aggregate by its unique identifier from the event sourcing store. Does not load any prior events.
        /// </summary>
        /// <param name="context">The database command context, including connection, transaction (if any), and cancellation token.</param>
        /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
        /// <returns>A result containing the aggregate if found, or <c>null</c> if no events exist for the specified identifier.</returns>
        public virtual async Task<Result<TAggregate?>> GetAsync(DapperCommandContext context, MlrbId id)
        {
            try
            {
                IReadOnlyList<EventRow> eventRows = await LoadEventRowsAsync(context, id).ConfigureAwait(false);
                if (eventRows.Count == 0)
                {
                    // [TO20260530] No events; this is okay because it means this is a new thing.
                    return Result.Ok<TAggregate?>(null);
                }

                EventStreamRow? stream = await LoadStreamAsync(context, id).ConfigureAwait(false);
                if (stream is null)
                {
                    // [TO20260530] We couldn't find the stream; this is okay because it means this is a new thing.
                    return Result.Ok<TAggregate?>(null);
                }


                TAggregate aggregate = _createFromStream(stream.Value);
                Replay(aggregate, eventRows);

                return Result.Ok<TAggregate?>(aggregate);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                return e.FailWithException();
            }
        }

        /// <summary>
        ///     Retrieves a collection of domain events associated with the specified stream identifier.
        /// </summary>
        /// <param name="context">The database command context that provides the SQLite connection and transaction.</param>
        /// <param name="streamId">The identifier of the event stream whose domain events are to be retrieved.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of domain events.</returns>
        public async Task<Result<IEnumerable<IDomainEvent>>> GetDomainEvents(
            DapperCommandContext context, MlrbId streamId)
        {
            try
            {
                DapperCommandContext ctx = context with { Arguments = new { StreamId = streamId.ToString() } };
                IEnumerable<EventRow> rows = await Commands.s_selectEventsCommand
                                                           .QueryAsync<EventRow>(ctx)
                                                           .ConfigureAwait(false);
                IEnumerable<IDomainEvent> events = rows.Select(r => (IDomainEvent)_eventSerializer
                                                                  .Deserialize(r.EventType, r.DataJson));
                return Result.Ok(events);
            }
            catch (Exception e)
            {
                return Result.Fail(e.ToError().Enrich(streamId));
            }
        }

        /// <summary>
        ///     Loads an event stream asynchronously from the database by its unique identifier.
        /// </summary>
        /// <param name="context">The command context containing database connection, transaction, and query arguments.</param>
        /// <param name="streamId">The unique identifier of the stream to be loaded.</param>
        /// <returns>Returns the event stream if it exists, or null if no stream is found.</returns>
        async Task<EventStreamRow?> LoadStreamAsync(DapperCommandContext context, MlrbId streamId)
        {
            DapperCommandContext ctx = context with { Arguments = new { StreamId = streamId.ToString() } };

            EventStreamRow? es;
            try
            {
                es = await Commands.s_selectStreamCommand.QuerySingleAsync<EventStreamRow>(ctx).ConfigureAwait(false);
            }
            catch (InvalidOperationException ioex)
            {
                // [TO20260530] This means that the stream doesn't existing in the database; return null.
                if ("Sequence contains no elements".Equals(ioex.Message, StringComparison.OrdinalIgnoreCase))
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
        ///     Asynchronously loads the event rows associated with a specific stream ID from the database.
        /// </summary>
        /// <param name="context">The database command context containing connection, transaction, and related parameters.</param>
        /// <param name="streamId">The unique identifier of the stream for which event rows are being queried.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains a read-only list of
        ///     <see cref="EventRow" /> instances.
        /// </returns>
        async Task<IReadOnlyList<EventRow>> LoadEventRowsAsync(DapperCommandContext context,
                                                               MlrbId               streamId)
        {
            DapperCommandContext ctx = context with { Arguments = new { StreamId = streamId.ToString() } };
            IEnumerable<EventRow> rows = await Commands.s_selectEventsCommand.QueryAsync<EventRow>(ctx)
                                                       .ConfigureAwait(false);
            return rows as EventRow[] ?? rows.ToArray();
        }

        /// <summary>
        ///     Replays a set of domain events onto an aggregate to reconstruct its state.
        /// </summary>
        /// <param name="aggregate">The aggregate instance that will be reconstructed by applying the historical domain events.</param>
        /// <param name="rows">
        ///     A read-only list of event rows containing serialized domain event data used to replay the
        ///     aggregate's event history.
        /// </param>
        void Replay(TAggregate aggregate, IReadOnlyList<EventRow> rows)
        {
            IEnumerable<IDomainEvent> events = rows.Select(row =>
                                                               (IDomainEvent)_eventSerializer.Deserialize(row.EventType,
                                                                   row.DataJson));
            aggregate.LoadFromHistory(events);
        }


        /// <summary>
        ///     Upserts the specified aggregate into the database using the provided Dapper command context.
        ///     If the aggregate does not exist, it will be inserted; if it exists, it will be updated.
        /// </summary>
        /// <param name="context">The Dapper command context containing the database connection and transaction information.</param>
        /// <param name="aggregate">The aggregate entity to upsert.</param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        public async Task<Result> UpsertAsync(DapperCommandContext context, TAggregate aggregate)
        {
            IReadOnlyList<IDomainEvent> pendingEvents = aggregate.DequeueUncommittedEvents();
            if (pendingEvents.Count == 0)
            {
                return Result.Ok();
            }

            string streamId = aggregate.Id.ToString();
            try
            {
                int? currentVersion = await GetStreamVersion(context, streamId).ConfigureAwait(false);
                int  expectedVersion, nextVersion;
                if (currentVersion is null)
                {
                    expectedVersion = aggregate.Version;
                    nextVersion     = 0;
                }
                else
                {
                    expectedVersion = aggregate.Version - pendingEvents.Count;
                    if (currentVersion.Value != expectedVersion)
                    {
                        throw new InvalidOperationException(
                                                            $"Concurrency conflict detected for stream {streamId}. Expected version {expectedVersion}, but actual version is {currentVersion}.");
                    }

                    nextVersion = currentVersion.Value + 1;
                }

                foreach (IDomainEvent evt in pendingEvents)
                {
                    await InsertDomainEventAsync(context, streamId, nextVersion, evt)
                       .ConfigureAwait(false);
                    nextVersion++;
                }

                await UpsertEventStreamAsync(context,
                                             streamId,
                                             currentVersion,
                                             nextVersion - 1)
                   .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Error err = e.ToError("Failed to upsert aggregate").Enrich(aggregate.Id);
                return Result.Fail(err);
            }

            return Result.Ok();
        }


        async Task UpsertEventStreamAsync(DapperCommandContext context,
                                          string               streamId,
                                          int?                 currentVersion,
                                          int                  nextVersion)
        {
            string  sql;
            object? args;

            if (currentVersion is null)
            {
                sql = """
                      insert into event_streams (id,  stream_type, version)
                      values (@StreamId, @Type, @Version);
                      """;
                args = new { StreamId = streamId, Type = _streamType, Version = nextVersion };
            }
            else
            {
                sql = """
                      update event_streams
                      set version = @Version,
                          modified_utc = utcnow()
                      where id = @StreamId
                        and version = @ExpectedVersion;
                      """;
                args = new { Version = nextVersion, StreamId = streamId, ExpectedVersion = currentVersion };
            }

            DapperCommand        cmd = new(sql);
            DapperCommandContext ctx = context with { Arguments = args };
            int                  i   = await cmd.ExecuteAsync(ctx).ConfigureAwait(false);
            if (i != 1)
            {
                throw new InvalidOperationException($"Failed to update event stream version for {streamId}");
            }
        }

        async Task InsertDomainEventAsync(DapperCommandContext context, string streamId, int nextVersion,
                                          IDomainEvent         domainEvent)
        {
            const string SQL = """
                               insert into events
                               (
                                   stream_id,
                                   id,
                                   stream_type,
                                   version,
                                   event_type,
                                   occurred_utc,
                                   data_json,
                                   metadata_json
                               )
                               values
                               (
                                   @StreamId,
                                   @Id,
                                   @StreamType,
                                   @Version,
                                   @EventType,
                                   @OccurredUtc,
                                   @DataJson,
                                   @MetadataJson
                               );
                               """;

            Type   t         = domainEvent.GetType();
            string eventType = t.GetCustomAttribute<EventTypeAttribute>()?.Name ?? t.Name;

            var args = new
                       {
                           StreamId   = streamId,
                           Id         = new MlrbId(domainEvent.OccurredUtc).ToString(),
                           StreamType = _streamType,
                           Version    = nextVersion,
                           EventType  = eventType,
                           domainEvent.OccurredUtc,
                           DataJson     = _eventSerializer.Serialize(domainEvent),
                           MetadataJson = "{}",
                       };

            DapperCommand        cmd = new(SQL);
            DapperCommandContext ctx = context with { Arguments = args };

            int written;
            try
            {
                written = await cmd.ExecuteAsync(ctx).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to insert event of type {t.Name} for stream {streamId}",
                                                    ex);
            }

            if (written != 1)
            {
                throw new InvalidOperationException($"Failed to insert event of type {t.Name} for stream {streamId}");
            }
        }

        async Task<int?> GetStreamVersion(DapperCommandContext context,
                                          string               streamId)
        {
            DapperCommandContext ctx            = context with { Arguments = new { StreamId = streamId } };
            DapperCommand        versionCmd     = new("SELECT version from event_streams WHERE id=@StreamId;");
            int?                 currentVersion = await versionCmd.ExecuteScalarAsync<int?>(ctx).ConfigureAwait(false);

            return currentVersion;
        }

        static class Commands
        {
            const string SelectStreamSql = """
                                           SELECT id AS StreamId,
                                                  stream_type AS StreamType,
                                                  version AS Version,
                                                  created_utc as Created,
                                                  modified_utc as Modified
                                           FROM event_streams
                                           WHERE id = @StreamId;
                                           """;

            const string SelectEventsSql = """
                                           select row_id as RowId,
                                                  stream_id as StreamId,
                                                  stream_type as StreamType,
                                                  event_type as EventType,
                                                  version as Version,
                                                  data_json as DataJson,
                                                  metadata_json as MetadataJson,
                                                  occurred_utc as OccurredUtc,
                                                  created_utc as Created,
                                                  modified_utc as Modified
                                           from events
                                           where stream_id = @StreamId
                                           order by version;
                                           """;

            // ReSharper disable once StaticMemberInGenericType
            internal static readonly DapperCommand s_selectStreamCommand = new(SelectStreamSql);

            // ReSharper disable once StaticMemberInGenericType
            internal static readonly DapperCommand s_selectEventsCommand = new(SelectEventsSql);
        }
    }
}