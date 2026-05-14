namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     Will generate a unique file name for the asset in the asset directory for the application.
    /// </summary>
    public class UniqueAssetNameStrategy : FileNameStrategyBase
    {
        protected override string MakeAssetFileNameForRangeEvent(string rangeEventTargetDir, string assetFileName)
        {
            string extension = Path.GetExtension(assetFileName);
            var targetFile = $"{AssetId}{extension}";

            return Path.Combine(rangeEventTargetDir, targetFile);
        }
    }
}
