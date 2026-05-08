using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using NanoidDotNet;
using static MyLittleRangeBook.Database.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.Database.Sqlite
{
    public class SqliteSimpleRangeEventRepository : ISimpleRangeEventRepository
    {
        // TODO [TO20260505] Introduce SQLite transactions.
        readonly IFitFilesDbService _filesDbService;
        readonly IShotViewFilesDbService _shotViewFilesDbService;
        readonly ISimpleRangeLogService _simpleRangeEventService;
        readonly ISqliteHelper _sqliteHelper;

        public SqliteSimpleRangeEventRepository(ISqliteHelper sqliteHelper,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeLogService simpleRangeEventService,
            [FromKeyedServices(DI_KEYS_SQLITE)] IFitFilesDbService filesDbService,
            [FromKeyedServices(DI_KEYS_SQLITE)] IShotViewFilesDbService shotViewFilesDbService)
        {
            _sqliteHelper = sqliteHelper;
            _simpleRangeEventService = simpleRangeEventService;
            _filesDbService = filesDbService;
            _shotViewFilesDbService = shotViewFilesDbService;
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
            return await UpsertAsync(simpleRangeEvent, Array.Empty<byte>(), cancellationToken)
                .ConfigureAwait(false);
        }


        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            byte[] fitFileContents,
            CancellationToken cancellationToken = default)
        {
            return await UpsertAsync(simpleRangeEvent, fitFileContents, string.Empty, string.Empty, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            byte[] fitFileContents,
            string shotViewCsvContents,
            string shotViewFileName,
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
            Result<long?> finalResult;
            try
            {
                Result<long?> sreResult = await _simpleRangeEventService
                    .UpsertAsync(conn, simpleRangeEvent, cancellationToken)
                    .ConfigureAwait(false);

                if (sreResult.IsFailed)
                {
                    return sreResult;
                }

                List<Result> results = [sreResult.ToResult()];

                if (fitFileContents is { Length: > 0 })
                {
                    // [TO20260504] Not sure how important the file name really is.
                    string syntheticFileName = simpleRangeEvent.Id + "_" +
                                               simpleRangeEvent.EventDate.ToString("yyyyMMdd") + ".fit";
                    Result<EntityId> fitResult = await _filesDbService
                        .UpsertFitFileAsync(conn,
                            await Nanoid.GenerateAsync(),
                            new ReadOnlyMemory<byte>(fitFileContents),
                            syntheticFileName, cancellationToken)
                        .ConfigureAwait(false);

                    results.Add(fitResult.ToResult());

                    if (fitResult.IsSuccess)
                    {
                        Result<long?> joinResult = await _filesDbService
                            .AssociateWithRangeEvent(conn, simpleRangeEvent.Id!, fitResult.Value.Id, cancellationToken)
                            .ConfigureAwait(false);
                        results.Add(joinResult.ToResult());
                    }
                }

                if (!string.IsNullOrEmpty(shotViewCsvContents))
                {
                    Result<EntityId> shotViewResult = await _shotViewFilesDbService
                        .UpsertShotViewFileAsync(conn,
                            await Nanoid.GenerateAsync(),
                            shotViewCsvContents,
                            shotViewFileName, cancellationToken)
                        .ConfigureAwait(false);

                    results.Add(shotViewResult.ToResult());

                    if (shotViewResult.IsSuccess)
                    {
                        Result<long?> joinResult = await _shotViewFilesDbService
                            .AssociateWithRangeEvent(conn, simpleRangeEvent.Id!, shotViewResult.Value.Id,
                                cancellationToken)
                            .ConfigureAwait(false);
                        results.Add(joinResult.ToResult());
                    }
                }

                finalResult = Result.Merge(results.ToArray()).ToResult(simpleRangeEvent.RowId);
            }
            catch (Exception e)
            {
                Error? err = new Error("Failed to upsert simple range event with file contents.")
                    .Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId)
                    .CausedBy(e);
                finalResult = Result.Fail<long?>(err);
            }

            return finalResult;
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

            return await _simpleRangeEventService.GetSimpleRangeEventsAsync(conn, cancellationToken);
        }

        public async Task<Result<bool>> DeleteAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

            return await _simpleRangeEventService.DeleteAsync(conn, simpleRangeEvent, cancellationToken);
        }

        /// <summary>
        ///     Add a firearm using the firearmName from the SimpleRangeEvent. If the firearm exists, then update the
        ///     Modified field.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="simpleRangeEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<Result<long?>> HandleFirearmUpsertAsync(SqliteConnection conn,
            SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
