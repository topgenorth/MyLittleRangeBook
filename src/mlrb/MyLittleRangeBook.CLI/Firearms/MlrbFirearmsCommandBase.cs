using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;

namespace MyLittleRangeBook
{
    public abstract class MlrbFirearmsCommandBase
    {
        protected MlrbFirearmsCommandBase(ILogger          logger,
                                          ICliDisplay      display,
                                          IFirearmsService firearmsService)
        {
            ArgumentNullException.ThrowIfNull(firearmsService);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(display);
            Logger          = logger;
            FirearmsService = firearmsService;
            CliDisplay      = display;
        }

        protected ILogger          Logger          { get; private set; }
        protected IFirearmsService FirearmsService { get; private set; }
        protected ICliDisplay      CliDisplay      { get; private set; }
    }
}