using System.Data.Common;
using FluentResults;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEventAssets;
using MyLittleRangeBook.Services;
using MyLittleRangeBook.Sqlite;

namespace MyLittleRangeBook.Database.Sqlite
{
    /// <summary>
    ///     Will copy the asset to the RangeAsset directory for the app, and update the Sqlite database.
    /// </summary>
    [Obsolete("Don't use", true)]
    public class SqliteSimpleAssetImporter : IRangeEventAssetImporter
    {
        const string InsertImageSql = @"
INSERT INTO RangeEventImages (Id, FileName, MimeType)
VALUES (@Id, @FileName, @MimeType)
ON CONFLICT(Id) DO UPDATE SET
    FileName = excluded.FileName,
    MimeType = excluded.MimeType,
    Modified = CURRENT_TIMESTAMP;";

        const string AssociateImageToEventSql = @"
INSERT OR IGNORE INTO SimpleRangeEvent_Images (SimpleRangeEventId, ImageId)
VALUES (@SimpleRangeEventId, @ImageId);";

        readonly IRangeEventAssetImporter? _inner;

        readonly ISqliteHelper _sqliteHelper;

        public SqliteSimpleAssetImporter(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;

            // // TODO [TO20260514] For now, just assume a unique filename each time.
            // _inner = new RangeEventFileAssetImporter(GetAssetDirectory(sqliteHelper.DatabaseFile),
            //     new UniqueAssetNameStrategy());
        }

        public SqliteSimpleAssetImporter(ISqliteHelper sqliteHelper, IRangeEventAssetImporter inner)
        {
            _sqliteHelper = sqliteHelper;
            _inner = inner;
        }

        public async Task<Result<(MlrbId assetId, string destinationPath)>> ImportAssetForRangeEvent(
            string rangeEventId,
            string assetToImport,
            CancellationToken ct = default)
        {
            // [TO20260514] First copy the file over.
            Result<(MlrbId assetId, string destinationPath)> copiedFile =
                await _inner.ImportAssetForRangeEvent(rangeEventId, assetToImport, ct);
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
            Result<MlrbId> result = await AssociateAssetWithRangeEvent(rangeEventId,
                copiedFile.Value.assetId,
                copiedFile.Value.destinationPath, ct);

            if (result.IsSuccess)
            {
                return Result.Ok((result.Value, copiedFile.Value.destinationPath));
            }

            File.Delete(copiedFile.Value.destinationPath);
            return Result.Fail("Didn't associate asset with the range event");


        }

        /// <summary>
        ///     Associates a specified asset with a range event by recording it in the database and linking it to the event.
        /// </summary>
        /// <param name="rangeEventId">The unique identifier of the range event with which the asset will be associated.</param>
        /// <param name="assetId">The unique identifier of the asset to be associated.</param>
        /// <param name="pathToAsset">The file path to the asset being associated with the range event.</param>
        /// <param name="ct">The cancellation token to monitor for cancellation requests.</param>
        /// <returns>
        ///     A result containing the asset ID and the asset's file path if the operation succeeds,
        ///     or an error if the operation fails.
        /// </returns>
        async Task<Result<MlrbId>> AssociateAssetWithRangeEvent(string rangeEventId,
            MlrbId assetId,
            string pathToAsset,
            CancellationToken ct)
        {
            string extension = Path.GetExtension(pathToAsset);
            string mimeType = FileExtensions.GetMimeType(extension);

            // TODO [TO20260514] For now, we just support images.
            await using SqliteConnection
                conn = await _sqliteHelper.GetDatabaseConnectionAsync(ct).ConfigureAwait(false);
            await using DbTransaction trans = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
            try
            {
                var associateRowsAffected = 0;
                var insert = new DapperCommand(InsertImageSql,
                    new { Id = assetId.ToString(), FileName = pathToAsset, MimeType = mimeType });
                int insertRowsAffected = await insert.ExecuteAsync(conn, trans, ct).ConfigureAwait(false);

                if (insertRowsAffected > 0)
                {
                    var associate = new DapperCommand(AssociateImageToEventSql,
                        new { SimpleRangeEventId = rangeEventId, ImageId = assetId.ToString() });
                    associateRowsAffected = await associate.ExecuteAsync(conn, trans, ct);
                }

                if (insertRowsAffected != 0 && associateRowsAffected != 0)
                {
                    await trans.CommitAsync(ct).ConfigureAwait(false);

                    return Result.Ok(assetId);
                }

                await trans.RollbackAsync(ct);
                return Result.Fail("Could not update the database with the file.");
            }
            catch (SqliteException sqe)
            {
                Error err = new Error("Unexpected error trying to associate the asset with the range event")
                    .CausedBy(sqe)
                    .Enrich(rangeEventId);
                err.WithMetadata("asset_destination", pathToAsset);
                await trans.RollbackAsync(ct);
                return Result.Fail(err);
            }
            catch (Exception e)
            {
                Error err = new Error("Unexpected error trying to add the asset to the database")
                    .CausedBy(e)
                    .Enrich(rangeEventId);
                err.WithMetadata("asset_destination", pathToAsset);
                await trans.RollbackAsync(ct);

                return Result.Fail(err);
            }
        }

        /// <summary>
        ///     Determines the directory path for storing assets related to range events, based on the location
        ///     of the SQLite database file. Creates the directory if it does not already exist. The assets directory should
        ///     be a sibling to the SQLite database file.
        /// </summary>
        /// <param name="sqliteDatabaseFile">The file path of the SQLite database file used by the application.</param>
        /// <returns>The full directory path where range event assets should be stored.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when the provided SQLite database file's directory path is invalid or cannot
        ///     be determined.
        /// </exception>
        static string GetAssetDirectory(string sqliteDatabaseFile)
        {
            string? dir = Path.GetDirectoryName(sqliteDatabaseFile);

            if (dir == null)
            {
                throw new ArgumentException("Invalid sqlite database directory path", nameof(sqliteDatabaseFile));
            }

            string assetDir = Path.Combine(dir, ConfigurationExtensions.RangeAssetsFolderName);
            Directory.CreateDirectory(assetDir);

            return assetDir;
        }
    }
}
