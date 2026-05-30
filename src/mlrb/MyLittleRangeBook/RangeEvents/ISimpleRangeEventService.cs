using System.Data;

namespace MyLittleRangeBook.RangeEvents
{
    public interface ISimpleRangeEventService
    {
        Task<Result> DeleteAsync(IDbConnection connection,
            SimpleRangeEvent simpleRangeEvent,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);

        Task<Result<long?>> UpsertAsync(IDbConnection connection,
            SimpleRangeEvent simpleRangeEvent,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);

        Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);
    }
}
