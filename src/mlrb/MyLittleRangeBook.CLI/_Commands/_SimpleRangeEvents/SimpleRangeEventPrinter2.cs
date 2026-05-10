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
                console.MarkupLineInterpolated($"[green]Range Trip RowId {sre.RowId}, Id {sre.Id}.[/]");

                return;
            }

            console.Write(TryLayouts(sre));
        }

        IRenderable TryLayouts(SimpleRangeEvent sre)
        {
            Table table = new Table()
                .Border(TableBorder.Square)
                .Expand()
                .AddColumn("Date", col => col.Alignment(Justify.Center).Width(10))
                .AddColumn("Firearm", col => col.Alignment(Justify.Center))
                .AddColumn("Range", col => col.Alignment(Justify.Center))
                .AddColumn("Rounds", col => col.Alignment(Justify.Center).Width(6));
            table.AddRow(
                sre.EventDate.ToString("yyyy-MM-dd"),
                sre.FirearmName,
                sre.RangeName,
                sre.RoundsFired.ToString()
            );

            Layout root = new Layout("root").SplitRows(
                new Layout("details"),
                new Layout("ammo"),
                new Layout("notes")
            );

            root["details"].Update(table);

            root["ammo"].SplitColumns(new Layout("ammolabel"), new Layout("ammocontent"));
            root["ammolabel"].Update(new Text("Ammo:"));
            root["ammocontent"].Update(new Text(sre.AmmoDescription?.Trim() ?? string.Empty));

            root["notes"].SplitColumns(new Layout("noteslabel"), new Layout("notescontent"));
            root["noteslabel"].Update(new Text("Notes:"));
            root["notescontent"].Update(new Text(sre.Notes?.Trim() ?? string.Empty));

            return root;

        }

        IRenderable TryTable(SimpleRangeEvent sre)
        {
            Table table = new Table()
                .Expand()
                .AddColumn("Date", col => col.Alignment(Justify.Center))
                .AddColumn("Firearm", col => col.Alignment(Justify.Center))
                .AddColumn("Range", col => col.Alignment(Justify.Left))
                .AddColumn("Rounds", col => col.Alignment(Justify.Center));

            table.AddRow(sre.EventDate.ToString("yyyy-MM-dd"), sre.FirearmName, sre.RangeName,
                sre.RoundsFired.ToString());

            if (!string.IsNullOrWhiteSpace(sre.AmmoDescription))
            {
                table.AddRow("Ammo", sre.AmmoDescription!, string.Empty, string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(sre.Notes))
            {
                table.AddRow("Notes", sre.Notes!, string.Empty, string.Empty, string.Empty);
            }

            return table;
        }
    }
}
