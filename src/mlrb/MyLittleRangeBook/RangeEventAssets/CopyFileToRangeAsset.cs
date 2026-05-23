namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     This delegate is used to create the name of a file based asset that will be copied over to the range event
    ///     directory.
    /// </summary>
    /// <param name="rangeAssetsDirectory">This is the name of directory that will hold range assets files.</param>
    /// <param name="rangeEventAssetFile">The <c cref="RangeEventAssetFile" /> that will be processed. </param>
    public delegate string AssetFileNameResolver(string rangeAssetsDirectory,
        RangeEventAssetFile rangeEventAssetFile);
}
