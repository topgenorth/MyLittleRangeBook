using ConsoleAppFramework;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.MlrbAssets
{
    [RegisterCommands("assets"), UsedImplicitly]
    public class MlrbAssetListCommand : MlrbSqliteCommandBase
    {
        public MlrbAssetListCommand(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper) : base(logger,
            display, sqliteHelper)
        {
        }

        [Command("list"), UsedImplicitly]
        public async Task<int> ListRangeAssets(CancellationToken ct = default)
        {
            CliDisplay.PrintCommandHeader("List range assets.");
            int returnCode = -1;
            await using ScopedSqliteConnection scope = await SqliteHelper.GetScopedDatabaseConnectionAsync(ct)
                .ConfigureAwait(false);
            await using SqliteConnection conn = scope.Connection;

            try
            {
                var ctx = new DapperCommandContext(conn, CancellationToken: ct);
                IEnumerable<AssetRow> list = await Commands.Select.QueryAsync<AssetRow>(ctx).ConfigureAwait(false);

                AssetsTablePrinter printer = new AssetsTablePrinter().SetAssets(list);
                printer.Print(CliDisplay.Console);
                returnCode = ReturnCodes.SUCCESS;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to list range assets");
                returnCode = ReturnCodes.FAILURE;
            }

            PressEnterToContinue();

            return returnCode;
        }

        internal record struct AssetRow(string Id, long RowId, string MimeType, string PathToAsset);

        static class Commands
        {
            const string Sql = """
                               SELECT id as Id, 
                                      row_id as RowId, 
                                      mime_type AS MimeType,
                                      path_to_asset_file AS PathToAsset
                               FROM asset_files 
                               ORDER BY id;
                               """;

            internal static readonly DapperCommand Select = new(Sql);
        }
    }
}
