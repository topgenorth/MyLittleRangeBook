using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.Console;
using Serilog;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     The simplest way to import - copy the file into the asset directory for the range event.
    /// </summary>
    [RegisterCommands("range-assets")]
    [UsedImplicitly]
    public class ImportFileCommand : MlrbCommandBase
    {
        readonly string _assetsDirectory;

        public ImportFileCommand(ILogger logger, ICliDisplay cliDisplay, IConfiguration configuration) : base(logger, cliDisplay)
        {
            _assetsDirectory = configuration.GetRangeAssetDirectory();
        }

        /// <summary>
        ///     Copy the file to the asset directory for the range event.
        /// </summary>
        /// <param name="rangeEventId"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [Command("add")]
        [UsedImplicitly]
        public async Task<int> CopyFileToAssetDirectory(string rangeEventId, string filePath)
        {
            CliDisplay.PrintCommandHeader(
                $"Copying file '{filePath}' to asset directory for range event '{rangeEventId}'");
            if (string.IsNullOrWhiteSpace(rangeEventId))
            {
                CliDisplay.PrintFailure("Must provide a range event Id");

                return ReturnCodes.FAILURE;
            }
            if (!File.Exists(filePath))
            {
                CliDisplay.PrintFailure("File does not exit.");

                return ReturnCodes.FIT_FILE_NOT_FOUND;
            }

            string rangeEventAssetDir = Path.Combine(_assetsDirectory, rangeEventId);

            string NameFileFit(string rangeEventTargetDir, string assetFileName)
            {
                string targetFileName = Path.GetFileName(filePath);
                Directory.CreateDirectory(rangeEventAssetDir);

                return Path.Combine(rangeEventTargetDir, targetFileName);
            }

            RangeEventAssetFile rfe = new RangeEventAssetFile(rangeEventId, filePath);
            Result<string> copiedFile = rfe.CopyToRangeEvent(NameFileFit);

            if (copiedFile.IsFailed)
            {
                Logger.Warning("Could not copy the file over.");
                CliDisplay.PrintFailure("Could not copy the file.");

                return ReturnCodes.FAILURE;
            }

            Logger.Verbose($"Copied file '{filePath}' to range event '{copiedFile.Value}'");
            CliDisplay.PrintSuccess("Copied file to range event.");

            return ReturnCodes.SUCCESS;
        }
    }
}
