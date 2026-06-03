using System.Data.Common;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.MlrbAssets.MlrbAssetAggregate;

namespace MyLittleRangeBook.MlrbAssets
{
    public class MlrbAssetAggregateSqliteRepository
        : SqliteAggregateRepository<MlrbAssetAggregate>, IMlrbAssetAggregateRepository
    {
        readonly IRangeAssetProjector _rangeAssetProjector;

        public MlrbAssetAggregateSqliteRepository(ISqliteHelper sqliteHelper,
            IEventSerializer eventSerializer,
            IRangeAssetProjector rangeAssetProjector)
            : base(sqliteHelper,
                eventSerializer,
                DEFAULT_STREAM_TYPE_NAME,
                Create)
        {
            _rangeAssetProjector = rangeAssetProjector;
        }

        public async Task<Result<MlrbAssetAggregate?>> GetAsync(FileInfo fileInfo,
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
