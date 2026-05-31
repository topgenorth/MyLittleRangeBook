using System.Data.Common;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.RangeEventAssets.RangeAssetAggregate;

namespace MyLittleRangeBook.RangeEventAssets
{
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
            var ctx = new RangeAssetProjectorContext(connection, transaction, streamId, pendingEvents,
                cancellationToken);

            return _rangeAssetProjector.ProjectAsync(ctx);
        }
    }
}
