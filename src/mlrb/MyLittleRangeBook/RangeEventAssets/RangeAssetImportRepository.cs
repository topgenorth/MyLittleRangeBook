using System.Data.Common;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.RangeEventAssets
{
    public class SqliteRangeAssetAggregateRepository : IRangeAssetAggregateRepository
    {
        readonly IRangeAssetProjector _rangeAssetProjector;

        public SqliteRangeAssetAggregateRepository(ISqliteHelper sqliteHelper,
            IEventSerializer eventSerializer,
            IRangeAssetProjector rangeAssetProjector)
            : base(sqliteHelper,
                eventSerializer,
                RangeAssetAggregate.DEFAULT_STREAM_TYPE_NAME,
                RangeAssetAggregate.Create)
        {
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

        public async Task<Result<RangeAssetAggregate>> GetAsync(FileInfo fileInfo,
            CancellationToken cancellationToken = default)
        {
            if (!fileInfo.Exists)
            {
                var err = new Error($"File not found: {fileInfo.FullName}").CausedBy(new FileNotFoundException());
                return Result.Fail(err);
            }

            var id = MlrbId.FromFile(fileInfo);
            return await GetAsync(id, cancellationToken).ConfigureAwait(false);
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

                    await InsertDomainEventAsync(connection, transaction, streamId, nextVersion, evt,
                        cancellationToken);
                    nextVersion++;
                }

                await UpsertEventStreamAsync(connection,
                    transaction,
                    aggregate.Id.ToString(),
                    currentVersion,
                    nextVersion - 1,
                    cancellationToken);


                var ctx = new RangeAssetProjectionContext(
                    connection,
                    transaction,
                    aggregate.Id,
                    pendingEvents,
                    cancellationToken
                );
                await _rangeAssetProjector.ProjectAsync(ctx).ConfigureAwait(false);

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
            IReadOnlyList<IDomainEvent> pendingEvents,
            CancellationToken cancellationToken)
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
                p = new { Version = nextVersion, StreamId = streamId, ExpectedVersion = currentVersion };
            }

            var cmd = new DapperCommand(sql, p);
            int i = await cmd.ExecuteAsync(conn, trans, ct).ConfigureAwait(false);
            if (i != 1)
            {
                throw new InvalidOperationException($"Failed to update event stream version for {streamId}");
            }
        }

        public async Task<Result<RangeAssetAggregate?>> GetAsync(FileInfo fileInfo, CancellationToken cancellationToken = default)
        {
            if (!fileInfo.Exists)
            {
                return Result.Fail("File does not exist.");
            }

            var streamId = MlrbId.FromFile(fileInfo);
            return await GetAsync(streamId, cancellationToken).ConfigureAwait(false);
        }
    }
}
