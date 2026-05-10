using MyLittleRangeBook.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI.Console
{
    public class SimpleRangeEventPrinter2 : ISimpleRangeEventPrinter
    {
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
        public void Print(IAnsiConsole console, SimpleRangeEvent sre, bool quiet = false)
        {
            if (quiet)
            {
                console.Write(QuietLayout(sre));
                return;
            }

            console.Write(TryLayouts(sre));
        }

        IRenderable TryLayouts(SimpleRangeEvent sre)
        {
            Table table = new Table()
                .Border(TableBorder.Square)
                .HideFooters()
                .Expand()
                .AddColumn("Date", col => col.Alignment(Justify.Center).Width(10))
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

            Grid grid = new Grid().AddColumn().AddColumn();
            grid.AddRow("Ammo: ", sre.AmmoDescription?.Trim() ?? string.Empty);
            root["ammo"].Update(new Panel(grid).Border(BoxBorder.Square).Expand());

            // root["ammo"].SplitColumns(new Layout("ammolabel"), new Layout("ammocontent"));
            // root["ammolabel"].Update(new Text("Ammo:"));
            // root["ammocontent"].Update(new Text(sre.AmmoDescription?.Trim() ?? string.Empty));

            root["notes"].SplitColumns(new Layout("noteslabel"), new Layout("notescontent"));
            root["noteslabel"].Update(new Text("Notes:"));
            root["notescontent"].Update(new Text(sre.Notes?.Trim() ?? string.Empty));

            return root;
        }

    }
}
