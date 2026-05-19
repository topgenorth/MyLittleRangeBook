using MyLittleRangeBook.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.Cartridges
{
    class CartridgesTablePrinter : IConsolePrinter
    {
        IEnumerable<Cartridge> _cartridges = [];
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
                .AddColumn("Name", col => col.Alignment(Justify.Left))
                .AddColumn("Common Name", col => col.Alignment(Justify.Left))
                .AddColumn("Diameter (mm)", col => col.Alignment(Justify.Right))
                .AddColumn("Diameter (in)", col => col.Alignment(Justify.Right))
                .AddColumn("Rifle", col => col.Alignment(Justify.Center))
                .AddColumn("Pistol", col => col.Alignment(Justify.Center))
                .AddColumn("Id", col => col.Alignment(Justify.Center).Width(21));
            foreach (Cartridge cartridge in _cartridges)
            {
                table.AddRow(cartridge.Name, cartridge.CommonName ?? string.Empty, cartridge.ProjectileDiameterMetric.ToString("F2"), cartridge.ProjectileDiameterImperial.ToString("F3"), cartridge.SuitableForRifle ? "Yes" : "No", cartridge.SuitableForPistol ? "Yes" : "No", cartridge.Id!);
            }
            Panel p = new Panel(table).Expand().Border(BoxBorder.None);
            return p;
        }
        public CartridgesTablePrinter SetCartridges(IEnumerable<Cartridge> cartridges)
        {
            _cartridges = cartridges;
            return this;
        }
    }
}
