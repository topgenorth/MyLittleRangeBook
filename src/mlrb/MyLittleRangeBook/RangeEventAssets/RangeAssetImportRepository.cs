using System.Data.Common;
using System.Reflection;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.RangeEventAssets
{
    class SqliteRangeAssetProjector : IRangeAssetProjector
    {
        readonly ILogger _logger;

        public SqliteRangeAssetProjector(ILogger logger)
        {
            _logger = logger;
        }

        public Task ProjectAsync(string toString,
            IReadOnlyList<IDomainEvent> pendingEvents,
            SqliteConnection connection,
            DbTransaction transaction,
            CancellationToken cancellationToken)
        {
            _logger.Warning("Projecting events for RangeAssetImport with {EventCount}", pendingEvents.Count);

            return Task.CompletedTask;
        }
    }

    public class SqliteRangeAssetAggregateRepository : IRangeAssetAggregateRepository
    {
        const string StreamType = "range-asset-import";
        readonly IEventSerializer _eventSerializer;
        readonly IRangeAssetProjector _rangeAssetProjector;
        readonly ISqliteHelper _sqliteHelper;

        public SqliteRangeAssetAggregateRepository(ISqliteHelper sqliteHelper,
            IEventSerializer eventSerializer,
            IRangeAssetProjector rangeAssetProjector)
        {
            _sqliteHelper = sqliteHelper;
            _eventSerializer = eventSerializer;
            _rangeAssetProjector = rangeAssetProjector;
        }

        public async Task<Result<RangeAssetAggregate>> GetAsync(MlrbId id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using SqliteConnection connection =
                    await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);
                await using DbTransaction transaction =
                    await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                #region Load the events first
                var selectEvents = """
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

                IEnumerable<EventRow> eventRows = rows as EventRow[] ?? rows.ToArray();
                if (!eventRows.Any())
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                    return Result.Ok();
                }
                #endregion

                #region Try to load the aggregate from the database; if you can't find one, create a new one.
                RangeAssetAggregate aggregate;
                Result<EventStream?> r = await GetEventStreamAsync(connection, transaction, id, cancellationToken)
                    .ConfigureAwait(false);

                bool isNew = r.IsFailed || r.Value is null;

                aggregate = isNew
                    ? RangeAssetAggregate.New(id)
                    : RangeAssetAggregate.Create(r.Value!.Value);

                foreach (EventRow row in eventRows)
                {
                    var evt = (IDomainEvent)_eventSerializer.Deserialize(row.EventType, row.DataJson);
                    aggregate.Apply(evt);
                }

                aggregate.ClearUncommittedEvents();
                #endregion


                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                return Result.Ok(aggregate);
            }
            catch (Exception e)
            {
                return e.FailWithException();
            }
        }

        public async Task<Result> SaveAsync(RangeAssetAggregate aggregate,
            CancellationToken cancellationToken = default)
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
                    // [TO20260526] Should never have an negative expected version!
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

                    nextVersion++;
                    await InsertDomainEventAsync(connection, transaction, streamId, nextVersion, evt, cancellationToken);
                }

                await UpsertEventStreamAsync(connection,
                    transaction,
                    aggregate.Id.ToString(),
                    currentVersion,
                    nextVersion,
                    cancellationToken);


                await _rangeAssetProjector
                    .ProjectAsync(aggregate.Id.ToString(), pendingEvents, connection, transaction, cancellationToken)
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
                Error? err = new Error(e.Message).CausedBy(e);

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
                p = new { StreamId = streamId, Type = StreamType, Version = nextVersion };
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
                p = new { Version = nextVersion, StreamId = streamId,  ExpectedVersion = currentVersion };
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

            // TODO [TO20260525] Kind of expensive; optimize this by caching the event type names in a dictionary
            Type t = domainEvent.GetType();
            string eventType = t.GetCustomAttribute<EventTypeAttribute>()?.Name ?? t.Name;
            var p = new
            {
                StreamId = streamId,
                Id = new MlrbId(domainEvent.OccurredUtc).ToString(),
                StreamType,
                Version = nextVersion,
                EventType = eventType,
                domainEvent.OccurredUtc,
                DataJson = _eventSerializer.Serialize(domainEvent),
                MetadataJson = "{}"
            };
            var cmd = new DapperCommand(SQL, p);
            var x = 0;
            try
            {
                x = await cmd.ExecuteAsync(connection, transaction, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                x = -1;

                throw new InvalidOperationException(
                    $"Failed to insert event of type {domainEvent.GetType().Name} for stream {streamId}", ex);
            }

            if (x != 1)
            {
                throw new InvalidOperationException(
                    $"Failed to insert event of type {domainEvent.GetType().Name} for stream {streamId}");
            }
        }

        /// <summary>
        ///     Retrieves the current version of a stream from the event streams table.
        /// </summary>
        /// <param name="c">An open SqliteConnection to be used for the query.</param>
        /// <param name="t">The transaction associated with the query execution.</param>
        /// <param name="streamId">The unique identifier of the stream whose version is to be retrieved.</param>
        /// <param name="ct">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     The current version of the stream as an integer, or null if the stream does not exist.
        /// </returns>
        async Task<int?> GetStreamVersion(SqliteConnection c,
            DbTransaction t,
            string streamId,
            CancellationToken ct)
        {
            var versionCmd = new DapperCommand("SELECT version from event_streams WHERE id=@StreamId;",
                new { StreamId = streamId });
            int? currentVersion = await versionCmd.ExecuteScalarAsync<int?>(c, t, ct)
                .ConfigureAwait(false);

            return currentVersion;
        }
    }
}
