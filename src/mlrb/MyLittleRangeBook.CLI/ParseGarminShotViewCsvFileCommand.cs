using ConsoleAppFramework;
using Fisher;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;

namespace MyLittleRangeBook
{
    public record struct GarminShotViewFileLoaded(string FirearmName, string FileName, string Contents, DateTimeOffset OccurredUtc);
    [RegisterCommands("garmin")]
    public class ParseGarminShotViewCsvFileCommand
    {
        readonly ICliDisplay      _cliDisplay;
        readonly IDocumentSession _session;
        readonly IFirearmsService _firearmsService;

        public ParseGarminShotViewCsvFileCommand(ICliDisplay cliDisplay, IDocumentSession session, IFirearmsService firearmsService)
        {
            _cliDisplay           = cliDisplay;
            _session              = session;
            _firearmsService = firearmsService;
        }

        /// <summary>
        /// Adds a Garmin ShotView CSV file. Capture the CSV file, and then covert it to a document.
        /// </summary>
        /// <param name="firearm"></param>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("add-shotview")]
        public async Task<int> AddShotViewFile(string firearm, string file, CancellationToken cancellationToken = default)
        {
            _cliDisplay.PrintCommandHeader("Import Garmin ShotView CSV file.");
            int    returnCode   = -1;
            if (!File.Exists(file))
            {
                _cliDisplay.PrintFailure($"File {file} not found.");
                returnCode= ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
                goto Exit;
            }

            var rFirearmId = await _firearmsService.FetchStreamIdForFirearm(firearm, cancellationToken);
            if (rFirearmId.IsFailed)
            {
                _cliDisplay.PrintFailure($"Firearm {firearm} not found.");
                returnCode = ReturnCodes.SHOTVIEW_FILE_READ_FAILURE;
                goto Exit;
            }

            string fileContents = string.Empty;
            try
            {
                fileContents = await File.ReadAllTextAsync(file, cancellationToken);
            }
            catch (Exception ex)
            {
                _cliDisplay.PrintFailure($"Error reading file {file}: {ex.Message}");
                returnCode =  ReturnCodes.SHOTVIEW_FILE_READ_FAILURE;
                goto Exit;
            }

            List<object> events = [];
            var e1 = new GarminShotViewFileLoaded(firearm, file, fileContents, DateTimeOffset.UtcNow);
            events.Add(e1);

            _session.Events.Append(rFirearmId.Value, events);
            await _session.SaveChangesAsync(cancellationToken);

            returnCode = ReturnCodes.SUCCESS;

Exit:
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