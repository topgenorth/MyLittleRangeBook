using MyLittleRangeBook.Console;

namespace MyLittleRangeBook
{
    /// <summary>
    ///     Base class for classes that will handle commands for the Console Application Framework.
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

        /// <summary>
        /// Pauses the application and prompts the user to press any key to continue.
        /// This method is only active in debug builds and is primarily intended for use during testing
        /// to prevent the console window from closing too quickly after program execution.
        /// </summary>
        protected void PressAnyKeyToContinue()
        {
#if DEBUG
            // [TO20260507] Need this when testing in Rider.  Without it the console window closes too fast.
            CliDisplay.Console.Write("Press any key to continue...");
            System.Console.ReadKey();
#endif

        }
    }
}
