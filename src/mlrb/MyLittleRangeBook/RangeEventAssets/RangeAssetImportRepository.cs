using System.Data.Common;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.RangeEventAssets.RangeAssetAggregate;

namespace MyLittleRangeBook.RangeEventAssets
{
    class SqliteRangeAssetProjector : IRangeAssetProjector
    {
        readonly ILogger _logger;

        public SqliteRangeAssetProjector(ILogger logger)
        {
            _logger = logger;
        }

        public async Task ProjectAsync(RangeAssetProjectorContext context)
        {
            _logger.Verbose("Projecting {EventCount} events for RangeAssetImport", context.PendingEvents.Count);

            Result r = await AssociateRangeAssetToRangeEvent(context);
        }

        async Task<Result> AssociateRangeAssetToRangeEvent(RangeAssetProjectorContext context)
        {
            MlrbId rangeEventId;
            try
            {
                (_, rangeEventId, _) = (RangeAssetAssociateWithRangeEvent)context.PendingEvents.First(domainEvent =>
                    domainEvent is RangeAssetAssociateWithRangeEvent);
            }
            catch (Exception e)
            {
                _logger.Verbose(e, "Could not find a RangeAssetAssociatedWithRangeEvent: {errorMessage}.", e.Message);
                Error err = new Error(e.Message).CausedBy(e).Enrich(context.RangeAssetId);

                return Result.Fail(err);
            }

            _logger.Verbose("Associating RangeAsset {RangeAssetId} to RangeEvent", context.RangeAssetId);

            var p = new { RangeEventId = rangeEventId, context.RangeAssetId };
            var cmd = new DapperCommand(
                "INSERT INTO SimpleRangeEvent_RangeAssets (SimpleRangeEventId, RangeAssetFilesId) VALUES (@RangeEventId, @RangeAssetId)",
                p);

            int r = await cmd.ExecuteAsync(context.Connection, context.Transaction, context.CancellationToken)
                .ConfigureAwait(false);
            if (r != 1)
            {
                return Result.Fail("Could not associate range asset to the range event.");
            }

            return Result.Ok();
        }
    }

    public class SqliteRangeAssetAggregateRepository
        : SqliteAggregateRepository<RangeAssetAggregate>, IRangeAssetAggregateRepository
    {
        readonly IRangeAssetProjector _rangeAssetProjector;

        public SqliteRangeAssetAggregateRepository(ISqliteHelper sqliteHelper,
            IEventSerializer eventSerializer,
            IRangeAssetProjector rangeAssetProjector)
            : base(sqliteHelper,
                eventSerializer,
                DEFAULT_STREAM_TYPE_NAME,
                Create)
        {
            _rangeAssetProjector = rangeAssetProjector;
        }

        public async Task<Result<RangeAssetAggregate?>> GetAsync(FileInfo fileInfo,
            CancellationToken cancellationToken = default)
        {
            if (!fileInfo.Exists)
            {
                return Result.Fail("File does not exist.");
            }

            var streamId = MlrbId.FromFile(fileInfo);

            return await GetAsync(streamId, cancellationToken).ConfigureAwait(false);
        }

        protected override Task ProjectAsync(SqliteConnection connection,
            DbTransaction transaction,
            string streamId,
            IReadOnlyList<IDomainEvent> pendingEvents,
            CancellationToken cancellationToken)
        {
            var ctx = new RangeAssetProjectorContext(connection, transaction, streamId, pendingEvents, cancellationToken);

            return _rangeAssetProjector.ProjectAsync(ctx);
        }
    }
}
