using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook
{
    class FirearmsTablePrinter : IConsolePrinter
    {
        IEnumerable<Firearm> _firearms = [];

        public void Print(IAnsiConsole console)
        {
            console.Write(BuildRenderable());
        }

        public IRenderable BuildRenderable()
        {
            Table table = new Table()
                .Border(TableBorder.DoubleEdge)
                .ShowRowSeparators()
                .Expand()
                .BorderColor(Color.White)
                .AddColumn("Name", col => col.Alignment(Justify.Left))
                .AddColumn("Rounds", col => col.Alignment(Justify.Center).Width(6))
                .AddColumn("Id", col => col.Alignment(Justify.Center).Width(26));

            foreach (Firearm firearm in _firearms)
            {
                table.AddRow(firearm.Name, firearm.RoundsFired.ToString(), firearm.Id!);
            }

            Panel p = new Panel(table).Expand().Border(BoxBorder.None);

            return p;
        }

        public FirearmsTablePrinter SetFirearms(IEnumerable<Firearm> firearms)
        {
            _firearms = firearms;

            return this;
        }
    }
}
