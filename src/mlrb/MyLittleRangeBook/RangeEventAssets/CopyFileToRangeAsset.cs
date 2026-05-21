using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Config;

namespace MyLittleRangeBook.RangeEventAssets
{
    public class CopyFileToRangeAsset
    {
        /// <summary>
        ///     This delegate is used to create the name of a file based asset that will be copied over to the range event
        ///     directory.
        /// </summary>
        /// <param name="rangeAssetsDirectory">This is the name of directory that will hold range assets files.</param>
        /// <param name="rangeEventAssetFile">The <c cref="RangeEventAssetFile" /> that will be processed. </param>
        public delegate string AssetFileNameResolver(string rangeAssetsDirectory,
            RangeEventAssetFile rangeEventAssetFile);

        /// <summary>
        ///     Use an 80K buffer for copying files.
        /// </summary>
        const int BufferSize = 81920;

        AssetFileNameResolver _assetNamer;
        string _rangeAssetsDirectory;

        public CopyFileToRangeAsset(string rangeAssetsDirectory)
        {
            _assetNamer = GetRangeEventAssetFilename;
            _rangeAssetsDirectory = rangeAssetsDirectory;
        }

        public CopyFileToRangeAsset(IConfiguration config)
        {
            _assetNamer = GetRangeEventAssetFilename;
            _rangeAssetsDirectory = config.GetRangeAssetDirectory();
        }

        public static string GetRangeEventAssetFilename(string rangeAssetDirectory,
            RangeEventAssetFile rangeEventAssetFile)
        {
            string rangeEventAssetDir = Path.Combine(rangeAssetDirectory, rangeEventAssetFile.RangeEventId);
            Directory.CreateDirectory(rangeEventAssetDir);

            string filename = Path.GetFileName(rangeEventAssetFile.PathToAsset);

            string rangeEventAssetFilename = Path.Combine(rangeEventAssetDir, filename);

            return rangeEventAssetFilename;
        }

        public CopyFileToRangeAsset NameRangeEventAssetFile(AssetFileNameResolver assetNamer)
        {
            ArgumentNullException.ThrowIfNull(assetNamer);
            _assetNamer = assetNamer;

            return this;
        }

        /// <summary>
        ///     Copy a the file on disk to the Range Asset directory for the Range Event.
        /// </summary>
        /// <param name="fileAsset"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Result> CopyFileAsync(RangeEventAssetFile fileAsset, CancellationToken ct = default)
        {
            Result result;
            try
            {
                await using var source = new FileStream(
                    fileAsset.PathToAsset,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    BufferSize,
                    FileOptions.Asynchronous);

                string rangeEventAssetFilename = _assetNamer(_rangeAssetsDirectory, fileAsset);
                await using var destination = new FileStream(
                    rangeEventAssetFilename,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    BufferSize,
                    FileOptions.Asynchronous);

                await source.CopyToAsync(destination, ct);

                result = Result.Ok();
            }
            catch (Exception e)
            {
                Error? err = new Error(e.Message).CausedBy(e);

                result = Result.Fail(err);
            }

            return result;
        }
    }
}
