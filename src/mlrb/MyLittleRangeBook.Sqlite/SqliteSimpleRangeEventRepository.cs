using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using static MyLittleRangeBook.Database.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.Database.Sqlite
{
    public class SqliteSimpleRangeEventRepository : ISimpleRangeEventRepository
    {
        readonly ILogger _logger;
        readonly ISimpleRangeLogService _simpleRangeLogService;
        readonly ISqliteHelper _sqliteHelper;

        public SqliteSimpleRangeEventRepository(ISqliteHelper sqliteHelper,
            [FromKeyedServices(SQLITE_KEY)] ISimpleRangeLogService simpleRangeLogService,
            ILogger logger)
        {
            _sqliteHelper = sqliteHelper;
            _simpleRangeLogService = simpleRangeLogService;
            _logger = logger;
        }

        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
            Result<long?> result = await _simpleRangeLogService.UpsertAsync(conn, simpleRangeEvent, cancellationToken);
            if (result.IsSuccess)
            {
                _logger.Verbose("SimpleRangeEvent {Id} saved RowId: {RowId}", simpleRangeEvent.Id,
                    result.Value);
            }
            else
            {
                _logger.Warning("SimpleRangeEvent {Id} could not be saved. RowId {RowId}", simpleRangeEvent.Id,
                    result.Value ?? -1);
                Error? reason  = new Error("SimpleRangeEvent could not be saved")
                    .WithMetadata("DataSource", conn.DataSource);
                result = result.WithError(reason);
            }

            return result;
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

            return await _simpleRangeLogService.GetSimpleRangeEventsAsync(conn, cancellationToken);
        }

        public async Task<Result<bool>> DeleteAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

            return await _simpleRangeLogService.DeleteAsync(conn, simpleRangeEvent, cancellationToken);
        }
    }
}
