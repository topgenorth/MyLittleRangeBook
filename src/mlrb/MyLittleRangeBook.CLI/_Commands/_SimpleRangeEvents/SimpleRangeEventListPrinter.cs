using MyLittleRangeBook.Models;
using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    /// <summary>
    ///     Prints a list of SimpleRangeEvents to the console.
    /// </summary>
    public interface ISimpleRangeEventListPrinter
    {
        Task<ISimpleRangeEventListPrinter> Start();
        Task<ISimpleRangeEventListPrinter> AddRow(SimpleRangeEvent simpleRangeEvent);
        Task<ISimpleRangeEventListPrinter> Finish();
        Task Print(IEnumerable<SimpleRangeEvent> simpleRangeEvents, CancellationToken cancellationToken = default);
    }

    public class SimpleRangeEventListPrinter : ISimpleRangeEventListPrinter
    {
        readonly ICliDisplay _cliDisplay;

        public SimpleRangeEventListPrinter(ICliDisplay cliDisplay)
        {
            _cliDisplay = cliDisplay;
        }

        internal IAnsiConsole Console => _cliDisplay.Console;

        public async Task<ISimpleRangeEventListPrinter> Start()
        {
            Console.MarkupLine("[white]Simple Range Events[/]");

            return this;
        }

        public async Task<ISimpleRangeEventListPrinter> AddRow(SimpleRangeEvent simpleRangeEvent)
        {
            Console.MarkupLine($"{simpleRangeEvent.EventDate:yyyy-MM-dd}, {simpleRangeEvent.Id}, {simpleRangeEvent.FirearmName} ({simpleRangeEvent.RoundsFired} rounds)");

            return this;
        }

        public async Task<ISimpleRangeEventListPrinter> Finish()
        {
            Console.MarkupLine("[white]Finished{/]");
            return this;
        }

        public async Task Print(IEnumerable<SimpleRangeEvent> simpleRangeEvents, CancellationToken cancellationToken = default)
        {
        }
    }
}
