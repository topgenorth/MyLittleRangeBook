using ConsoleAppFramework;
using Fisher;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.RangeSessions
{
    [RegisterCommands("rangeevents")]
    public sealed class DisplaySingleRangeEventCommand
    {
        readonly ICliDisplay _cliDisplay;
        readonly ILogger     _logger;

        readonly IDocumentSession _session;

        // readonly ISimpleRangeSessionService _simpleRangeSessionService;
        public DisplaySingleRangeEventCommand(ICliDisplay cliDisplay, ILogger logger, IDocumentSession session)
        {
            _cliDisplay = cliDisplay;
            _logger     = logger;
            _session    = session;
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
            int returnCode;

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
                SimpleRangeSession? sre = await _session.LoadAsync<SimpleRangeSession>(id, ct);
                SimpleRangeSessionPrinter2 p = new();
                p.Print(_cliDisplay.Console, sre!, quiet);
                _cliDisplay.PrintSuccess("Range event displayed successfully.");
                returnCode = ReturnCodes.SUCCESS;
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