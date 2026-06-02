using System.Data.Common;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.RangeEventAssets.Handlers
{
    public class InsertAssetFileSqliteHandler : IPipelineHandler<MlrbAssetFile>
    {
        readonly ISqliteHelper _sqliteHelper;

        public InsertAssetFileSqliteHandler(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }

        public string Name => "Adding/updating MLRB asset in SQLite database.";

        public async Task<Result> ExecuteAsync(PipelineContext<MlrbAssetFile> context,
            Func<PipelineContext<MlrbAssetFile>, Task<Result>> next)
        {
            string fileExtension = Path.GetExtension(context.Record.FileToImport);
            context.Metadata["FileExtension"] = fileExtension;

            try
            {
                var assetRow = new AssetRow(null,
                    context.Record.Id.ToString(),
                    context.Record.FileToImport,
                    context.Record.DestinationFile,
                    context.Record.MimeType,
                    context.Record.FileContents,
                    Created: context.Record.Created,
                    Modified: context.Record.Modified);

                await using SqliteConnection conn =
                    await _sqliteHelper.GetDatabaseConnectionAsync().ConfigureAwait(false);
                await using DbTransaction trans =
                    await conn.BeginTransactionAsync(context.CancellationToken).ConfigureAwait(false);

                // @Id, @OriginalFilename, @PathToRangeAssetFile, @MimeType, @FileContentBytes, @Created, @Modified
                var p = new
                {
                    assetRow.Id,
                    OriginalFilename = assetRow.OriginalFileName,
                    PathToRangeAssetFile = context.Record.FileToImport,
                    assetRow.MimeType,
                    assetRow.FileContentBytes,
                    assetRow.Created,
                    Modified = context.Record.Aggregate.Modified
                };

                var dapperCtx = new DapperCommandContext(conn, trans, context.CancellationToken) { Arguments = p };
                int i = await Commands.UpsertCommand
                    .ExecuteAsync(dapperCtx)
                    .ConfigureAwait(false);
                if (i == 1)
                {
                    await trans.CommitAsync(context.CancellationToken).ConfigureAwait(false);
                    context.Metadata["InsertIntoSqlite"] = true;
                    context.Record.Aggregate.StoredInDatabase(assetRow.FileContentBytes, DateTimeOffset.UtcNow);
                }
                else
                {
                    await trans.RollbackAsync(context.CancellationToken).ConfigureAwait(false);
                    var msg = $"Expected to affect 1 row, but affected {i} rows.";
                    context.Metadata["InsertIntoSqlite"] = false;
                    context.Metadata["InsertIntoSqliteError"] = msg;
                    context.Record.Aggregate.Fail(msg, DateTimeOffset.UtcNow);

                    return Result.Fail(msg);
                }
            }
            catch (Exception ex)
            {
                context.Metadata["InsertIntoSqlite"] = false;
                context.Metadata["InsertIntoSqliteError"] = ex.Message;
                context.Record.Aggregate.Fail(ex, DateTimeOffset.UtcNow);

                return Result.Fail(ex.ToString());
            }

            return await next(context);
        }

        static class Commands
        {
            const string UpsertSql = """
                                     INSERT INTO asset_files (
                                         id,
                                         original_file_name,
                                         path_to_asset_file,
                                         mime_type,
                                         file_content_bytes,
                                         created,
                                         modified
                                     )
                                     VALUES (
                                         @Id, 
                                         @OriginalFilename,
                                         @PathToRangeAssetFile,
                                         @MimeType,
                                         @FileContentBytes,
                                         @Created,
                                         @Modified
                                     )
                                     ON CONFLICT (id) DO UPDATE SET
                                         original_file_name = @OriginalFilename,
                                         path_to_asset_file = @PathToRangeAssetFile,
                                         mime_type          = @MimeType,
                                         file_content_bytes = @FileContentBytes,
                                         modified           = @Modified
                                     ;
                                     """;

            internal static readonly DapperCommand UpsertCommand = new(UpsertSql);
        }

        /// <summary>
        ///     Represents a record for storing a Garmin FIT file in a Sqlite database.
        /// </summary>
        /// <param name="RowId">The ID of the row in the database.</param>
        /// <param name="Id">The ID of the range event asset file.</param>
        /// <param name="OriginalFileName">This isn't the full path -it's just the filename with extension.</param>
        /// <param name="PathToAssetFile">The full path to the asset file on disk.</param>
        /// <param name="MimeType">The MIME type of the file.</param>
        /// <param name="FileContentBytes">The contents of the file.</param>
        /// <param name="Created">The date and time when the file was created.</param>
        /// <param name="Modified">The date and time when the file was last modified.</param>
        record AssetRow(
            long? RowId,
            string Id,
            string OriginalFileName,
            string PathToAssetFile,
            string MimeType,
            byte[] FileContentBytes,
            DateTimeOffset Modified,
            DateTimeOffset Created);
    }
}
