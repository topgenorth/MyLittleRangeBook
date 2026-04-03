using CommunityToolkit.HighPerformance;
using ConsoleAppFramework;
using MySimpleRangeLog.CLI.Model;
using Spectre.Console;

namespace MySimpleRangeLog.CLI
{
    [RegisterCommands("console")]
    public class DisplayXeroFitToConsoleCommand
    {
        readonly IAnsiConsole _console;

        readonly ILogger _logger;
        readonly XeroShotSessionParser _xeroParser;

        public DisplayXeroFitToConsoleCommand(ILogger logger, IAnsiConsole? console)
        {
            _logger = logger;
            _xeroParser = new XeroShotSessionParser(logger);
            _console = console ?? AnsiConsole.Console;
        }


        /// <summary>
        /// Displays the FIT file to the console.
        /// </summary>
        /// <param name="file">Path to the FIT file</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("")]
        public async Task<int> ToConsoleAsync(string file, CancellationToken ct)
        {
            if (!File.Exists(file))
            {
                _logger.Warning("File {file} not found.", file);
                _console.MarkupLineInterpolated($"[bold red]✗ Error:[/] Could not find '{file}'.");

                return ReturnCodes.FILE_NOT_FOUND;
            }

            var result = (await file.LoadAsync(ct))
                .Bind(bytesFromFitFile =>
                {
                    _logger.Verbose("Loaded {bytes} bytes.", bytesFromFitFile.Length);
                    using var stream = bytesFromFitFile.AsStream();

                    return _xeroParser.Decode(stream);
                });

            if (result.IsFailed)
            {
                _console.MarkupLineInterpolated($"[bold red]✗ Error:[/] Failed to parse FIT file '{file}'");
                _logger.Error("Failed to process FIT file {file}.", file);

                return result.HasError<UnexpectedFitFileTypeError>() ? ReturnCodes.FAILED_TO_PARSE : ReturnCodes.FAILED_TO_LOAD;
            }

            var shotSession = result.Value;
            shotSession.FileName = file;

            DisplaySessionToConsole(_console, shotSession);

            return ReturnCodes.SUCCESS;
        }

        static void DisplayFileProcessingPath(IAnsiConsole console, string file)
        {
            var path = new TextPath(file).RootColor(Color.Red)
                .StemColor(Color.White)
                .LeafColor(Color.Blue);

            console.Write("Processing file ");
            console.Write(path);
        }

        void DisplaySessionToConsole(IAnsiConsole console, ShotSession session)
        {
            console.MarkupLine("[green]✓ Parsed FIT file.[/]");
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

            console.Write(table);
        }
    }
}
