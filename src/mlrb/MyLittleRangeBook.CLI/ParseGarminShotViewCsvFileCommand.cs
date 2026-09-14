using ConsoleAppFramework;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook
{
    [RegisterCommands("garmin")]
    public class ParseGarminShotViewCsvFileCommand
    {
        readonly ICliDisplay _cliDisplay;

        [Command("parse-shotview")]
        public async Task<int> ParseCsvFile(string fileName, CancellationToken cancellationToken = default)
        {
            _cliDisplay.PrintCommandHeader("Parse Garmin ShotView CSV file.");
            if (!File.Exists(fileName))
            {
                _cliDisplay.PrintFailure($"File {fileName} not found.");
                return ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
            }

            string fileContents = string.Empty;

            try
            {
                fileContents = await File.ReadAllTextAsync(fileName, cancellationToken);
            }
            catch (Exception ex)
            {
                _cliDisplay.PrintFailure($"Error reading file {fileName}: {ex.Message}");
                return ReturnCodes.SHOTVIEW_FILE_READ_FAILURE;
            }

            int    returnCode   = -1;

            if (returnCode == ReturnCodes.SUCCESS)
            {
                _cliDisplay.PrintSuccess("Parsed the file.");
            }
            return returnCode;
        }

    }
}