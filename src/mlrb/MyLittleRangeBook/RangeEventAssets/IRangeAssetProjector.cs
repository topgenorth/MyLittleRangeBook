namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     Defines functionality for projecting domain events related to file imports into a storage system.
    /// </summary>
    public interface IRangeAssetProjector
    {
        Task ProjectAsync(RangeAssetProjectorContext context);
    }
}
