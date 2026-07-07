using FluentResults;
using MyLittleRangeBook.FIT.Model;

namespace MyLittleRangeBook.FIT
{
    public interface IXeroCsvShotSessionParser
    {
        /// <summary>
        ///     Parses a CSV file exported from a Garmin Xero C1 into a ShotSession.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<ShotSession>> ParseCsvFileAsync(string filePath, CancellationToken cancellationToken);

        /// <summary>
        ///     Checks if the file is a ShotView CSV file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> IsShotViewCsvAsync(string filePath, CancellationToken cancellationToken);
    }
}
