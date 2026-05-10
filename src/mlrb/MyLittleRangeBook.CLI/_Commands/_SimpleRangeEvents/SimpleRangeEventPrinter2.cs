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
            var t1 = new Text("Date: " + sre.EventDate.ToString("yyyy-MM-dd"));
            var t2 = new Text("Firearm: " + sre.FirearmName);
            var t3 = new Text("Range: " + sre.RangeName);
            var t4 = new Text("Rounds: " + sre.RoundsFired);

            Layout root = new Layout("root").SplitRows(
                new Layout("details"),
                new Layout("ammo"),
                new Layout("notes")
            );

            Table table = new Table()
                .Border(TableBorder.Square)
                .Expand()
                .AddColumn("Date", col => col.Alignment(Justify.Left).Width(10))
                .AddColumn("Firearm", col => col.Alignment(Justify.Center))
                .AddColumn("Range", col => col.Alignment(Justify.Left))
                .AddColumn("Rounds", col => col.Alignment(Justify.Center).Width(6))
                .AddRow(
                    sre.EventDate.ToString("yyyy-MM-dd"),
                    sre.FirearmName,
                    sre.RangeName,
                    sre.RoundsFired.ToString());
            root["details"].Update(table).Ratio(1);

            root["ammo"].SplitColumns(new Layout("ammolabel"), new Layout("ammocontent"));
            root["ammolabel"].Update(new Text("Ammo:")).Size(6);
            root["ammocontent"].Update(new Text(sre.AmmoDescription ?? string.Empty));

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
