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
        public static IAnsiConsole WriteAppInfo(this IAnsiConsole console)
        {
            var a = Assembly.GetExecutingAssembly();

            string appVersion = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";


            console.MarkupLine($"[bold]{a.GetName().Name}[/] v{appVersion}");

            return console;
        }
    }
}
