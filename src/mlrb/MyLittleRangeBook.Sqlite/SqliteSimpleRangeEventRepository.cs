using Dapper;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
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
        readonly IRangeEventAssetImporter _importRangeEventAsset;

        public SqliteSimpleRangeEventRepository(ISqliteHelper sqliteHelper,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeLogService simpleRangeEventService,
            [FromKeyedServices(DI_KEYS_SQLITE)] IFitFilesDbService filesDbService,
            [FromKeyedServices(DI_KEYS_SQLITE)] IShotViewFilesDbService shotViewFilesDbService,
            [FromKeyedServices(DI_KEYS_SQLITE)] IRangeEventAssetImporter importRangeEventAsset
            )
        {
            _sqliteHelper = sqliteHelper;
            _simpleRangeEventService = simpleRangeEventService;
            _filesDbService = filesDbService;
            _shotViewFilesDbService = shotViewFilesDbService;
            _importRangeEventAsset = importRangeEventAsset;
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
            return await UpsertAsync(simpleRangeEvent, fitFileContents, string.Empty, string.Empty, string.Empty,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            byte[] fitFileContents,
            string shotViewCsvContents,
            string shotViewFileName,
            string imageFilePath = "",
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
                            new MlrbId().ToString(),
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
                            new MlrbId().ToString(),
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

                if (!string.IsNullOrEmpty(imageFilePath) && File.Exists(imageFilePath))
                {
                    Result<(MlrbId assetId, string destinationPath)> copyImageResult = await _importRangeEventAsset
                        .ImportAssetForRangeEvent(imageFilePath, simpleRangeEvent.Id!, cancellationToken)
                        .ConfigureAwait(false);
                    results.Add(copyImageResult.ToResult());
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

        public async Task<Result<SimpleRangeEvent>> GetAsync(string id, CancellationToken cancellationToken)
        {
            const string SQL = "SELECT * FROM main.SimpleRangeEvents WHERE Id=@Id;";
            await using SqliteConnection conn = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                SimpleRangeEvent? sre = await conn.QueryFirstOrDefaultAsync<SimpleRangeEvent>(SQL, new { Id = id });
                if (sre is not null)
                {
                    return Result.Ok(sre);
                }

                Error err = new Error("Could not find range event " + id + ".").Enrich(id);
                return Result.Fail(err);
            }
            catch (Exception ex)
            {
                Error err = new Error(ex.Message).CausedBy(ex).Enrich(id);

                return Result.Fail(err);
            }

        }

        public async Task<Result<bool>> DeleteAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

            return await _simpleRangeEventService.DeleteAsync(conn, simpleRangeEvent, cancellationToken);
        }
    }
}
