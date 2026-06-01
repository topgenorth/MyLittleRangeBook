using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.RangeEventAssets.Handlers
{
    public class InsertAssetFileSqliteHandler : IPipelineHandler<MlrbAssetFile>
    {
        static class Commands
        {
            internal static readonly DapperCommand UpsertCommand = new DapperCommand(UpsertSql);
            const string UpsertSql = """
                                     INSERT INTO asset_files (
                                         id,
                                         original_file_name,
                                         mime_type,
                                         file_content_bytes,
                                         path_to_asset_file,
                                         created,
                                         modified
                                     )
                                     VALUES (
                                         @Id,
                                         @FileName,
                                         @MimeType,
                                         @Contents,
                                         @PathToRangeAssetFile,
                                         @Created,
                                         @Modified
                                     )
                                     ON CONFLICT (id) DO UPDATE SET
                                         original_file_name             = @FileName,
                                         mime_type             = @MimeType,
                                         contents                = @Contents,
                                         path_to_asset_file = @PathToRangeAssetFile,
                                         modified             = @Modified
                                     ;
                                     """;
        }


        readonly ISqliteHelper _sqliteHelper;

        public InsertAssetFileSqliteHandler(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }

        public string Name => "Import a file as MLRB asset.";

        public async Task<Result> ExecuteAsync(PipelineContext<MlrbAssetFile> context,
            Func<PipelineContext<MlrbAssetFile>, Task<Result>> next)
        {
            string fileExtension = Path.GetExtension(context.Record.PathToAsset);
            context.Metadata["FileExtension"] = fileExtension;

            try
            {
                Result<RangeEventAssetRow> rowResult = await CreateRow(context).ConfigureAwait(false);
                if (rowResult.IsFailed)
                {
                    context.Metadata["InsertIntoSqlite"] = false;
                    context.Metadata["InsertIntoSqliteError"] = rowResult.Errors[0].Message;
                    context.Record.Aggregate.Fail(rowResult.Errors[0].Message, DateTimeOffset.UtcNow);
                }
                else
                {
                    await using SqliteConnection conn =
                        await _sqliteHelper.GetDatabaseConnectionAsync().ConfigureAwait(false);
                    RangeEventAssetRow v = rowResult.Value!;
                    var p = new
                    {
                        v.Id,
                        v.FileName,
                        v.MimeType,
                        Contents = v.FileContents,
                        PathToRangeAssetFile = context.Record.PathToAsset,
                        v.Created,
                        Modified = DateTimeOffset.UtcNow
                    };

                    var dapperCtx = new DapperCommandContext(conn, null, context.CancellationToken);
                    int i = await Commands.UpsertCommand.ExecuteAsync(dapperCtx).ConfigureAwait(false);
                    if (i == 1)
                    {
                        context.Metadata["InsertIntoSqlite"] = true;
                        context.Record.Aggregate.StoredInDatabase(v.FileContents, DateTimeOffset.UtcNow);
                    }
                    else
                    {
                        var msg = $"Expected to affect 1 row, but affected {i} rows.";
                        context.Metadata["InsertIntoSqlite"] = false;
                        context.Metadata["InsertIntoSqliteError"] = msg;
                        context.Record.Aggregate.Fail(msg, DateTimeOffset.UtcNow);

                        return Result.Fail(msg);
                    }
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

        async Task<Result<RangeEventAssetRow>> CreateRow(PipelineContext<MlrbAssetFile> context)
        {
            Result<ReadOnlyMemory<byte>> fileContents = await context.Record.PathToAsset
                .LoadFileBytesAsync(CancellationToken.None)
                .ConfigureAwait(false);
            if (fileContents.IsFailed)
            {
                context.Metadata["InsertIntoSqlite"] = false;
                context.Metadata["InsertIntoSqliteError"] = fileContents.Errors[0].Message;

                return Result.Fail(fileContents.Errors[0].Message);
            }

            string fileExtension = Path.GetExtension(context.Record.PathToAsset);
            var row = new RangeEventAssetRow(
                null,
                context.Record.Id.ToString(),
                Path.GetFileName(context.Record.PathToAsset),
                FileExtensions.GetMimeType(fileExtension),
                fileContents.Value.ToArray(),
                Created: DateTimeOffset.UtcNow,
                Modified: DateTimeOffset.UtcNow);

            return Result.Ok(row);
        }

        /// <summary>
        ///     Represents a record for storing a Garmin FIT file in a Sqlite database.
        /// </summary>
        /// <param name="RowId">The ID of the row in the database.</param>
        /// <param name="Id">The ID of the range event asset file.</param>
        /// <param name="FileName">This isn't the full path -it's just the filename with extension.</param>
        /// <param name="MimeType">The MIME type of the file.</param>
        /// <param name="FileContents">The contents of the file.</param>
        /// <param name="Created">The date and time when the file was created.</param>
        /// <param name="Modified">The date and time when the file was last modified.</param>
        record RangeEventAssetRow(
            long? RowId,
            string Id,
            string FileName,
            string MimeType,
            byte[] FileContents,
            DateTimeOffset Modified,
            DateTimeOffset Created);
    }
}
