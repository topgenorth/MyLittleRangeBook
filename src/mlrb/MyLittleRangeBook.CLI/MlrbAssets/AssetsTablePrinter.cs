using MyLittleRangeBook.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.MlrbAssets
{
    class AssetsTablePrinter : IConsolePrinter
    {
        IEnumerable<MlrbAssetListCommand.AssetRow> _assets = [];

        public void Print(IAnsiConsole console)
        {
            console.Write(BuildRenderable());
        }

        public IRenderable BuildRenderable()
        {
            Table table = new Table()
                    .Border(TableBorder.Rounded)
                    .Expand()
                    .BorderColor(Color.White)
                    .AddColumn("Id", col => col.Alignment(Justify.Center).Width(26))
                    .AddColumn("Path To Asset", col => col.Alignment(Justify.Left))
                    .AddColumn("MIME Type", col => col.Alignment(Justify.Center))
                    .AddColumn("Row Id", col => col.Alignment(Justify.Right).Width(6))
                ;

            foreach (MlrbAssetListCommand.AssetRow asset in _assets)
            {
                table.AddRow(asset.Id, asset.PathToAsset, asset.MimeType, asset.RowId.ToString());
            }

            Panel p = new Panel(table).Expand().Border(BoxBorder.None);

            return p;
        }

        public AssetsTablePrinter SetAssets(IEnumerable<MlrbAssetListCommand.AssetRow> assets)
        {
            _assets = assets;

            return this;
        }
    }
}
