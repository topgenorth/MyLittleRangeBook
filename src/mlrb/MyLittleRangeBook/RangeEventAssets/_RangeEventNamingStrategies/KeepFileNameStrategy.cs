namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     Keeps the original filename of the asset, but uses the path to the asset directory for the range event.
    /// </summary>
    public class KeepFileNameStrategy : FileNameStrategyBase
    {
        protected override string MakeAssetFileNameForRangeEvent(string rangeEventTargetDir, string assetFileName)
        {
            string targetFileName = Path.GetFileName(assetFileName);
            string assetPath = Path.Combine(rangeEventTargetDir, targetFileName);

            return assetPath;
        }
    }
}
