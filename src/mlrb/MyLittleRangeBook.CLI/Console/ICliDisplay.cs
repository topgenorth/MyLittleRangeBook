using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    public interface ICliDisplay
    {
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

        string AppVersion { get; }
        IAnsiConsole Console { get; }
    }
}
