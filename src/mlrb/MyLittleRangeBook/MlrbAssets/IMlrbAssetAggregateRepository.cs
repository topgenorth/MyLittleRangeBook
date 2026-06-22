using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.MlrbAssets
{
    public interface IMlrbAssetAggregateRepository
    {
        /// <summary>
        /// Will try and retrieve the MrlbAsset aggregate using the Id.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <returns>The aggregate if it can be found, NULL if it cannot (implying that this is a new asset).</returns>
        Task<Result<MlrbAssetAggregate?>> GetAsync(DapperCommandContext context, MlrbId id);

        /// <summary>
        /// Will try and retrieve the MrlbAsset aggregate using the fileInfo.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileInfo"></param>
        /// <returns>The aggregate if it can be found, NULL if it cannot (implying that this is a new asset).</returns>
        Task<Result<MlrbAssetAggregate?>> GetAsync(DapperCommandContext context, FileInfo fileInfo);

        /// <summary>
        /// Persist the aggregate stream.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        Task<Result> SaveAsync(DapperCommandContext context, MlrbAssetAggregate aggregate);
    }
}
