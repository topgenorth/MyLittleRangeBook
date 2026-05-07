using MyLittleRangeBook.Models;
using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    public class SimpleRangeEventPrinter : ISimpleRangeEventPrinter
    {
        readonly ICliDisplay _cliDisplay;

        public SimpleRangeEventPrinter(ICliDisplay cliDisplay)
        {
            _cliDisplay = cliDisplay;
        }

        public void Print(IAnsiConsole console, SimpleRangeEvent sre, bool quiet = false)
        {
            if (quiet)
            {
                console.MarkupLineInterpolated($"[green]Range Trip added: RowId {sre.RowId}, Id {sre.Id}.[/]");
            }
            else
            {
                Grid bodyGrid = new Grid().AddColumns(2);
                bodyGrid.AddRow("", "[green]Range Trip Added[/]");

                bodyGrid.AddRow("  [white]RowId:[/]", sre.RowId.ToString() ?? string.Empty);
                bodyGrid.AddRow("  [white]Id:[/]", sre.Id!);
                bodyGrid.AddRow("  [white]Date:[/]", sre.EventDate.ToString("yyyy-MMM-dd"));
                bodyGrid.AddRow("  [white]Firearm:[/]", sre.FirearmName);
                bodyGrid.AddRow("  [white]Range:[/] ", sre.RangeName);

                if (sre.RoundsFired > 0)
                {
                    bodyGrid.AddRow("  [white]Rounds:[/] ", sre.RoundsFired.ToString());
                }

                if (!string.IsNullOrWhiteSpace(sre.AmmoDescription))
                {
                    bodyGrid.AddRow("  [white]Ammo:[/] ", sre.AmmoDescription);
                }

                if (!string.IsNullOrWhiteSpace(sre.Notes))
                {
                    bodyGrid.AddRow("  [white]Notes:[/] ", sre.Notes);
                }

                Layout layout = new Layout("root")
                    .SplitRows(new Layout("header"), new Layout("body"));
                Grid headerGrid = CreateHeaderGrid();
                layout["header"].Update(headerGrid);
                layout["body"].Update(bodyGrid);
                console.Write(layout);
            }
        }

        Grid CreateHeaderGrid()
        {
            var headerGrid = new Grid();
            headerGrid.AddColumn();
            headerGrid.AddRow($"[grey]Version:[/] [green]{Markup.Escape(_cliDisplay.AppVersion)}[/]");

            return headerGrid;
        }
    }
}
