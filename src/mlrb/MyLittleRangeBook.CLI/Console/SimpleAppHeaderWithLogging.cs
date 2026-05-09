using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    public class SimpleAppHeaderWithLogging : SimpleAppHeader
    {
        ILogger _logger;

        public SimpleAppHeaderWithLogging(ILogger logger)
        {
            _logger = logger;
        }

        public override void Print(IAnsiConsole console)
        {
            if (!string.IsNullOrWhiteSpace(Action))
            {
                _logger.Information(Action!);
            }

            base.Print(console);
        }
    }
}