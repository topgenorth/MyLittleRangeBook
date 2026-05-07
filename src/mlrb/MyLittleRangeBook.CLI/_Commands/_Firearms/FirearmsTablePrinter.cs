using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI
{
    class FirearmsTablePrinter : IConsolePrinter
    {
        IEnumerable<Firearm> _firearms = [];

        public FirearmsTablePrinter SetFirearms(IEnumerable<Firearm> firearms)
        {
            _firearms = firearms;

            return this;
        }
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
                .AddColumn("Notes", col => col.Alignment(Justify.Left))
                .AddColumn("Id", col => col.Alignment(Justify.Center).Width(21))
                .AddColumn("Row Id", col => col.Alignment(Justify.Center).Width(6));

            foreach (Firearm firearm in _firearms)
            {
                table.AddRow(firearm.Name, firearm.Notes ?? string.Empty, firearm.Id!, firearm.RowId!.ToString() ?? "");
            }

            Panel p = new Panel(table).Expand().Border(BoxBorder.None);

            return p;
        }
    }
}
