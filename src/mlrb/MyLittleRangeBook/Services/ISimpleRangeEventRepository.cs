using System.Data;
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

        Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(CancellationToken cancellationToken = default);
    }
}
