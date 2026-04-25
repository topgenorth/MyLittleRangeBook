using System.Reflection;
using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    public sealed class CliDisplay : ICliDisplay
    {
        public const string AppName = "MyLittleRangeBook CLI";

        public const string WarningGlyph = "⚠";
        public const string SuccessGlyph = "✔";
        public const string ErrorGlyph = "❌";
        readonly string _appName;
        public string AppVersion { get; }


        public CliDisplay(IAnsiConsole console)
        {
            _appName = AppName;
            AppVersion = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";
            Console = console;
        }

        public IAnsiConsole Console { get; }

        public void WriteHeader(string action)
        {
            OriginalAppHeaderPrinter x = new OriginalAppHeaderPrinter()
                .SetAction(action)
                .SetAppVersion(AppVersion);
            x.Print(Console);
            Console.WriteLine();
        }

        public void WriteSuccess(string message)
        {
            Console.WriteLine();

            Rule rule = new Rule("[green]Completed[/]")
                .RuleStyle("green")
                .LeftJustified();

            Console.Write(rule);
            Console.MarkupLine($"[green]{SuccessGlyph} {Markup.Escape(message)}[/]");
            Console.WriteLine();
        }

        public void WriteFailure(string message)
        {
            Console.WriteLine();

            Rule rule = new Rule("[red]Failed[/]")
                .RuleStyle("red")
                .LeftJustified();

            Console.Write(rule);
            Console.MarkupLine($"[red]{ErrorGlyph} {Markup.Escape(message)}[/]");
            Console.WriteLine();
        }

        public async Task RunStatusAsync(
            string status,
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            await Console.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(status, async _ =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await action(cancellationToken);
                });
        }

        public async Task<T> RunStatusAsync<T>(
            string status,
            Func<CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            return await Console.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(status, async _ =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    return await action(cancellationToken);
                });
        }
    }
}
