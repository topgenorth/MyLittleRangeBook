using ByteAether.Ulid;
using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Services
{
    /// <summary>
    ///     Will generate a unique file name for the asset in the asset directory for the application.
    /// </summary>
    public class UniqueAssetNameStrategy : IRangeEventAssetNamingStrategy
    {
        string? _rangeAssetsDirectory;

        public Result<(string assetId, string assetPath)> GenerateAssetFileName(string rangeEventId,
            string assetFileName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(_rangeAssetsDirectory);
            ArgumentException.ThrowIfNullOrWhiteSpace(assetFileName);
            ArgumentException.ThrowIfNullOrWhiteSpace(rangeEventId);

            string rangeEventAssetDirectory;
            string extension = Path.GetExtension(assetFileName);

            try
            {
                rangeEventAssetDirectory = Path.Combine(_rangeAssetsDirectory, rangeEventId);
                Directory.CreateDirectory(rangeEventAssetDirectory);
            }
            catch (Exception e)
            {
                Error? error = new Error("Failed to create asset directory for range event").CausedBy(e);

                return Result.Fail(error);
            }

            MlrbId assetId = new MlrbId();
            var newFileName = $"{assetId}{extension}";
            string assetPath = Path.Combine(rangeEventAssetDirectory, newFileName);

            return Result.Ok((assetId.ToString(), assetPath));
        }

        public IRangeEventAssetNamingStrategy In(string rangeAssetsDirectory)
        {
            _rangeAssetsDirectory = rangeAssetsDirectory;

            return this;
        }
    }
}
