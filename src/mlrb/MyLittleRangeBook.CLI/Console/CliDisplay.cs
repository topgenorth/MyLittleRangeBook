using System.Reflection;
using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    public sealed class CliDisplay : ICliDisplay
    {
        const string AppName = "MyLittleRangebook";
        readonly string _appName;
        readonly string _version;

        public const string WarningGlyph = "⚠";
        public const string SuccessGlyph = "✔";
        public const string ErrorGlyph = "❌";


        public CliDisplay(IAnsiConsole console)
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            _appName = AppName;
            _version = assembly.GetName().Version?.ToString(3) ?? "0.0.0";
            Console = console;
        }

        public IAnsiConsole Console { get; }

        public void WriteHeader(string action)
        {
            var grid = new Grid();
            grid.AddColumn();

            grid.AddRow($"[bold]{Markup.Escape(_appName)}[/]");
            grid.AddRow($"[grey]Version:[/] [green]{Markup.Escape(_version)}[/]");
            grid.AddRow($"[grey]Action:[/] [yellow]{Markup.Escape(action)}[/]");

            Panel panel = new Panel(grid).Expand()
                .Header("[bold blue]Starting[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(Color.SteelBlue1))
                .Padding(1, 0, 1, 0);

            Console.Write(panel);
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
