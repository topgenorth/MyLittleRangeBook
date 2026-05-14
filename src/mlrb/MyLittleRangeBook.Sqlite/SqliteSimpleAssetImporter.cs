using Dapper;
using FluentResults;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEventAssets;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///  Will copy the asset to the RangeAsset directory for the app, and update the Sqlite database.
    /// </summary>
    public class SqliteSimpleAssetImporter: IRangeEventAssetImporter
    {
        readonly IRangeEventAssetImporter _inner;
        readonly ISqliteHelper _sqliteHelper;

        public SqliteSimpleAssetImporter(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
            string assetDir = GetAssetDirectory(sqliteHelper.DatabaseFile);
            _inner = new SimpleAssetImporter(assetDir);
        }

        public SqliteSimpleAssetImporter(ISqliteHelper sqliteHelper, IRangeEventAssetImporter inner)
        {
            _sqliteHelper = sqliteHelper;
            _inner = inner;
        }

        public async Task<Result<(string assetId, string destinationPath)>> ImportAssetForRangeEvent(string assetToImport, string rangeEventId, CancellationToken ct = default)
        {
            Result<(string assetId, string destinationPath)> copiedFile = await _inner.ImportAssetForRangeEvent(assetToImport, rangeEventId, ct);
            if (copiedFile.IsFailed)
            {
                return copiedFile;
            }

            if (ct.IsCancellationRequested)
            {
                File.Delete(copiedFile.Value.destinationPath);

                return Result.Fail("Task was cancelled.");
            }

            return await AssociateAssetWithRangeEvent(copiedFile.Value.assetId, rangeEventId,
                copiedFile.Value.destinationPath, ct);

        }

        internal async Task<Result<(string assetId, string assetPath)>> AssociateAssetWithRangeEvent(string assetId, string rangeEventId, string pathToAsset, CancellationToken ct)
        {
            string extension = Path.GetExtension(pathToAsset);
            string mimeType = FileExtensions.GetMimeType(extension);

            // TODO [TO20260514] For now, we just support images.
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(ct).ConfigureAwait(false);

            try
            {
                #region record this in the database.
                const string INSERT_IMAGE_SQL = @"
INSERT INTO RangeEventImages (Id, FileName, MimeType)
VALUES (@Id, @FileName, @MimeType)
ON CONFLICT(Id) DO UPDATE SET
    FileName = excluded.FileName,
    MimeType = excluded.MimeType,
    Modified = CURRENT_TIMESTAMP;";

                await conn.ExecuteAsync(new CommandDefinition(INSERT_IMAGE_SQL,
                    new { Id = assetId, FileName = pathToAsset, MimeType = mimeType },
                    cancellationToken: ct));
                #endregion

                #region Associate the record for the image to the event.
                const string ASSOCIATE_IMAGE_TO_EVENT_SQL = @"
INSERT OR IGNORE INTO SimpleRangeEvent_Images (SimpleRangeEventId, ImageId)
VALUES (@SimpleRangeEventId, @ImageId);";

                await conn.ExecuteAsync(new CommandDefinition(ASSOCIATE_IMAGE_TO_EVENT_SQL,
                    new { SimpleRangeEventId = rangeEventId, ImageId = assetId },
                    cancellationToken: ct));
                #endregion

                return Result.Ok((assetId, pathToAsset));
            }
            catch (Exception e)
            {
                Error err = new Error("Unexpected error trying to associate the asset with the range event").CausedBy(e)
                    .Enrich(rangeEventId);
                err.WithMetadata("asset_destination", pathToAsset);
                return Result.Fail(err);
            }

        }

        internal static string GetAssetDirectory(string sqliteDatabaseFile)
        {
            string? dir = Path.GetDirectoryName(sqliteDatabaseFile);

            if (dir == null)
            {
                throw new ArgumentException("Invalid sqlite database directory path", nameof(sqliteDatabaseFile));
            }

            string assetDir = Path.Combine(dir, SimpleAssetImporter.RangeAssetsFolderName);
            Directory.CreateDirectory(assetDir);

            return assetDir;
        }

    }
}
