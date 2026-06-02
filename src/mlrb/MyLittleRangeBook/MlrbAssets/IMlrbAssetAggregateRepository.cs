using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    public interface IMlrbAssetAggregateRepository
    {
        /// <summary>
        /// Will try and retrieve the MrlbAsset aggregate using the Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The aggregate if it can be found, NULL if it cannot (implying that this is a new asset).</returns>
        Task<Result<MlrbAssetAggregate?>> GetAsync(MlrbId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Will try and retrieve the MrlbAsset aggregate using the fileInfo.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The aggregate if it can be found, NULL if it cannot (implying that this is a new asset).</returns>
        Task<Result<MlrbAssetAggregate?>> GetAsync(FileInfo fileInfo, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist the aggregate stream.
        /// </summary>
        /// <param name="aggregate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> SaveAsync(MlrbAssetAggregate aggregate, CancellationToken cancellationToken = default);
    }
}
