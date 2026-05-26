using System.Data.Common;
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
            await using SqliteConnection connection =
                await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using DbTransaction transaction =
                await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
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

            var aggregate = new RangeAssetAggregate();
            foreach (EventRow row in eventRows)
            {
                object evt = _eventSerializer.Deserialize(row.EventType, row.DataJson);
                aggregate.Apply((IDomainEvent)evt);
            }

            aggregate.ClearUncommittedEvents();
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

            return Result.Ok(aggregate);
        }

        public async Task<Result> SaveAsync(RangeAssetAggregate aggregate,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<IDomainEvent> pendingEvents = aggregate.DequeueUncommittedEvents();
            if (pendingEvents.Count == 0)
            {
                return Result.Ok();
            }

            // [TO20260525] It is non-sensical to have a negative expected version, but we can treat it as 0 to allow saving new aggregates without events.
            int expectedVersion = Math.Max(aggregate.Version - pendingEvents.Count, 0);
            var streamId = aggregate.Id.ToString();

            await using SqliteConnection connection =
                await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using DbTransaction transaction =
                await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                (int? currentVersion, int nextVersion) = await GetNextVersionAsync(connection, transaction, streamId,
                    expectedVersion, cancellationToken);


                foreach (IDomainEvent evt in pendingEvents)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                        return Result.Fail("Operation was cancelled.");
                    }

                    nextVersion++;
                    await InsertDomainEvent(connection, transaction, streamId, nextVersion, evt, cancellationToken);
                }

                // [TO20260525] Order matters here - nextVersion has to be updated.
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
                sql =
                    "insert into event_streams (id, stream_type, version) values (@StreamId, @StreamType, @Version);";
                p = new { StreamId = streamId, StreamType, Version = nextVersion };
            }
            else
            {
                sql =
                    "update event_streams set version = @Version where id = @StreamId   and version = @ExpectedVersion;";
                p = new { Version = nextVersion, StreamId = streamId, ExpectedVersion = currentVersion };
            }

            var cmd = new DapperCommand(sql, p);
            int i = await cmd.ExecuteAsync(conn, trans, ct).ConfigureAwait(false);
            if (i != 1)
            {
                throw new InvalidOperationException($"Failed to update event stream version for {streamId}");
            }
        }

        async Task InsertDomainEvent(SqliteConnection connection,
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
            var p = new
            {
                StreamId = streamId,
                Id = new MlrbId(domainEvent.OccurredUtc).ToString(),
                StreamType,
                Version = nextVersion,
                EventType = domainEvent.GetType().Name,
                domainEvent.OccurredUtc,
                DataJson = _eventSerializer.Serialize(domainEvent),
                MetadataJson = "{}"
            };
            var cmd = new DapperCommand(SQL, p);
            int x = 0;
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

        async Task<(int? currentVersion, int nextVersion)> GetNextVersionAsync(SqliteConnection connection,
            DbTransaction transaction,
            string streamId,
            int expectedVersion,
            CancellationToken cancellationToken)
        {
            var versionCmd = new DapperCommand("SELECT version from event_streams WHERE id=@StreamId;",
                new { StreamId = streamId });
            int? currentVersion = await versionCmd.ExecuteScalarAsync<int?>(connection, transaction, cancellationToken)
                .ConfigureAwait(false);
            int actualVersion = currentVersion ?? 0;

            if (actualVersion == expectedVersion)
            {
                return (currentVersion, expectedVersion);
            }

            throw new InvalidOperationException(
                $"Concurrency conflict: expected version {expectedVersion} but actual version is {actualVersion} for stream `{streamId}`.");
        }
    }
}
