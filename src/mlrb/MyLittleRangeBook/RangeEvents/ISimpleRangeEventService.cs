using System.Data;

namespace MyLittleRangeBook.RangeEvents
{
    public interface ISimpleRangeEventService
    {
        // TODO [TO20260528] These methods need to accept an IDbTransaction.
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
