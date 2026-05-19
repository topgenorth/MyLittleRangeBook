using MyLittleRangeBook.Console;

namespace MyLittleRangeBook
{
    /// <summary>
    /// Base class for classes that will handle commands for the Console Application Framework.
    /// </summary>
    public abstract class MlrbCommandBase
    {
        protected ICliDisplay CliDisplay;
        protected ILogger Logger;

        protected MlrbCommandBase(ILogger logger, ICliDisplay cliDisplay)
        {
            Logger = logger;
            CliDisplay = cliDisplay;
        }
    }
}
