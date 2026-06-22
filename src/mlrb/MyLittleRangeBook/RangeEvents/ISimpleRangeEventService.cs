using System.Data;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    public interface ISimpleRangeEventService
    {
        Task<Result> DeleteAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent);

        Task<Result<long?>> UpsertAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent);

        Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(DapperCommandContext context);
    }
}
