using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    public interface ICliDisplay
    {
        string AppVersion { get; }
        IAnsiConsole Console { get; }

        /// <summary>
        ///     This prints a header to the console when the command starts up.
        /// </summary>
        /// <param name="action"></param>
        void PrintCommandHeader(string? action = null);

        /// <summary>
        /// Prints a one line success message to the console.
        /// </summary>
        /// <param name="message"></param>
        void PrintSuccess(string message);

        /// <summary>
        /// Prints a one line failure message to the console.
        /// </summary>
        /// <param name="message"></param>
        void PrintFailure(string message);

        [Obsolete]
        Task<T> RunStatusAsync<T>(
            string status,
            Func<CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default);
    }
}
