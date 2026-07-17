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

        Task<Result> DisassociateFromFirearm(DapperCommandContext context, MlrbId firearmId,
                                             MlrbId               rangeEventId);

        /// <summary>
        /// Exports range event records to a CSV file.
        /// </summary>
        /// <param name="context">The command context containing connection and transaction details.</param>
        /// <param name="csvFileName">The name of the CSV file to which the range event data will be exported.</param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        Task<Result> ExportToCsv(DapperCommandContext context, string csvFileName);
    }
}
