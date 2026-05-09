using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    public interface ICommandHeaderPrinter
    {
        void Print(IAnsiConsole console);
        ICommandHeaderPrinter SetAction(string? action);
    }
}