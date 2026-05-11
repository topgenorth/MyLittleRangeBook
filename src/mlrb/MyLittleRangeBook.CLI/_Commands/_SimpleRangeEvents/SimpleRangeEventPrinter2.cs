using MyLittleRangeBook.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI.Console
{
    public class SimpleRangeEventPrinter2 : ISimpleRangeEventPrinter
    {
        public void Print(IAnsiConsole console, SimpleRangeEvent sre, bool quiet = false)
        {
            if (quiet)
            {
                console.Write(QuietLayout(sre));

                return;
            }

            console.Write(TryLayouts(sre));
        }

        IRenderable QuietLayout(SimpleRangeEvent sre)
        {
            Grid grid = new Grid().Expand()
                .AddColumn()
                .AddColumn()
                .AddColumn()
                .AddColumn()
                .AddColumn();
            grid.AddRow("Id: " + sre.Id?.Trim(),
                ", Date: " + sre.EventDate.ToString("yyyy-MM-dd"),
                ", Firearm: " + sre.FirearmName.Trim(),
                ", Range: " + sre.RangeName.Trim(),
                ", Rounds: " + sre.RoundsFired);

            return grid;
        }

        IRenderable TryLayouts(SimpleRangeEvent sre)
        {
            Table table = new Table()
                .Border(TableBorder.Square)
                .Expand()
                .AddColumn("Date", col => col.Alignment(Justify.Center).Width(10).Padding(0, 0))
                .AddColumn("Firearm", col => col.Alignment(Justify.Center))
                .AddColumn("Range", col => col.Alignment(Justify.Center))
                .AddColumn("Rounds", col => col.Alignment(Justify.Center).Width(6));
            table.AddRow(
                sre.EventDate.ToString("yyyy-MM-dd"),
                sre.FirearmName.Trim(),
                sre.RangeName.Trim(),
                sre.RoundsFired.ToString().Trim()
            );

            Layout root = new Layout("root").SplitRows(
                new Layout("details"),
                new Layout("ammo"),
                new Layout("notes")
            );
            root["details"].Update(table);

            GridColumn c1 = new GridColumn().Padding(0, 0).Width(7);
            GridColumn c2 = new GridColumn().Padding(0, 0);
            Grid grid = new Grid().AddColumn(c1).AddColumn(c2).Expand();
            grid.AddRow("Ammo: ", sre.AmmoDescription?.Trim() ?? string.Empty);
            root["ammo"].Update(new Panel(grid).Border(BoxBorder.Square).Expand().Padding(0,0));

            GridColumn c3 = new GridColumn().Padding(0, 0).Width(7);
            GridColumn c4 = new GridColumn().Padding(0, 0);
            Grid grid2 = new Grid().AddColumn(c3).AddColumn(c4).Collapse();
            grid2.AddRow("Notes: ", sre.Notes?.Trim() ?? string.Empty);
            root["notes"].Update(new Panel(grid2).Border(BoxBorder.Square).Expand());

            return new Panel(root).Border(BoxBorder.Square).Expand();
        }
    }
}
