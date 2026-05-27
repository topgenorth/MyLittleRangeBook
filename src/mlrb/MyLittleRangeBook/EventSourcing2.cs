using System.Data.Common;
using System.Reflection;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    /// <summary>
    ///     Abstract base class for event-sourced aggregates. Subclasses are responsible for
    ///     implementing the <see cref="Apply" /> method to mutate state in response to domain events.
    ///     The base class retains the uncommitted event list as well as the <see cref="Id" />,
    ///     <see cref="Version" />, <see cref="StreamType" /> and <see cref="DefaultStreamType" /> properties.
    /// </summary>
    public abstract class Aggregate
    {
        readonly List<IDomainEvent> _uncommitted = [];

        protected Aggregate()
        {
            StreamType = DefaultStreamType;
        }

        /// <summary>
        ///     The default stream type for this aggregate. Subclasses must override this with a constant
        ///     string that uniquely identifies the type of stream the aggregate represents.
        /// </summary>
        public abstract string DefaultStreamType { get; }

        public MlrbId Id { get; protected set; } = MlrbId.Empty;
        public int Version { get; protected set; } = -1;
        public string StreamType { get; protected set; }

        /// <summary>
        ///     Applies a domain event to mutate the aggregate state. Subclasses must implement this
        ///     to handle their specific event types.
        /// </summary>
        public abstract void Apply(IDomainEvent e);

        /// <summary>
        ///     Raises a new domain event: applies it to the state, appends it to the uncommitted list,
        ///     and increments the version.
        /// </summary>
        public void Raise(IDomainEvent e)
        {
            Apply(e);
            _uncommitted.Add(e);
            Version++;
        }

        /// <summary>
        ///     Returns and clears the list of uncommitted events.
        /// </summary>
        public IReadOnlyList<IDomainEvent> DequeueUncommittedEvents()
        {
            IDomainEvent[] events = _uncommitted.ToArray();
            _uncommitted.Clear();

            return events;
        }

        public void ClearUncommittedEvents()
        {
            _uncommitted.Clear();
        }

        /// <summary>
        ///     Initializes the aggregate from an existing event stream (used during rehydration).
        /// </summary>
        protected void Hydrate(EventStream stream)
        {
            Id = stream.StreamId;
            StreamType = stream.StreamType;
            Version = stream.Version;
        }
    }

    /// <summary>
    ///     Generic SQLite-backed repository for any <see cref="Aggregate" /> subclass. It persists
    ///     uncommitted events to the <c>events</c> table and upserts the corresponding row in the
    ///     <c>event_streams</c> table.
    /// </summary>
    /// <typeparam name="TAggregate">The concrete aggregate type.</typeparam>
    public class SqliteAggregateRepository<TAggregate> where TAggregate : Aggregate
    {
        readonly Func<MlrbId, TAggregate> _createNew;
        readonly Func<EventStream, TAggregate> _createFromStream;
        readonly IEventSerializer _eventSerializer;
        readonly ISqliteHelper _sqliteHelper;
        readonly string _streamType;

        /// <param name="sqliteHelper">SQLite connection factory.</param>
        /// <param name="eventSerializer">Serializer used to (de)serialize <see cref="IDomainEvent" /> instances.</param>
        /// <param name="streamType">The stream type identifier used in the <c>event_streams</c>/<c>events</c> tables.</param>
        /// <param name="createNew">Factory invoked when a stream does not exist and a brand-new aggregate must be constructed for the supplied id.</param>
        /// <param name="createFromStream">Factory invoked when rehydrating an aggregate from an existing event stream.</param>
        public SqliteAggregateRepository(ISqliteHelper sqliteHelper,
            IEventSerializer eventSerializer,
            string streamType,
            Func<MlrbId, TAggregate> createNew,
            Func<EventStream, TAggregate> createFromStream)
        {
            _sqliteHelper = sqliteHelper;
            _eventSerializer = eventSerializer;
            _streamType = streamType;
            _createNew = createNew;
            _createFromStream = createFromStream;
        }

        public async Task<Result<TAggregate>> GetAsync(MlrbId id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using SqliteConnection connection =
                    await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);
                await using DbTransaction transaction =
                    await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                const string selectEvents = """
                                            select stream_id as StreamId,
                                                   stream_type as StreamType,
                                                   version as Version,
                                                   event_type as EventType,
                                                   occurred_utc as OccurredUtc,
                                                   data_json as DataJson,
                                                   metadata_json as MetadataJson
                                            from events
                                            where stream_id = @StreamId
                                            order by version asc;
                                            """;
                var selectEventsCmd = new DapperCommand(selectEvents, new { StreamId = id.ToString() });
                IEnumerable<EventRow> rows = await selectEventsCmd
                    .QueryAsync<EventRow>(connection, transaction, cancellationToken)
                    .ConfigureAwait(false);

                EventRow[] eventRows = rows as EventRow[] ?? rows.ToArray();
                if (eventRows.Length == 0)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                    return Result.Ok();
                }

                Result<EventStream?> r = await GetEventStreamAsync(connection, transaction, id, cancellationToken)
                    .ConfigureAwait(false);

                bool isNew = r.IsFailed || r.Value is null;

                TAggregate aggregate = isNew
                    ? _createNew(id)
                    : _createFromStream(r.Value!.Value);

                foreach (EventRow row in eventRows)
                {
                    var evt = (IDomainEvent)_eventSerializer.Deserialize(row.EventType, row.DataJson);
                    aggregate.Apply(evt);
                }

                aggregate.ClearUncommittedEvents();

                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                return Result.Ok(aggregate);
            }
            catch (Exception e)
            {
                return e.FailWithException();
            }
        }

        public async Task<Result> SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<IDomainEvent> pendingEvents = aggregate.DequeueUncommittedEvents();
            if (pendingEvents.Count == 0)
            {
                return Result.Ok();
            }

            var streamId = aggregate.Id.ToString();

            await using SqliteConnection connection =
                await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using DbTransaction transaction =
                await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                int? currentVersion = await GetStreamVersion(connection, transaction, streamId, cancellationToken)
                    .ConfigureAwait(false);
                int expectedVersion, nextVersion;
                if (currentVersion is null)
                {
                    expectedVersion = aggregate.Version;
                    nextVersion = 0;
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
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                        return Result.Fail("Operation was cancelled.");
                    }

                    await InsertDomainEventAsync(connection,
                            transaction,
                            streamId,
                            nextVersion,
                            evt,
                            cancellationToken)
                        .ConfigureAwait(false);
                    nextVersion++;
                }

                await UpsertEventStreamAsync(connection,
                        transaction,
                        streamId,
                        currentVersion,
                        nextVersion - 1,
                        cancellationToken)
                    .ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                    return Result.Fail("Operation was cancelled.");
                }

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                Error err = new Error(e.Message).CausedBy(e);

                return Result.Fail(err);
            }

            return Result.Ok();
        }

        async Task<Result<EventStream?>> GetEventStreamAsync(SqliteConnection c,
            DbTransaction t,
            MlrbId streamId,
            CancellationToken ct)
        {
            try
            {
                const string SQL = """
                                   SELECT id AS StreamId, stream_type AS StreamType, version AS Version,
                                          created_utc as Created, modified_utc as Modified
                                   FROM event_streams
                                   WHERE id = @StreamId;
                                   """;
                var cmd = new DapperCommand(SQL, new { StreamId = streamId.ToString() });
                EventStream? stream = await cmd.QuerySingleAsync<EventStream>(c, t, ct).ConfigureAwait(false);

                return Result.Ok(stream);
            }
            catch (Exception ex)
            {
                return ex.FailWithException();
            }
        }

        async Task UpsertEventStreamAsync(SqliteConnection conn,
            DbTransaction trans,
            string streamId,
            int? currentVersion,
            int nextVersion,
            CancellationToken ct)
        {
            string sql;
            object? p;
            if (currentVersion is null)
            {
                sql = """
                      insert into event_streams (id,  stream_type, version)
                      values (@StreamId, @Type, @Version);
                      """;
                p = new { StreamId = streamId, Type = _streamType, Version = nextVersion };
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
                p = new { Version = nextVersion, StreamId = streamId, ExpectedVersion = currentVersion };
            }

            var cmd = new DapperCommand(sql, p);
            int i = await cmd.ExecuteAsync(conn, trans, ct).ConfigureAwait(false);
            if (i != 1)
            {
                throw new InvalidOperationException($"Failed to update event stream version for {streamId}");
            }
        }

        async Task InsertDomainEventAsync(SqliteConnection connection,
            DbTransaction transaction,
            string streamId,
            int nextVersion,
            IDomainEvent domainEvent,
            CancellationToken ct)
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

            Type t = domainEvent.GetType();
            string eventType = t.GetCustomAttribute<EventTypeAttribute>()?.Name ?? t.Name;
            var p = new
            {
                StreamId = streamId,
                Id = new MlrbId(domainEvent.OccurredUtc).ToString(),
                StreamType = _streamType,
                Version = nextVersion,
                EventType = eventType,
                domainEvent.OccurredUtc,
                DataJson = _eventSerializer.Serialize(domainEvent),
                MetadataJson = "{}"
            };
            var cmd = new DapperCommand(SQL, p);
            int x;
            try
            {
                x = await cmd.ExecuteAsync(connection, transaction, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to insert event of type {domainEvent.GetType().Name} for stream {streamId}",
                    ex);
            }

            if (x != 1)
            {
                throw new InvalidOperationException(
                    $"Failed to insert event of type {domainEvent.GetType().Name} for stream {streamId}");
            }
        }

        async Task<int?> GetStreamVersion(SqliteConnection c,
            DbTransaction t,
            string streamId,
            CancellationToken ct)
        {
            var versionCmd = new DapperCommand("SELECT version from event_streams WHERE id=@StreamId;",
                new { StreamId = streamId });
            int? currentVersion = await versionCmd.ExecuteScalarAsync<int?>(c, t, ct).ConfigureAwait(false);

            return currentVersion;
        }
    }
}
