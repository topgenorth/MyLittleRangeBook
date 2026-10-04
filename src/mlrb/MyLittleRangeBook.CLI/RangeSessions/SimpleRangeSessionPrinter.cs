using MyLittleRangeBook.Console;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.RangeSessions
{
    public class SimpleRangeSessionPrinter : ISimpleRangeSessionPrinter
    {
        readonly ICliDisplay _cliDisplay;

        public SimpleRangeSessionPrinter(ICliDisplay cliDisplay)
        {
            _cliDisplay = cliDisplay;
        }

        public void Print(IAnsiConsole console, SimpleRangeSession sre, bool quiet = false)
        {
            if (quiet)
            {
                console.MarkupLineInterpolated($"[green]Range Trip added, Id {sre.Id}.[/]");
            }
            else
            {
                Grid bodyGrid = new Grid().AddColumns(2);

                bodyGrid.AddRow("  [white]Id:[/]", sre.Id!.ToString());
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

                console.Write(bodyGrid);
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
