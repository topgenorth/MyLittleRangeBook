using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    public interface IMlrbAssetAggregateRepository
    {
        Task<Result<MlrbAssetAggregate?>> GetAsync(MlrbId id, CancellationToken cancellationToken = default);
        Task<Result<MlrbAssetAggregate?>> GetAsync(FileInfo fileInfo, CancellationToken cancellationToken = default);
        Task<Result> SaveAsync(MlrbAssetAggregate aggregate, CancellationToken cancellationToken = default);
    }
}
