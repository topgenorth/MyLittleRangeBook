using MyLittleRangeBook.IO;

namespace MyLittleRangeBook.Console
{
    public class CliDisplay : ICliDisplay
    {
        readonly ICommandHeaderPrinter _commandHeaderPrinter;

        public CliDisplay(IAnsiConsole console, ICommandHeaderPrinter commandHeaderPrinter)
        {
            AppVersion = GetType().Assembly.GetAssemblyVersionInformation();
            Console = console;
            _commandHeaderPrinter = commandHeaderPrinter;
        }

        // [TO20260503] This might be better off as extension methods to IAnsiConsole?
        public string AppVersion { get; }

        public IAnsiConsole Console { get; }


        public void PrintCommandHeader(string? action)
        {
            _commandHeaderPrinter.SetAction(action).Print(Console);
        }

        public void PrintSuccess(string message)
        {
            Console.PrintSuccess(message);
        }

        public void PrintFailure(string message)
        {
            Console.PrintProblem(message);
        }

        public void PrintWarning(string message)
        {
            Console.PrintWarning(message);
        }

        public void PrintInfo(string message)
        {
            Console.MarkupLine($"[blue]{AnsiConsoleExtensions.BulletGlyph} {message}[/]");
        }
    }
}
