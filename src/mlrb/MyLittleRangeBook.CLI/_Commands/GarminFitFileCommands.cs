using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.FIT.Model;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Services;
using static MyLittleRangeBook.CLI.ReturnCodes;

namespace MyLittleRangeBook.CLI
{
    [RegisterCommands("fit")]
    [UsedImplicitly]
    public class GarminFitFileImporterCommand : MlrbCommandBase
    {
        readonly IRangeEventAssetImporter _assetImporter;
        readonly ISimpleRangeEventRepository _repo;
        public GarminFitFileImporterCommand(ILogger logger,
            ICliDisplay cliDisplay,
            [FromKeyedServices(SqliteHelperExtensions.DI_KEYS_SQLITE)]ISimpleRangeEventRepository repo,
            IRangeEventAssetImporter assetImporter) : base(logger, cliDisplay)
        {
            _assetImporter = assetImporter;
            _repo = repo;
        }

        /// <summary>
        ///     Will copy the FIT file to the RangeAsset directory and associate it with the SimpleRangeEvent.
        /// </summary>
        /// <param name="rangeEventId"></param>
        /// <param name="fitFile"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("import")]
        [UsedImplicitly]
        public async Task<int> AddFitFileToRangeEvent(string rangeEventId,
            string fitFile,
            CancellationToken ct = default)
        {
            CliDisplay.PrintCommandHeader("Import FIT file");
            Logger.Debug("Adding FIT file {fitFile} to RangeAsset {rangeAssetId}.", fitFile, rangeEventId);

            try
            {
                Result<SimpleRangeEvent> getRangeEvent = await _repo.GetAsync(rangeEventId, ct).ConfigureAwait(false);
                if (getRangeEvent.IsFailed)
                {
                    Logger.Warning("Could not find SimpleRangeEvent {simpleRangeEventId}, nothing imported.", rangeEventId);

                    return FAILURE;
                }

                Result<(MlrbId assetId, string destinationPath)> copyFile = await _assetImporter
                    .ImportAssetForRangeEvent(rangeEventId, fitFile, ct)
                    .ConfigureAwait(false);
                if (copyFile.IsFailed)
                {
                    return FIT_FILE_READ_FAILURE;
                }

                Logger.Debug("Copied the file {fitFile} to {destinationPath}, ID {assetId}", fitFile,
                    copyFile.Value.destinationPath, copyFile.Value.assetId);

                return SUCCESS;
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Something when very wrong trying to import the FIT file.");
                return FAILURE;
            }
        }
    }

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
