using Spectre.Console;

namespace MyLittleRangeBook.Cli.Console
{
    public interface ICliDisplay
    {
        IAnsiConsole Console { get; }
        void WriteHeader(string action);
        void WriteSuccess(string message);
        void WriteFailure(string message);

        Task RunStatusAsync(
            string status,
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default);

        Task<T> RunStatusAsync<T>(
            string status,
            Func<CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default);

    }
}
