using Dapper;
using FluentResults;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEventAssets;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///     Will copy the asset to the RangeAsset directory for the app, and update the Sqlite database.
    /// </summary>
    public class SqliteSimpleAssetImporter : IRangeEventAssetImporter
    {
        readonly IRangeEventAssetImporter _inner;
        readonly ISqliteHelper _sqliteHelper;

        public SqliteSimpleAssetImporter(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;

            // TODO [TO20260514] For now, just assume a unique filename each time.
            _inner = new SimpleAssetImporter(GetAssetDirectory(sqliteHelper.DatabaseFile), new UniqueAssetNameStrategy());
        }

        public SqliteSimpleAssetImporter(ISqliteHelper sqliteHelper, IRangeEventAssetImporter inner)
        {
            _sqliteHelper = sqliteHelper;
            _inner = inner;
        }

        public async Task<Result<(MlrbId assetId, string destinationPath)>> ImportAssetForRangeEvent(
            string assetToImport,
            string rangeEventId,
            CancellationToken ct = default)
        {

            // [TO20260514] First copy the file over.
            Result<(MlrbId assetId, string destinationPath)> copiedFile =
                await _inner.ImportAssetForRangeEvent(assetToImport, rangeEventId, ct);
            if (copiedFile.IsFailed)
            {
                return copiedFile;
            }

            if (ct.IsCancellationRequested)
            {
                File.Delete(copiedFile.Value.destinationPath);

                return Result.Fail("Task was cancelled.");
            }

            // [TO20260514] Now create the records that will associate the event with the asset.
            Result<(MlrbId assetId, string assetPath)> result = await AssociateAssetWithRangeEvent(
                copiedFile.Value.assetId,
                rangeEventId,
                copiedFile.Value.destinationPath,
                ct);

            // TODO [TO20260514] What happens if the association fails?
            return result;
        }

        /// <summary>
        /// Associates a specified asset with a range event by recording it in the database and linking it to the event.
        /// </summary>
        /// <param name="assetId">The unique identifier of the asset to be associated.</param>
        /// <param name="rangeEventId">The unique identifier of the range event with which the asset will be associated.</param>
        /// <param name="pathToAsset">The file path to the asset being associated with the range event.</param>
        /// <param name="ct">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>A result containing the asset ID and the asset's file path if the operation succeeds,
        /// or an error if the operation fails.</returns>
        async Task<Result> AssociateAssetWithRangeEvent(MlrbId assetId,
            string rangeEventId,
            string pathToAsset,
            CancellationToken ct)
        {
            string extension = Path.GetExtension(pathToAsset);
            string mimeType = FileExtensions.GetMimeType(extension);

            // TODO [TO20260514] For now, we just support images.
            await using SqliteConnection
                conn = await _sqliteHelper.GetDatabaseConnectionAsync(ct).ConfigureAwait(false);

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

                return Result.Ok();
            }
            catch (Exception e)
            {
                Error err = new Error("Unexpected error trying to associate the asset with the range event").CausedBy(e)
                    .Enrich(rangeEventId);
                err.WithMetadata("asset_destination", pathToAsset);

                return Result.Fail(err);
            }
        }

        /// <summary>
        /// Determines the directory path for storing assets related to range events, based on the location
        /// of the SQLite database file. Creates the directory if it does not already exist. The assets directory should
        /// be a sibling to the SQLite database file.
        /// </summary>
        /// <param name="sqliteDatabaseFile">The file path of the SQLite database file used by the application.</param>
        /// <returns>The full directory path where range event assets should be stored.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided SQLite database file's directory path is invalid or cannot be determined.</exception>
        static string GetAssetDirectory(string sqliteDatabaseFile)
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
