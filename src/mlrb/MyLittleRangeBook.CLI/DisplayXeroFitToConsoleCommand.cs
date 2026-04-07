using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.FIT.Model;
using Spectre.Console;
using static MyLittleRangeBook.CLI.ReturnCodes;

namespace MyLittleRangeBook.CLI
{
    [RegisterCommands("display")]
    public class DisplayXeroFitToConsoleCommand
    {
        readonly ICliDisplay _cliDisplay;
        readonly ILogger _logger;
        readonly IXeroShotSessionParser _xeroParser;

        public DisplayXeroFitToConsoleCommand(ILogger logger, ICliDisplay cliDisplay, IXeroShotSessionParser xeroParser)
        {
            _logger = logger;
            _xeroParser = xeroParser;
            _cliDisplay = cliDisplay;
            _xeroParser = xeroParser;
        }


        /// <summary>
        ///     Displays the FIT file to the console.
        /// </summary>
        /// <param name="file">Path to the FIT file</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("console")]
        [UsedImplicitly]
        public async Task<int> ToConsoleAsync(string file, CancellationToken cancellationToken)
        {
            _cliDisplay.WriteHeader("Displaying FIT File");
            if (!File.Exists(file))
            {
                _logger.Warning("File {file} not found.", file);
                _cliDisplay.WriteFailure($"Could not find '{file}'.");

                return DATABASE_FILE_NOT_FOUND;
            }

            var result = await _cliDisplay.RunStatusAsync("Loading FIT file...",
                async ct =>
                {
                    var result = await _xeroParser.DecodeFITFileAsync(file, ct);

                    if (result.IsFailed)
                    {
                        _logger.Error("Failed to process FIT file {file}.", file);

                        return Result.Fail<ShotSession>(result.Errors);
                    }

                    var shotSession = result.Value;
                    shotSession.FileName = file;

                    return Result.Ok(shotSession);
                },
                cancellationToken
            );

            if (result.IsFailed)
            {
                _cliDisplay.WriteFailure("Failed to parse FIT file.");

                return result.HasError<UnsupportedFitFileTypeError>() ? FAILED_TO_PARSE : FAILED_TO_LOAD;
            }

            var session = result.Value;
            DisplaySessionToConsole(_cliDisplay, session);
            _cliDisplay.WriteSuccess("FIT file loaded.");

            return SUCCESS;
        }

        void DisplaySessionToConsole(ICliDisplay cliDisplay, ShotSession session)
        {
            var title = new TableTitle("Session Stats").SetStyle(Style.Parse("bold"));
            var captionStyle = Style.Parse("italic").Foreground(Color.White);

            var table = new Table()
                .Title(title)
                .Caption($"File: {Path.GetFileName(session.FileName)}", captionStyle)
                .Border(TableBorder.DoubleEdge);

            table.AddColumn("Stat");
            table.AddColumn("Speed (fps)");

            table.AddRow("Avg Velocity", session.AverageSpeed.ToFps().ToString());
            table.AddRow("Max Velocity", session.MaxSpeed.ToFps().ToString());
            table.AddRow("Min Velocity", session.MinSpeed.ToFps().ToString());
            table.AddRow("S/D", session.StandardDeviation.ToFps().ToString("F1"));
            table.AddRow("ES", session.ExtremeSpread.ToFps().ToString());

            cliDisplay.Console.Write(table);
            cliDisplay.Console.WriteLine();
        }
    }
}
