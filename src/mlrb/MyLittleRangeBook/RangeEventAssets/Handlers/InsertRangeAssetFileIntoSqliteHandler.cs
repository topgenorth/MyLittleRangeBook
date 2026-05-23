using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.RangeEventAssets.Handlers
{
    public class InsertRangeAssetFileIntoSqliteHandler : IPipelineHandler<RangeEventAssetFile>
    {
        const string UpsertSql = """
                                 INSERT INTO RangeAssetFiles (
                                     Id,
                                     FileName,
                                     MimeType,
                                     Contents,
                                     PathToRangeAssetFile,
                                     Created,
                                     Modified
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
                                 ON CONFLICT (Id) DO UPDATE SET
                                     FileName             = @FileName,
                                     MimeType             = @MimeType,
                                     Contents             = @Contents,
                                     PathToRangeAssetFile = @PathToRangeAssetFile,
                                     Modified             = @Modified
                                 ;
                                 """;

        const int BufferSize = 81920;
        readonly ISqliteHelper _sqliteHelper;

        public InsertRangeAssetFileIntoSqliteHandler(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }

        public string Name => "Store Garmin FIT file in Sqlite database";

        public async Task<Result> ExecuteAsync(PipelineContext<RangeEventAssetFile> context,
            Func<PipelineContext<RangeEventAssetFile>, Task<Result>> next)
        {
            string fileExtension = Path.GetExtension(context.Record.PathToAsset);
            context.Metadata["FileExtension"] = fileExtension;

            try
            {
                Result<ReadOnlyMemory<byte>> fileContents = await context.Record.PathToAsset
                    .LoadFileBytesAsync(CancellationToken.None)
                    .ConfigureAwait(false);

                if (fileContents.IsFailed)
                {
                    context.Metadata["InsertIntoSqlite"] = false;
                    context.Metadata["InsertIntoSqliteError"] = fileContents.Errors[0].Message;

                    return await next(context);
                }

                var row = new RangeEventAssetRow(
                    null,
                    context.Record.Id.ToString(),
                    Path.GetFileName(context.Record.PathToAsset),
                    FileExtensions.GetMimeType(fileExtension),
                    fileContents.Value.ToArray(),
                    Created: DateTimeOffset.UtcNow,
                    Modified: DateTimeOffset.UtcNow);

                await using SqliteConnection conn =
                    await _sqliteHelper.GetDatabaseConnectionAsync().ConfigureAwait(false);
                var cd = new CommandDefinition(UpsertSql, row);
                int i = await conn.ExecuteAsync(cd).ConfigureAwait(false);
                context.Metadata["InsertIntoSqlite"] = i == 1;
            }
            catch (Exception ex)
            {
                context.Metadata["InsertIntoSqlite"] = false;
                context.Metadata["InsertIntoSqliteError"] = ex.Message;
            }

            return await next(context);
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
        internal record RangeEventAssetRow(
            long? RowId,
            string Id,
            string FileName,
            string MimeType,
            byte[] FileContents,
            DateTimeOffset Modified,
            DateTimeOffset Created);
    }
}
