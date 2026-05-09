using MyLittleRangeBook.IO;
using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
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

        [Obsolete]
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
                    await action(cancellationToken).ConfigureAwait(false);
                })
                .ConfigureAwait(false);
        }

        [Obsolete]
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

                    return await action(cancellationToken).ConfigureAwait(false);
                })
                .ConfigureAwait(false);
        }
    }
}
