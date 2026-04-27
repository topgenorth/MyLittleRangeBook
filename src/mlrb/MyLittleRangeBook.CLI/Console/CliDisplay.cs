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
        public string AppVersion { get; }


        public CliDisplay(IAnsiConsole console)
        {
            AppVersion = FileExtensions.SimpleAssemblyVersionInformation();
            Console = console;
        }

        public IAnsiConsole Console { get; }


        public void WriteAppHeader(string action)
        {
            // OriginalAppHeaderPrinter x = new OriginalAppHeaderPrinter()
            //     .SetAction(action)
            //     .SetAppVersion(AppVersion);
            SimpleAppHeader x = new SimpleAppHeader();
            x.Print(Console);
        }

        public void WriteSuccess(string message)
        {
            Console.WriteSuccess(message);
        }

        public void WriteFailure(string message)
        {
            Console.WriteProblem(message);
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
