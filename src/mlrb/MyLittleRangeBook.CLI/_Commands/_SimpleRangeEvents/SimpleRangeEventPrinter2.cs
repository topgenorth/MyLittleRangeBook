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

            console.Write(TryPanels(sre));
        }

        IRenderable TryPanels(SimpleRangeEvent sre)
        {
            var t1 = new Text("Date: " + sre.EventDate.ToString("yyyy-MM-dd"));
            var t2 = new Text("Firearm: " + sre.FirearmName);
            var t3 = new Text("Range: " + sre.RangeName);
            var t4 = new Text("Rounds: " + sre.RoundsFired);

            Layout root = new Layout("root").SplitRows(
                new Layout("details").Size(5),
                new Layout("ammo") /*,
                new Layout("notes")*/);

            // root["details"].SplitColumns(
            //         new Layout("date"),
            //         new Layout("firearm"),
            //         new Layout("range"),
            //         new Layout("rounds"));
            // root["date"].Update(t1);
            // root["firearm"].Update(t2);
            // root["range"].Update(t3);
            // root["rounds"].Update(t4);


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

            // root["ammo"].SplitColumns(new Layout("ammolabel"), new Layout("ammocontent"));
            // root["ammolabel"].Update(new Text("Ammo:")).Size(6);
            // root["ammocontent"].Update(new Text(sre.AmmoDescription ?? string.Empty));

            Grid g1 = new Grid()
                .Collapse()
                .AddColumn()
                .AddColumn()
                .AddRow("Ammo: ", sre.AmmoDescription?.Trim() ?? string.Empty)
                .AddRow("Notes: ", sre.Notes?.Trim() ?? string.Empty);
            // Panel p = new Panel(g1).Padding(0, 0);
            // root["ammo"].Update(g1).Ratio(2);
            // root["ammo"].Update(new Text("Up"));
            // var p = new Panel(new Text("Notes: " + sre.Notes ?? string.Empty)).Padding(0,0).Expand();
            // root["noteslabel"].Update(new Text("Notes:")).Size(7);
            // root["notescontent"].Update(p);

            // if (string.IsNullOrWhiteSpace(sre.Notes))
            // {
            //     root["notes"].Invisible();
            // }

            // root["notes"].SplitColumns(new Layout("noteslabel"), new Layout("notescontent")).Invisible();
            // if (!string.IsNullOrWhiteSpace(sre.AmmoDescription))
            // {
            //     root["ammo"].Visible();
            // }

            var p3 = new Panel(root).Border(BoxBorder.Square).Collapse();
            // p3.Height = 10;

            return root;
            // // return p3;

            var row1 = new Columns(t1, t2, t3, t4);
            Table t = new Table().AddColumn("Date").AddColumn("Firearm").AddColumn("Range").AddColumn("Rounds Fired");
            t.Title = new TableTitle("Range Event " + sre.Id);
            t.AddRow(t1, t2, t3, t4);
            t.Expand();

            var rows = new List<IRenderable> { row1 };

            if (!string.IsNullOrWhiteSpace(sre.AmmoDescription))
            {
                var r2c1 = new Text("Ammo:");
                var r2c2 = new Text(sre.AmmoDescription!);
                var columns = new Columns(r2c1, r2c2);
                rows.Add(columns);
            }

            if (!string.IsNullOrWhiteSpace(sre.Notes))
            {
                var r3c1 = new Text("Notes:");
                var r3c2 = new Text(sre.Notes!);
                rows.Add(new Columns(r3c1, r3c2));
            }

            Layout container;
            if (rows.Count == 1)
            {
                container = new Layout();
                container.Update(rows[0]);
            }
            else if (rows.Count == 2)
            {
                container = new Layout("Range Event").SplitRows(new Layout("details"), new Layout("ammo"));
                container["details"].Update(rows[0]);
                container["ammo"].Update(rows[1]);
            }
            else
            {
                container = new Layout("Range Event").SplitRows(new Layout("details"), new Layout("ammo"),
                    new Layout("notes"));
                container["details"].Update(rows[0]);
                container["ammo"].Update(rows[1]);
                container["notes"].Update(rows[2]);
            }

            var r = new Rows(rows);
            r.Expand();


            return r;
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
