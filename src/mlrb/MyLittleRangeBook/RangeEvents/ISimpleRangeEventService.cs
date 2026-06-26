using System.Data;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    public interface ISimpleRangeEventService
    {
        Task<Result> DeleteAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent);

        Task<Result<SimpleRangeEvent>> GetAsync(DapperCommandContext context, MlrbId simpleRangeEventId);

        /// <summary>
        /// Insert or update a record in the simple_range_event table.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="simpleRangeEvent"></param>
        /// <returns></returns>
        Task<Result<MlrbId>> UpsertAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent);

        Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(DapperCommandContext context);
    }
}
