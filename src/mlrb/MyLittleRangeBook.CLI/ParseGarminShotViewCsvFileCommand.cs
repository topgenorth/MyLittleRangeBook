using ConsoleAppFramework;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook
{
    [RegisterCommands("garmin")]
    public class ParseGarminShotViewCsvFileCommand
    {
        readonly ICliDisplay _cliDisplay;
        public ParseGarminShotViewCsvFileCommand(ICliDisplay cliDisplay)
        {
            _cliDisplay = cliDisplay;
        }

        [Command("add-shotview")]
        public async Task<int> AddShotViewFile(string file, CancellationToken cancellationToken = default)
        {
            _cliDisplay.PrintCommandHeader("Import Garmin ShotView CSV file.");
            if (!File.Exists(file))
            {
                _cliDisplay.PrintFailure($"File {file} not found.");
                return ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
            }

            string fileContents = string.Empty;

            try
            {
                fileContents = await File.ReadAllTextAsync(file, cancellationToken);
            }
            catch (Exception ex)
            {
                _cliDisplay.PrintFailure($"Error reading file {file}: {ex.Message}");
                return ReturnCodes.SHOTVIEW_FILE_READ_FAILURE;
            }

            int    returnCode   = -1;

            if (returnCode == ReturnCodes.SUCCESS)
            {
                _cliDisplay.PrintSuccess("Parsed the file.");
            }
            else
            {
                _cliDisplay.PrintFailure("Something went wrong.");
            }
            return returnCode;
        }

    }
}