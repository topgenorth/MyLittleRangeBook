using System.Reflection;
using MyLittleRangeBook.IO;

namespace MyLittleRangeBook.Console
{
    public static class AnsiConsoleExtensions
    {
        // [TO20260503] This might be a better way than using ICliDisplay?
        public const string AppName = "MyLittleRangeBook CLI";

        public const string WARNING_GLYPH = "⚠";
        const        string SUCCESS_GLYPH = "✓";
        public const string ERROR_GLYPH    = "✗";
        public const string BulletGlyph   = "•";


        public static IAnsiConsole PrintWarning(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold yellow]{WARNING_GLYPH} {message.Trim()}[/]");

            return console;
        }

        public static IAnsiConsole PrintProblem(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold red]{ERROR_GLYPH} {message.Trim()}[/]");

            return console;
        }

        public static IAnsiConsole PrintSuccess(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold green]{SUCCESS_GLYPH} {message.Trim()}[/]");

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
