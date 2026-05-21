namespace MyLittleRangeBook.Console
{
    public class SimpleAppHeaderWithLogging : ICommandHeaderPrinter
    {
        readonly SimpleAppHeader _inner;
        readonly ILogger _logger;
        string? _action;

        public SimpleAppHeaderWithLogging(SimpleAppHeader inner, ILogger logger)
        {
            _inner = inner;
            _logger = logger;
        }

        /// <summary>
        ///     Print the app header to the console, and log the action that is being performed. Resets the action when done.
        /// </summary>
        /// <param name="console"></param>
        public void Print(IAnsiConsole console)
        {
            if (!string.IsNullOrWhiteSpace(_action))
            {
                _logger.Information(_action!);
            }

            _inner.Print(console);
            _action = null;
            _inner.SetAction(null);
        }

        public ICommandHeaderPrinter SetAction(string? action)
        {
            _action = action;
            _inner.SetAction(action);

            return this;
        }
    }
}
