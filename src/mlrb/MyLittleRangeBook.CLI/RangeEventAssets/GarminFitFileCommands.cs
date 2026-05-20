using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.FIT.Model;
using static MyLittleRangeBook.ReturnCodes;

namespace MyLittleRangeBook.RangeEventAssets
{
    [RegisterCommands("fit")]
    public class GarminFitFileCommands : MlrbCommandBase
    {
        readonly IXeroShotSessionParser _xeroParser;

        public GarminFitFileCommands(ILogger logger,
            ICliDisplay cliDisplay,
            IXeroShotSessionParser xeroParser) : base(
            logger, cliDisplay)
        {
            _xeroParser = xeroParser;
        }


        /// <summary>
        ///     Used to explore the FIT file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("explore")]
        [UsedImplicitly]
        public async Task<int> ExploreAsync(string file, CancellationToken ct)
        {
            CliDisplay.PrintCommandHeader("Explore FIT file");

            Result<ShotSession> result = await ((XeroShotSessionParser)_xeroParser)
                .ExploreFitFileAsync(file, ct)
                .ConfigureAwait(false);
            if (result.IsFailed)
            {
                Logger.Error("Failed to explore FIT file {file}.", file);
                CliDisplay.Console.MarkupLine($"[red]Failed to explore FIT file {file}.[/]");

                return FIT_FILE_PARSE_FAILURE;
            }

            return SUCCESS;
        }

        /// <summary>
        ///     Displays the FIT file to the console.
        /// </summary>
        /// <param name="file">Path to the FIT file</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("console")]
        [UsedImplicitly]
        public async Task<int> ToConsoleAsync(string file, CancellationToken ct)
        {
            CliDisplay.PrintCommandHeader("Displaying FIT File");
            if (!File.Exists(file))
            {
                Logger.Warning("File {file} not found.", file);
                CliDisplay.PrintFailure($"Could not find '{file}'.");

                return FIT_FILE_NOT_FOUND;
            }


            Result<ShotSession> result = await _xeroParser.DecodeFITFileAsync(file, ct).ConfigureAwait(false);

            if (result.IsFailed)
            {
                Logger.Error("Failed to process FIT file {file}.", file);

                return FIT_FILE_PARSE_FAILURE;
            }

            Logger.Debug("Fit file {fitFile}", file);

            ShotSession? shotSession = result.Value;
            shotSession.FileName = file;

            ShotSession? session = result.Value;
            DisplaySessionToConsole(CliDisplay, session);
            CliDisplay.PrintSuccess("FIT file loaded.");

            return SUCCESS;
        }

        void DisplaySessionToConsole(ICliDisplay cliDisplay, ShotSession session)
        {
            // TODO [TO20260509] Refactor this into a IConsolePrinter.
            TableTitle title = new TableTitle("Session Stats").SetStyle(Style.Parse("bold"));
            Style captionStyle = Style.Parse("italic").Foreground(Color.White);

            Table table = new Table()
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
