using MyLittleRangeBook.Console;
using MyLittleRangeBook.RangeEvent;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Prints a list of SimpleRangeEvents to the console.
    /// </summary>
    public interface ISimpleRangeEventListPrinter
    {
        Task<ISimpleRangeEventListPrinter> Start();
        Task<ISimpleRangeEventListPrinter> AddRow(SimpleRangeEvent simpleRangeEvent);
        Task<ISimpleRangeEventListPrinter> Finish();
    }

    public class SimpleRangeEventListPrinter : ISimpleRangeEventListPrinter
    {
        readonly ICliDisplay _cliDisplay;
        Table? _simpleRangeeventTable;

        public SimpleRangeEventListPrinter(ICliDisplay cliDisplay)
        {
            _cliDisplay = cliDisplay;
        }

        internal IAnsiConsole Console => _cliDisplay.Console;

        public async Task<ISimpleRangeEventListPrinter> Start()
        {
            _simpleRangeeventTable = new Table()
                .Border(TableBorder.DoubleEdge)
                .ShowRowSeparators()
                .Expand()
                .BorderColor(Color.White)
                .AddColumn("Id", col => col
                    .Alignment(Justify.Center)
                    .Width(21)
                )
                .AddColumn("Date", col => col.Alignment(Justify.Center))
                .AddColumn("Firearm", col => col.Alignment(Justify.Left))
                .AddColumn("Range", col => col.Alignment(Justify.Center))
                .AddColumn("Rounds", col => col.Alignment(Justify.Center))
                .AddColumn("Ammo", col => col.Alignment(Justify.Left))
                .AddColumn("Notes", col => col.Alignment(Justify.Left));


            return this;
        }

        public async Task<ISimpleRangeEventListPrinter> AddRow(SimpleRangeEvent simpleRangeEvent)
        {
            if (_simpleRangeeventTable is null)
            {
                return this;
            }

            _simpleRangeeventTable.AddRow(
                simpleRangeEvent.Id!,
                simpleRangeEvent.EventDate.ToString("yyyy-MM-dd"),
                simpleRangeEvent.FirearmName,
                simpleRangeEvent.RangeName,
                simpleRangeEvent.RoundsFired.ToString(),
                simpleRangeEvent.AmmoDescription ?? string.Empty,
                simpleRangeEvent.Notes ?? string.Empty
            );

            return this;
        }

        public async Task<ISimpleRangeEventListPrinter> Finish()
        {
            if (_simpleRangeeventTable is null)
            {
                Console.PrintSuccess("Nothing was started.");
            }
            else
            {
                Console.Write(_simpleRangeeventTable);
                Console.PrintSuccess("Finished with list.");
            }

            _simpleRangeeventTable = null;

            return this;
        }
    }
}
