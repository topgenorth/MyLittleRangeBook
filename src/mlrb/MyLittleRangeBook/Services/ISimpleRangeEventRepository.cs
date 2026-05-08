using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Services
{
    public interface ISimpleRangeEventRepository
    {
        Task<Result<bool>> DeleteAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default);

        Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default);


        /// <summary>
        ///     Will create a new Range Event and associate the FIT file with it.
        /// </summary>
        /// <param name="simpleRangeEvent"></param>
        /// <param name="fitFileContents"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            byte[] fitFileContents,
            CancellationToken cancellationToken = default);

        Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            string shotViewCsvContents,
            string fileName,
            CancellationToken cancellationToken = default);

        Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(CancellationToken cancellationToken =
            default);
    }
}
