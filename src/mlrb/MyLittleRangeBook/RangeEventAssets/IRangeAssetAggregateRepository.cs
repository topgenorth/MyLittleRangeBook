using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    public interface IRangeAssetAggregateRepository
    {
        Task<Result<RangeAssetAggregate?>> GetAsync(MlrbId id, CancellationToken cancellationToken = default);
        Task<Result<RangeAssetAggregate>> GetAsync(FileInfo fileInfo, CancellationToken cancellationToken = default);
        Task<Result> SaveAsync(RangeAssetAggregate aggregate, CancellationToken cancellationToken = default);
    }
}
