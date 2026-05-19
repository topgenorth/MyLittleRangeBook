using System.Reflection;
using MyLittleRangeBook.IO;

namespace MyLittleRangeBook.Console
{
    public static class AnsiConsoleExtensions
    {
        // [TO20260503] This might be a better way than using ICliDisplay?
        public const string AppName = "MyLittleRangeBook CLI";

        public const string WarningGlyph = "⚠";
        public const string SuccessGlyph = "✓";
        public const string ErrorGlyph = "✗";

        [Obsolete("Don't use")]
        public static ICliDisplay PrintAppInfo(this ICliDisplay cliDisplay)
        {
            cliDisplay.Console.PrintAppInfo();

            return cliDisplay;
        }

        public static IAnsiConsole WriteWarning(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold yellow]{WarningGlyph} {message.Trim()}[/]");

            return console;
        }

        public static IAnsiConsole PrintProblem(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold red]{ErrorGlyph} {message.Trim()}[/]");

            return console;
        }

        public static IAnsiConsole PrintSuccess(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold green]{SuccessGlyph} {message.Trim()}[/]");

            return console;
        }

        public static IAnsiConsole PrintAppInfo(this IAnsiConsole console)
        {
            var a = Assembly.GetExecutingAssembly();

            string appVersion = a.GetAssemblyVersionInformation();

            console.MarkupLine($"[bold white]{a.GetName().Name} v{appVersion}[/]");

            return console;
        }
    }
}
