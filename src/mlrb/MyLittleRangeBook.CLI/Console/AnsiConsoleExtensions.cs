using System.Reflection;
using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    public static class AnsiConsoleExtensions
    {
        public const string AppName = "MyLittleRangeBook CLI";

        public const string WarningGlyph = "⚠";
        public const string SuccessGlyph = "✓";
        public const string ErrorGlyph = "✗";
        public static ICliDisplay WriteAppInfo(this ICliDisplay cliDisplay)
        {
            cliDisplay.Console.WriteAppInfo();
            return cliDisplay;
        }

        public static IAnsiConsole WriteWarning(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold yellow]{WarningGlyph} {message}[/]");

            return console;
        }
        public static IAnsiConsole WriteProblem(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold red]{ErrorGlyph} {message}[/]");

            return console;
        }
        public static IAnsiConsole WriteSuccess(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold green]{SuccessGlyph} {message}[/]");
            return console;
        }
        public static IAnsiConsole WriteAppInfo(this IAnsiConsole console)
        {
            var a = Assembly.GetExecutingAssembly();

            string appVersion = a.GetAssemblyVersionInformation();

            console.MarkupLine($"[bold white]{a.GetName().Name} v{appVersion}[/]");

            return console;
        }
    }
}
