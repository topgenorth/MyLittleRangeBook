using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    public class SimpleAppHeaderWithLogging : ICommandHeaderPrinter
    {
        readonly ICommandHeaderPrinter _inner;
        readonly ILogger _logger;
        string? _action;

        public SimpleAppHeaderWithLogging(ICommandHeaderPrinter inner, ILogger logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public void Print(IAnsiConsole console)
        {
            if (!string.IsNullOrWhiteSpace(_action))
            {
                _logger.Information(_action!);
            }

            _inner.Print(console);
        }

        public ICommandHeaderPrinter SetAction(string? action)
        {
            _action = action;
            _inner.SetAction(action);

            return this;
        }
    }
}
