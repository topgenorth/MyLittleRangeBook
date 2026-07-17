using System.Data.Common;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.MlrbAssets.Handlers
{
    public class InsertAssetFileSqliteHandler : IPipelineHandler<MlrbAssetFile>
    {
        readonly IFirearmsService _firearmsService;
        readonly ISqliteHelper    _sqliteHelper;

        public InsertAssetFileSqliteHandler(ISqliteHelper sqliteHelper, IFirearmsService firearmsService)
        {
            _sqliteHelper    = sqliteHelper;
            _firearmsService = firearmsService;
        }

        public string Name => "Adding/updating MLRB asset in SQLite database.";

        public async Task<Result> ExecuteAsync(PipelineContext<MlrbAssetFile>                     context,
                                               Func<PipelineContext<MlrbAssetFile>, Task<Result>> next)
        {
            string fileExtension = Path.GetExtension(context.Record.FileToImport);
            context.Metadata["FileExtension"] = fileExtension;
            await using SqliteConnection conn =
                await _sqliteHelper.GetDatabaseConnectionAsync().ConfigureAwait(false);
            await using DbTransaction trans =
                await conn.BeginTransactionAsync(context.CancellationToken).ConfigureAwait(false);
            DapperCommandContext dapperCtx = new(conn, trans, context.CancellationToken);

            try
            {
                AssetRow assetRow = new(null,
                                        context.Record.Id.ToString(),
                                        context.Record.FileToImport,
                                        context.Record.DestinationFile,
                                        context.Record.MimeType,
                                        context.Record.FileContents,
                                        Created: context.Record.Created,
                                        Modified: context.Record.Modified,
                                        Sha256: context.Record.SHA256!);


                // @Id, @OriginalFilename, @PathToRangeAssetFile, @MimeType, @FileContentBytes, @Created, @Modified, @Sha256
                var p1 = new
                         {
                             assetRow.Id,
                             OriginalFilename     = assetRow.OriginalFileName,
                             PathToRangeAssetFile = assetRow.PathToAssetFile,
                             assetRow.MimeType,
                             assetRow.FileContentBytes,
                             assetRow.Created,
                             assetRow.Modified,
                             assetRow.Sha256,
                         };

                DapperCommandContext ctx1 = dapperCtx with { Arguments = p1 };
                int i = await Commands.s_upsertCommand
                                      .ExecuteAsync(ctx1)
                                      .ConfigureAwait(false);

                MlrbId assetId;
                switch (i)
                {
                    case 1:
                        assetId                              = assetRow.Id;
                        context.Metadata["InsertIntoSqlite"] = true;
                        context.Record.Aggregate.StoredInDatabase(assetRow.FileContentBytes, DateTimeOffset.UtcNow);
                        break;
                    case 0:
                        // Odds are that this file was already inserted (SHA256).
                        var p2 = new { assetRow.Sha256 };
                        DapperCommandContext ctx2 = dapperCtx with { Arguments = p2 };
                        string? x = await Commands.s_getAssetId.ExecuteScalarAsync<string>(ctx2).ConfigureAwait(false);
                        assetId                                   = MlrbId.FromString(x!);
                        context.Metadata["InsertIntoSqlite"]      = false;
                        context.Metadata["InsertIntoSqliteError"] = "File already exists in database.";
                        break;
                    default:
                    {
                        await trans.RollbackAsync(context.CancellationToken).ConfigureAwait(false);
                        string msg = $"Expected to affect 1 row, but affected {i} rows.";
                        context.Metadata["InsertIntoSqlite"]      = false;
                        context.Metadata["InsertIntoSqliteError"] = msg;
                        context.Record.Aggregate.Fail(msg, DateTimeOffset.UtcNow);

                        return Result.Fail(msg);
                    }
                }


                await trans.CommitAsync(context.CancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync(context.CancellationToken).ConfigureAwait(false);
                context.Metadata["InsertIntoSqlite"]      = false;
                context.Metadata["InsertIntoSqliteError"] = ex.Message;
                context.Record.Aggregate.Fail(ex, DateTimeOffset.UtcNow);
                return Result.Fail(ex.ToString());
            }

            return await next(context);
        }

        static class Commands
        {
            const string GET_ASSET_ID_VIA_SHA256_SQL = """
                                                        SELECT id from main.asset_files WHERE sha256=@Sha256;
                                                       """;

            const string UPSERT_ASSET_FILES_SQL = """
                                                  INSERT INTO asset_files (
                                                      id,
                                                      original_file_name,
                                                      path_to_asset_file,
                                                      mime_type,
                                                      file_content_bytes,
                                                      created,
                                                      modified,
                                                      sha256
                                                  )
                                                  VALUES (
                                                      @Id,
                                                      @OriginalFilename,
                                                      @PathToRangeAssetFile,
                                                      @MimeType,
                                                      @FileContentBytes,
                                                      @Created,
                                                      @Modified,
                                                      @Sha256
                                                  )
                                                  ON CONFLICT (sha256) DO NOTHING
                                                  ON CONFLICT (id) DO UPDATE SET
                                                      original_file_name = @OriginalFilename,
                                                      path_to_asset_file = @PathToRangeAssetFile,
                                                      mime_type          = @MimeType,
                                                      file_content_bytes = @FileContentBytes,
                                                      modified           = @Modified,
                                                      sha256             = @Sha256
                                                  ;
                                                  """;

            internal static readonly DapperCommand s_upsertCommand = new(UPSERT_ASSET_FILES_SQL);
            internal static readonly DapperCommand s_getAssetId    = new(GET_ASSET_ID_VIA_SHA256_SQL);
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
        /// <param name="Sha256">The SHA256 checksum of the file.</param>
        /// <param name="Created">The date and time when the file was created.</param>
        /// <param name="Modified">The date and time when the file was last modified.</param>
        record AssetRow(
            long?          RowId,
            string         Id,
            string         OriginalFileName,
            string         PathToAssetFile,
            string         MimeType,
            byte[]         FileContentBytes,
            string         Sha256,
            DateTimeOffset Modified,
            DateTimeOffset Created);
    }
}