using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    public class SimpleRangeEventRepositoryFirearmStream : ISimpleRangeEventRepository
    {
        readonly SqliteSimpleRangeEventRepository _wrapped;
        readonly IFirearmAggregateRepository      _faRepo;
        public SimpleRangeEventRepositoryFirearmStream(SqliteSimpleRangeEventRepository wrapped, IFirearmAggregateRepository faRepo)
        {
            _wrapped = wrapped;
            _faRepo  = faRepo;
        }

        public Task<Result> DeleteAsync(SimpleRangeEvent  simpleRangeEvent,
                                        CancellationToken cancellationToken = default) => _wrapped.DeleteAsync(simpleRangeEvent, cancellationToken);

        public async Task<Result<long>> UpsertAsync(SimpleRangeEvent  simpleRangeEvent,
                                                     CancellationToken cancellationToken = default)
        {
            var r1 = await _wrapped.UpsertAsync(simpleRangeEvent, cancellationToken);
            if (r1.IsFailed)
                return r1;

            throw new NotImplementedException();
        }

        public async Task<Result<long>> UpsertAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent)
        {
            var r1 = await _wrapped.UpsertAsync(context, simpleRangeEvent);
            if (r1.IsFailed)
            {
                return Result.Fail(r1.Errors[0]);
            }

            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default) => _wrapped.GetSimpleRangeEventsAsync(cancellationToken);

        public Task<Result<SimpleRangeEvent>> GetAsync(string id, CancellationToken cancellationToken = default) =>
            _wrapped.GetAsync(id, cancellationToken);
    }
}