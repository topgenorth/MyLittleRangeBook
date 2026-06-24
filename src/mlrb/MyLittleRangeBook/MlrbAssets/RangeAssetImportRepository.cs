using System.Data.Common;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.MlrbAssets.MlrbAssetAggregate;

namespace MyLittleRangeBook.MlrbAssets
{
    public class MlrbAssetAggregateSqliteRepository
        : SqliteAggregateRepository<MlrbAssetAggregate>, IMlrbAssetAggregateRepository
    {
        readonly IRangeAssetProjector _rangeAssetProjector;

        public MlrbAssetAggregateSqliteRepository(ISqliteHelper        sqliteHelper,
                                                  IEventSerializer     eventSerializer,
                                                  IRangeAssetProjector rangeAssetProjector)
            : base(sqliteHelper,
                   eventSerializer,
                   DEFAULT_STREAM_TYPE_NAME,
                   Create) =>
            _rangeAssetProjector = rangeAssetProjector;

        public override async Task<Result<MlrbAssetAggregate?>> GetAsync(DapperCommandContext context, MlrbId id) =>
            await base.GetAsync(context, id).ConfigureAwait(false);

        public async Task<Result<MlrbAssetAggregate?>> GetAsync(DapperCommandContext context, FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return Result.Fail("File does not exist.");
            }

            MlrbId streamId = MlrbId.FromFile(fileInfo);

            Result<MlrbAssetAggregate?> r1 = await GetAsync(context, streamId).ConfigureAwait(false);
            return r1;
        }

        public async Task<Result> SaveAsync(DapperCommandContext context, MlrbAssetAggregate aggregate) =>
            await UpsertAsync(context, aggregate).ConfigureAwait(false);

        public Task<Result> SaveAsync(MlrbAssetAggregate aggregate, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        protected Task ProjectAsync(SqliteConnection            connection,
                                    DbTransaction               transaction,
                                    string                      streamId,
                                    IReadOnlyList<IDomainEvent> pendingEvents,
                                    CancellationToken           cancellationToken)
        {
            RangeAssetProjectorContext ctx = new(connection, transaction, streamId, pendingEvents,
                                                 cancellationToken);

            return _rangeAssetProjector.ProjectAsync(ctx);
        }
    }
}