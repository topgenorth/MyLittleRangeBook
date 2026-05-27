using System.Data.Common;
using Microsoft.Data.Sqlite;
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
            _logger.Warning("Projecting {EventCount} events for RangeAssetImport", pendingEvents.Count);

            return Task.CompletedTask;
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
                RangeAssetAggregate.DEFAULT_STREAM_TYPE_NAME,
                RangeAssetAggregate.Create)
        {
            _rangeAssetProjector = rangeAssetProjector;
        }

        protected override Task ProjectAsync(SqliteConnection connection,
            DbTransaction transaction,
            string streamId,
            IReadOnlyList<IDomainEvent> pendingEvents,
            CancellationToken cancellationToken)
        {
            return _rangeAssetProjector.ProjectAsync(streamId,
                pendingEvents,
                connection,
                transaction,
                cancellationToken);
        }
    }
}
