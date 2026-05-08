using FluentResults;
using Dapper;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.IO;
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
            return await UpsertAsync(simpleRangeEvent, fitFileContents, string.Empty, string.Empty, string.Empty, cancellationToken)
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

                if (!string.IsNullOrEmpty(imageFilePath) && File.Exists(imageFilePath))
                {
                    Result<string> copyImageResult = await _sqliteHelper.CopyImageToEventHistory(imageFilePath, simpleRangeEvent.Id!);
                    if (copyImageResult.IsSuccess)
                    {
                        string extension = Path.GetExtension(copyImageResult.Value);
                        string mimeType = FileExtensions.GetMimeType(extension);
                        string imageId = await Nanoid.GenerateAsync();
                        string relativePath = Path.GetRelativePath(_sqliteHelper.DatabaseFile,copyImageResult.Value);

                        #region File is copied, record this in the database.
                        const string INSERT_IMAGE_SQL = @"
INSERT INTO RangeEventImages (Id, FileName, MimeType)
VALUES (@Id, @FileName, @MimeType)
ON CONFLICT(Id) DO UPDATE SET
    FileName = excluded.FileName,
    MimeType = excluded.MimeType,
    Modified = CURRENT_TIMESTAMP;";

                        await conn.ExecuteAsync(new CommandDefinition(INSERT_IMAGE_SQL,
                            new { Id = imageId, FileName = relativePath, MimeType = mimeType },
                            cancellationToken: cancellationToken));
                        #endregion

                        #region Associate the record for the image to the event.
                        const string ASSOCIATE_IMAGE_TO_EVENT_SQL = @"
INSERT OR IGNORE INTO SimpleRangeEvent_Images (SimpleRangeEventId, ImageId)
VALUES (@SimpleRangeEventId, @ImageId);";

                        await conn.ExecuteAsync(new CommandDefinition(ASSOCIATE_IMAGE_TO_EVENT_SQL,
                            new { SimpleRangeEventId = simpleRangeEvent.Id, ImageId = imageId },
                            cancellationToken: cancellationToken));
                        #endregion
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

    }
}
