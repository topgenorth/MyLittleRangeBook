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

        /// <summary>
        /// Will add or update a simple range event. If necessary, then a new Firearm record will be added.
        /// </summary>
        /// <param name="simpleRangeEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
            Result<long?> result = await _simpleRangeLogService.UpsertAsync(conn, simpleRangeEvent, cancellationToken);
            if (result.IsSuccess)
            {
                _logger.Verbose("SimpleRangeEvent {Id} saved RowId: {RowId}", simpleRangeEvent.Id,
                    result.Value);

                if (result is not { IsSuccess: true, Value: > 0 })
                {
                    return result;
                }

                Result<long> firearmRowIdResult = await UpsertFirearmAsync(conn, simpleRangeEvent, cancellationToken);
                // [TO20260421] For now, this isn't a big deal.
                _logger.Verbose(
                    firearmRowIdResult.IsFailed
                        ? "Firearm {FirearmName} could not be saved for SimpleRangeEvent {Id}"
                        : "Firearm {FirearmName} saved for SimpleRangeEvent {Id}",
                    simpleRangeEvent.FirearmName, simpleRangeEvent.Id);
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

        async Task<Result<long>> UpsertFirearmAsync(SqliteConnection conn,
            SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken)
        {
            // [TO20260421] Need to create a new one.
            Result<long> result;
            Error? couldntSaveFirearmError = new Error("Could not save Firearm")
                .WithMetadata("FirearmName", simpleRangeEvent.FirearmName);

            try
            {
                await using SqliteCommand insertCmd = conn.CreateCommand();
                insertCmd.CommandText =
                    """
                    INSERT INTO Firearms (Id, Name, Created, Modified) 
                    VALUES (nanoid(), @firearm_name, utcnow(), utcnow())
                    ON CONFLICT (Name) DO UPDATE SET Modified = utcnow()
                    RETURNING RowId;
                    """;
                insertCmd.Parameters.AddWithValue("@firearm_name", simpleRangeEvent.FirearmName);

                object? x2 = await insertCmd.ExecuteScalarAsync(cancellationToken);
                if (long.TryParse(x2?.ToString() ?? "", out long rowId2))
                {
                    result = new Result<long>().WithValue(rowId2);
                }
                else
                {
                    _logger.Warning("Could not save Firearm {FirearmName}", simpleRangeEvent.FirearmName);
                    result = Result.Fail<long>(couldntSaveFirearmError);
                }

            }
            catch (Exception e)
            {
                _logger.Warning(e, "Could not save Firearm {FirearmName}", simpleRangeEvent.FirearmName);
                result = Result.Fail<long>(couldntSaveFirearmError.CausedBy(e));
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
