using System.Reflection;
using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    public static class AnsiConsoleExtensions
    {
        public static ICliDisplay WriteAppInfo(this ICliDisplay cliDisplay)
        {

            cliDisplay.Console.WriteAppInfo();
            return cliDisplay;
        }

        public static IAnsiConsole WriteWarning(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold yellow]⚠ {message}[/]");

            return console;
        }
        public static IAnsiConsole WriteProblem(this IAnsiConsole console, string message)
        {
            console.MarkupLineInterpolated($"[bold red]✗ {message}[/]");

            return console;
        }
        public static IAnsiConsole WriteSuccess(this IAnsiConsole console, string message)
        {
            console.MarkupLine("[bold green]✓ " + message +"[/]");
            return console;
        }
        public static IAnsiConsole WriteAppInfo(this IAnsiConsole console)
        {
            var a = Assembly.GetExecutingAssembly();

            string appVersion = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";


            console.MarkupLine($"[bold white]{a.GetName().Name} v{appVersion}[/]");

            return console;
        }
    }
}
