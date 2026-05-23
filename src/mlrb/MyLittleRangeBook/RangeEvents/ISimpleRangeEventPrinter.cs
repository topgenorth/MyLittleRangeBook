using MyLittleRangeBook.RangeEvent;
using Spectre.Console;

namespace MyLittleRangeBook.RangeEvents
{
    public interface ISimpleRangeEventPrinter
    {
        /// <summary>
        ///     Will display the single SimpleRangeEvent in the console.
        /// </summary>
        /// <param name="console"></param>
        /// <param name="sre"></param>
        /// <param name="quiet"></param>
        void Print(IAnsiConsole console, SimpleRangeEvent sre, bool quiet);
    }
}
