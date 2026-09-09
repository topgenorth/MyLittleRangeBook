using ConsoleAppFramework;
using Fisher;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;

namespace MyLittleRangeBook
{
    /// <summary>
    ///     THis will associate a ShotView file with
    /// </summary>
    [RegisterCommands("firearms")]
    [UsedImplicitly]
    public class AddGarminShotViewFileToFirearm
    {
        readonly ICliDisplay      _cliDisplay;
        readonly IFirearmsService _firearmsService;
        readonly ILogger          _logger;


        public AddGarminShotViewFileToFirearm(ILogger logger, ICliDisplay cliDisplay, IFirearmsService firearmsService)
        {
            _logger          = logger;
            _cliDisplay      = cliDisplay;
            _firearmsService = firearmsService;
        }

        /// <summary>
        ///     Append a Garmin ShotView CSV file to the named firearm.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("add-shotview-file")]
        [UsedImplicitly]
        public async Task<int> AddGarminShotViewCSV(string            name,
                                                    string            file,
                                                    CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.Warning("Firearm name is null or empty.");
                _cliDisplay.PrintFailure("Firearm name must be provided.");
                return ReturnCodes.FAILURE;
            }

            if (string.IsNullOrWhiteSpace(file))
            {
                _logger.Warning("File path is null or empty.");
                _cliDisplay.PrintFailure("File path must be provided.");
                return ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
            }

            if (!File.Exists(file))
            {
                _logger.Warning("File does not exist: {File}", file);
                _cliDisplay.PrintFailure($"File does not exist: {file}");
                return ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
            }


            try
            {
                string fileContent = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
                Result r           = await _firearmsService.AddGarminShotviewCsv(name, fileContent, cancellationToken);
                if (r.IsSuccess)
                {
                    _logger.Information("Add the Shotview file {File} to the {Firearm}", file, name);
                    _cliDisplay.PrintSuccess($"Updated the firearm {name}   with the ShotView file {file}.");
                    return ReturnCodes.SUCCESS;
                }

                _logger.Error("Failed to add '{File}' to {Firearm}", file, name);
                _cliDisplay.PrintFailure($"Failed to add {file} to {name}.");
                return ReturnCodes.FAILURE;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to read file: {File}", file);
                _cliDisplay.PrintFailure($"Failed to read file: {ex.Message}");
                return ReturnCodes.SHOTVIEW_FILE_READ_FAILURE;
            }
        }
    }
}