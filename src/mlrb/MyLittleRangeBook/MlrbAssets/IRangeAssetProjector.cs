using MyLittleRangeBook.EventSourcing;

namespace MyLittleRangeBook.MlrbAssets
{
    /// <summary>
    ///     Defines functionality for projecting domain events related to file imports into a storage system.
    /// </summary>
    public interface IRangeAssetProjector
    {
        // TODO [TO20260531] Make this IProjector.
        Task ProjectAsync(RangeAssetProjectorContext context);
    }
}
