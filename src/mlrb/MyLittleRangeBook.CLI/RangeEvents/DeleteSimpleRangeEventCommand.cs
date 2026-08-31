using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using static MyLittleRangeBook.ReturnCodes;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     This class will delete simple range events from the document store.
    /// </summary>
    [RegisterCommands("rangeevent")]
    [UsedImplicitly]
    public sealed class DeleteSimpleRangeEventCommand
    {
        readonly ICliDisplay              _cliDisplay;
        readonly ILogger                  _logger;
        readonly ISimpleRangeEventService _simpleRangeEventService;

        public DeleteSimpleRangeEventCommand(ISimpleRangeEventService simpleRangeEventService, ILogger logger,
                                             ICliDisplay              cliDisplay)
        {
            _simpleRangeEventService = simpleRangeEventService;
            _logger                  = logger;
            _cliDisplay              = cliDisplay;
        }

        /// <summary>
        ///     Delete a range event from the database by ID.
        /// </summary>
        /// <param name="id">The ID of the range event to delete.</param>
        /// <param name="quiet">If set to true, then less verbose output.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("delete")]
        [UsedImplicitly]
        public async Task<int> DeleteRangeEvent(string id, bool quiet = false, CancellationToken ct = default)
        {
            if (!quiet)
            {
                _cliDisplay.PrintCommandHeader($"Delete range event {id}");
            }

            int returnCode;
            if (!Guid.TryParse(id, out Guid eventId))
            {
                // TODO [TO20260830] specialized error code?
                returnCode = FAILURE;
                goto ExitCommand;
            }

            try
            {
                // First, retrieve the event to ensure it exists
                Result<SimpleRangeEvent> getResult =
                    await _simpleRangeEventService.GetAsync(Guid.Parse(id), ct).ConfigureAwait(false);

                if (getResult.IsFailed)
                {
                    _logger.Warning("Could not find simple range event {id} for deletion.", id);
                    _cliDisplay.PrintFailure("Could not find the requested range event.");
                    returnCode = FAILURE;
                }
                else
                {
                    // Delete the event
                    Result<bool> deleteResult = await _simpleRangeEventService.DeleteAsync(getResult.Value, ct)
                                                                              .ConfigureAwait(false);

                    if (deleteResult.IsSuccess)
                    {
                        _cliDisplay.PrintSuccess($"Range event {id} deleted successfully.");
                        returnCode = SUCCESS;
                    }
                    else
                    {
                        _logger.Warning("Failed to delete simple range event {id}.", id);
                        _cliDisplay.PrintFailure("Failed to delete the range event.");
                        returnCode = FAILURE;
                    }
                }
            }
            catch (Exception e)
            {
                returnCode = FAILURE;
                _logger.Error(e, e.Message);
                _cliDisplay.PrintFailure("An error occurred while deleting the range event.");
            }

            ExitCommand:
            System.Console.ReadKey();

            return returnCode;
        }
    }
}