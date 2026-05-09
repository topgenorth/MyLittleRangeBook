using MyLittleRangeBook.CLI.Console;

namespace MyLittleRangeBook.CLI
{
    public class MlrbCommandBase
    {
        protected ICliDisplay CliDisplay;
        protected ILogger Logger;

        public MlrbCommandBase(ILogger logger, ICliDisplay cliDisplay)
        {
            Logger = logger;
            CliDisplay = cliDisplay;
        }
    }
}
