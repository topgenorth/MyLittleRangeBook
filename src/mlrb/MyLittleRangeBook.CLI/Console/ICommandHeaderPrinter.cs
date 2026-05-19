namespace MyLittleRangeBook.Console
{
    public interface ICommandHeaderPrinter
    {
        void Print(IAnsiConsole console);
        ICommandHeaderPrinter SetAction(string? action);
    }
}