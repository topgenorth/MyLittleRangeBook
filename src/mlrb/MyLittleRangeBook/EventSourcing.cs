using System.Data.Common;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterDomainEventSerializers(this IServiceCollection services)
        {
            services.AddScoped<IEventSerializer, SystemTextJsonEventSerializer>(serviceProvider =>
            {
                var l = new List<Type>();
                l.AddRange(SupportedRangeAssetEvents);
                l.AddRange(SupportedFirearmsEvents);

                return new SystemTextJsonEventSerializer(l);
            });

            return services;
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
        const string SelectStreamSql = """
                                       SELECT id AS StreamId, stream_type AS StreamType, version AS Version,
                                              created_utc as Created, modified_utc as Modified
                                       FROM event_streams
                                       WHERE id = @StreamId;
                                       """;

        const string SelectEventsSql = """
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

        readonly Func<EventStream, TAggregate> _createFromStream;
        readonly IEventSerializer _eventSerializer;
        protected readonly ISqliteHelper SqliteHelper;
        readonly string _streamType;

        /// <param name="sqliteHelper">SQLite connection factory.</param>
        /// <param name="eventSerializer">Serializer used to (de)serialize <see cref="IDomainEvent" /> instances.</param>
        /// <param name="streamType">The stream type identifier used in the <c>event_streams</c>/<c>events</c> tables.</param>
        /// <param name="createFromStream">Factory invoked when rehydrating an aggregate from an existing event stream.</param>
        public SqliteAggregateRepository(ISqliteHelper sqliteHelper,
            IEventSerializer eventSerializer,
            string streamType,
            Func<EventStream, TAggregate> createFromStream)
        {
            SqliteHelper = sqliteHelper;
            _eventSerializer = eventSerializer;
            _streamType = streamType;
            _createFromStream = createFromStream;
        }

        public async Task<Result<TAggregate?>> GetAsync(MlrbId id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using SqliteConnection connection =
                    await SqliteHelper.GetDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);

                IReadOnlyList<EventRow> eventRows = await LoadEventRowsAsync(connection, id, cancellationToken)
                    .ConfigureAwait(false);
                if (eventRows.Count == 0)
                {
                    // [TO20260530] No events; this is okay because it means this is a new thing.
                    return Result.Ok<TAggregate?>(null);
                }

                EventStream? stream = await LoadStreamAsync(connection, id, cancellationToken).ConfigureAwait(false);
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

        async Task<EventStream?> LoadStreamAsync(SqliteConnection connection,
            MlrbId streamId,
            CancellationToken ct)
        {
            var ctx = new DapperCommandContext(connection, null, ct, new { StreamId = streamId.ToString() });

            var cmd = new DapperCommand(SelectStreamSql);

            EventStream? es;
            try
            {
                es = await cmd.QuerySingleAsync<EventStream>(ctx).ConfigureAwait(false);
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

        async Task<IReadOnlyList<EventRow>> LoadEventRowsAsync(SqliteConnection connection,
            MlrbId streamId,
            CancellationToken ct)
        {
            var ctx = new DapperCommandContext(connection, null, ct, new { StreamId = streamId.ToString() });
            var cmd = new DapperCommand(SelectEventsSql);
            IEnumerable<EventRow> rows = await cmd.QueryAsync<EventRow>(ctx).ConfigureAwait(false);

            return rows as EventRow[] ?? rows.ToArray();
        }

        void Replay(TAggregate aggregate, IReadOnlyList<EventRow> rows)
        {
            IEnumerable<IDomainEvent> events = rows.Select(row =>
                (IDomainEvent)_eventSerializer.Deserialize(row.EventType, row.DataJson));
            aggregate.LoadFromHistory(events);
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
                await SqliteHelper.GetDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);
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

                await ProjectAsync(connection, transaction, streamId, pendingEvents, cancellationToken)
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

        /// <summary>
        ///     Hook invoked after events are persisted but before the transaction is committed. Override
        ///     to project the just-saved events into additional read-model tables within the same transaction.
        /// </summary>
        protected virtual Task ProjectAsync(SqliteConnection connection,
            DbTransaction transaction,
            string streamId,
            IReadOnlyList<IDomainEvent> pendingEvents,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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

            var ctx = new DapperCommandContext(conn, trans, ct, p);
            var cmd = new DapperCommand(sql);
            int i = await cmd.ExecuteAsync(ctx).ConfigureAwait(false);
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
            var cmd = new DapperCommand(SQL);
            var ctx = new DapperCommandContext(connection, transaction, ct, p);
            int x;
            try
            {
                x = await cmd.ExecuteAsync(ctx).ConfigureAwait(false);
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
            var ctx = new DapperCommandContext(c, t, ct, new { StreamId = streamId });
            var versionCmd = new DapperCommand("SELECT version from event_streams WHERE id=@StreamId;");
            int? currentVersion = await versionCmd.ExecuteScalarAsync<int?>(ctx).ConfigureAwait(false);

            return currentVersion;
        }
    }

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

        /// <summary>
        ///     Replays a sequence of historical events against the aggregate via <see cref="Apply" />,
        ///     without enqueueing them as uncommitted. The aggregate's <see cref="Version" /> is expected
        ///     to have been set via <see cref="Hydrate" /> from the corresponding <see cref="EventStream" />.
        /// </summary>
        protected internal void LoadFromHistory(IEnumerable<IDomainEvent> events)
        {
            foreach (IDomainEvent e in events)
            {
                Apply(e);
            }
        }
    }

    /// <summary>
    ///     Represents an attribute used to associate a name with a specific event type.
    ///     This attribute is intended to be applied to structures that represent domain events,
    ///     facilitating their identification and serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class EventTypeAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    /// <summary>
    /// </summary>
    /// <param name="StreamId">A unique value that represents the event .</param>
    /// <param name="StreamType"></param>
    /// <param name="Version"></param>
    /// <param name="Created"></param>
    /// <param name="Modified"></param>
    public record struct EventStream(
        string StreamId,
        string StreamType,
        int Version,
        DateTimeOffset Created,
        DateTimeOffset Modified);

    public record struct EventRow(
        string StreamId,
        string StreamType,
        string EventType,
        int Version,
        string DataJson,
        string MetadataJson,
        DateTimeOffset OccurredUtc,
        DateTimeOffset Created,
        DateTimeOffset Modified);


    public record RangeAssetProjectorContext(
        SqliteConnection Connection,
        DbTransaction Transaction,
        MlrbId RangeAssetId,
        IReadOnlyList<IDomainEvent> PendingEvents,
        CancellationToken CancellationToken = default);


    public interface IDomainEvent
    {
        MlrbId StreamId { get; }
        DateTimeOffset OccurredUtc { get; }
    }

    /// <summary>
    ///     Will serialize the domain event to JSON.
    /// </summary>
    public interface IEventSerializer
    {
        string GetEventType(object @event);
        string Serialize(object domainEvent);
        object Deserialize(string rowEventType, string rowDataJson);
    }

    /// <summary>
    ///     Defines functionality for projecting domain events related to file imports into a storage system.
    /// </summary>
    public interface IProjector
    {
            Task<Result> ProjectAggregateAsync(DapperCommandContext context, MlrbId streamId,
                IEnumerable<IDomainEvent>? domainEvents = null, CancellationToken cancellationToken = default);
    }
}
