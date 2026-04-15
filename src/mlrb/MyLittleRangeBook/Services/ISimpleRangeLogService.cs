using System.Data;
using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Services
{
    public interface ISimpleRangeLogService
    {
        Task<Result<bool>> DeleteAsync(IDbConnection connection,
            SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default);

        Task<Result<long?>> UpsertAsync(IDbConnection connection,
            SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default);

        Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(IDbConnection connection,
            CancellationToken cancellationToken = default);
    }
}
