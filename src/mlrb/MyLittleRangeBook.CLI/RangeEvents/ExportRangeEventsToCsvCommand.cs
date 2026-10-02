using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using static MyLittleRangeBook.ReturnCodes;

namespace MyLittleRangeBook.RangeEvents
{
    [RegisterCommands("rangeevents")]
    public sealed class ExportRangeEventsToCsvCommand
    {
        readonly ICliDisplay              _cliDisplay;
        readonly ILogger                  _logger;
        readonly ISimpleRangeSessionService _simpleRangeSessionService;

        public ExportRangeEventsToCsvCommand(ICliDisplay              cliDisplay,
                                             ILogger                  logger,
                                             ISimpleRangeSessionService simpleRangeSessionService)
        {
            _cliDisplay              = cliDisplay;
            _logger                  = logger;
            _simpleRangeSessionService = simpleRangeSessionService;
        }

        /// <summary>
        ///     Will export simple range events to a CSV file.
        /// </summary>
        /// <param name="file">If omitted, then a temporary file name will be used.</param>
        /// <param name="quiet"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("export-to-csv")]
        [UsedImplicitly]
        public async Task<int> ExportToCsv(string?           file              = null,
                                           bool              quiet             = false,
                                           CancellationToken cancellationToken = default)
        {
            _cliDisplay.PrintCommandHeader("Export range events to CSV.");


            int    returnCode;
            string csvFileName = file ?? Path.GetTempFileName();
            try
            {
                Result r = await _simpleRangeSessionService.ExportToCsv(csvFileName, cancellationToken)
                                                         .ConfigureAwait(false);

                returnCode = r.IsSuccess ? SUCCESS : FAILURE;
            }
            catch (Exception ex)
            {
                _cliDisplay.PrintFailure(ex.Message);
                returnCode = FAILURE;
            }

            if (returnCode == SUCCESS)
            {
                _cliDisplay.PrintSuccess($"Range events exported to CSV successfully {csvFileName}.");
            }
            else
            {
                _cliDisplay.PrintFailure("Failed to export range events to CSV.");
            }

            return returnCode;
        }
    }
}