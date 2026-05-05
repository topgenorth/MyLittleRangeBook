using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using static MyLittleRangeBook.Database.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.Database.Sqlite
{
    public class SqliteSimpleRangeEventRepository : ISimpleRangeEventRepository
    {
        readonly ISimpleRangeLogService _simpleRangeLogService;
        readonly ISqliteHelper _sqliteHelper;

        public SqliteSimpleRangeEventRepository(ISqliteHelper sqliteHelper,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeLogService simpleRangeLogService)
        {
            _sqliteHelper = sqliteHelper;
            _simpleRangeLogService = simpleRangeLogService;
        }

        /// <summary>
        ///     Will add or update a simple range event. If necessary, then a new Firearm record will be added.
        /// </summary>
        /// <param name="simpleRangeEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            FileInfo fitFileInfo,
            CancellationToken cancellationToken = default)
        {
            if (!fitFileInfo.Exists)
            {
                return await UpsertAsync(simpleRangeEvent, cancellationToken).ConfigureAwait(false);
            }

            Result<ReadOnlyMemory<byte>> loadResult =
                await fitFileInfo.LoadFileBytesAsync(cancellationToken).ConfigureAwait(false);
            if (loadResult.IsSuccess)
            {
                return await UpsertAsync(simpleRangeEvent, loadResult.Value.ToArray(), cancellationToken)
                    .ConfigureAwait(false);
            }

            // TODO [TO20260504] Need to include somethign in the result that says the FIT file didn't get saved because it couldn't be loaded
            Result<long?> r = await UpsertAsync(simpleRangeEvent, cancellationToken).ConfigureAwait(false);

            return r;
        }

        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            byte[] fitFileContents,
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
            Result<long?> finalResult;
            try
            {
                Result<long?> sreResult = await _simpleRangeLogService
                    .UpsertAsync(conn, simpleRangeEvent, cancellationToken)
                    .ConfigureAwait(false);


                // [TO20260504] Not sure how important the file name really is.
                string syntheticFileName = simpleRangeEvent.Id + "_" +
                                           simpleRangeEvent.EventDate.ToString("yyyyMMdd") + ".fit";
                Result<(string id, long rowId)> fitResult = await _sqliteHelper
                    .WriteFileToTableAsync(conn, SqliteFileTable.FitFiles, syntheticFileName, fitFileContents,
                        cancellationToken)
                    .ConfigureAwait(false);

                Result<long?> firearmResult =
                    await HandleFirearmUpsertAsync(conn, simpleRangeEvent, cancellationToken).ConfigureAwait(false);


                finalResult = Result.Merge(sreResult, fitResult, firearmResult).ToResult(simpleRangeEvent.RowId);
            }
            catch (Exception e)
            {
                finalResult = Result.Fail<long?>($"Failed to upsert simple range event: {e.Message}");
            }

            return finalResult;
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

        async Task<Result<(string id, long rowId)>> HandleFitFileUpsertAsync(FileInfo fitFileInfo,
            CancellationToken cancellationToken,
            SqliteConnection conn)
        {
            byte[] contents = await File.ReadAllBytesAsync(fitFileInfo.FullName, cancellationToken);

            return await _sqliteHelper
                .WriteFileToTableAsync(conn, SqliteFileTable.FitFiles, fitFileInfo.FullName, contents,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Add a firearm using the firearmName from the SimpleRangeEvent. If the firearm exists, then just update it's
        ///     modified field.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="simpleRangeEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<Result<long?>> HandleFirearmUpsertAsync(SqliteConnection conn,
            SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken)
        {
            // [TO20260421] Need to create a new one.
            Result<long?> finalResult;
            Error? couldntSaveFirearmError = new Error("Could not save Firearm")
                .WithMetadata("SimpleRangeEventId", simpleRangeEvent.Id)
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
                if (x2 is null)
                {
                    finalResult = Result.Fail<long?>(couldntSaveFirearmError);
                }
                else
                {
                    var rowId = Convert.ToInt64(x2);
                    finalResult = new Result<long?>().WithValue(rowId);
                }
            }
            catch (Exception e)
            {
                finalResult = Result.Fail<long?>(couldntSaveFirearmError.CausedBy(e));
            }

            return finalResult;
        }
    }
}
