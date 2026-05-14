using FluentResults;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MyLittleRangeBook.Services
{
    public interface IRangeEventAssetImporter
    {
        /// <summary>
        ///     Will copy the asset to the data directory for the app, and associate it with the specified range event.
        /// </summary>
        /// <param name="assetToImport"></param>
        /// <param name="rangeEventId"></param>
        /// <param name="ct"></param>
        /// <returns>A tuple that holds the ID of the new asset, and the path it was copied to.</returns>
        Task<Result<(string assetId, string destinationPath)>> ImportAssetForRangeEvent(string assetToImport,
            string rangeEventId, CancellationToken ct = default);
    }
}
