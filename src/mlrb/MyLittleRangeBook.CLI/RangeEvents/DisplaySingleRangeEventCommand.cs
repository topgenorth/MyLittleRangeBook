using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook.RangeEvents
{
    [RegisterCommands("rangeevents")]
    public sealed class DisplaySingleRangeEventCommand
    {
        readonly ICliDisplay              _cliDisplay;
        readonly ILogger                  _logger;
        readonly ISimpleRangeSessionService _simpleRangeSessionService;
        public DisplaySingleRangeEventCommand(ICliDisplay cliDisplay, ILogger logger, ISimpleRangeSessionService simpleRangeSessionService)
        {
            _cliDisplay              = cliDisplay;
            _logger                  = logger;
            _simpleRangeSessionService = simpleRangeSessionService;
        }

        /// <summary>
        ///     Display a single range event.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quiet">If set to true, then less verbose, single line output.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("show")]
        [UsedImplicitly]
        public async Task<int> DisplayOneRangeEvent(string id, bool quiet = false, CancellationToken ct = default)
        {
            int  returnCode;

            if (quiet)
            {
                _cliDisplay.PrintCommandHeader();
            }
            else
            {
                _cliDisplay.PrintCommandHeader($"Show range event {id}");
            }

            if (!Guid.TryParse(id, out Guid eventId))
            {
                _cliDisplay.PrintFailure("Invalid range event ID.");
                return ReturnCodes.FAILURE;
            }

            try
            {
                Result<SimpleRangeSession> result =
                    await _simpleRangeSessionService.GetAsync(eventId, ct).ConfigureAwait(false);

                if (result.IsFailed)
                {
                    _logger.Warning("Could not find simple range event {id} for display.", id);
                    _cliDisplay.PrintFailure("Could not find the request range event.");
                    returnCode = ReturnCodes.FAILURE;
                }
                else if (result.Value is null)
                {
                    _cliDisplay.PrintWarning("Simple range event not found.");
                    returnCode = ReturnCodes.FAILURE;
                }
                else
                {
                    SimpleRangeSessionPrinter2 p = new();
                    p.Print(_cliDisplay.Console, result.Value!, quiet);
                    _cliDisplay.PrintSuccess("Range event displayed successfully.");
                    returnCode = ReturnCodes.SUCCESS;
                }
            }
            catch (Exception e)
            {
                returnCode = ReturnCodes.FAILURE;
                _logger.Error(e, e.Message);
                _cliDisplay.PrintFailure("An error occurred while displaying the range event.");
            }

            System.Console.ReadKey();

            return returnCode;
        }

    }
}