using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Config;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     The simplest way to import - copy the file into the asset directory.
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
        /// <param name="file"></param>
        /// <param name="rangeEventId"></param>
        /// <returns></returns>
        [Command("add")]
        [UsedImplicitly]
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task<int> CopyFileToAssetDirectory(string file, string? rangeEventId = null)
        {
            CliDisplay.PrintCommandHeader("Add asset");
            if (string.IsNullOrWhiteSpace(rangeEventId))
            {
                rangeEventId = MlrbId.Empty.ToString();
                Logger.Warning("No RangeEvent specified.");
                CliDisplay.PrintWarning($"The file {file} is not assigned to any RangeEvent.");
            }
            if (!File.Exists(file))
            {
                CliDisplay.PrintFailure("File does not exit.");

                return ReturnCodes.FIT_FILE_NOT_FOUND;
            }

            string AssetFileDestination(string rangeEventTargetDir, string assetFileName)
            {
                string rangeEventAssetDir = Path.Combine(_assetsDirectory, rangeEventId);
                Directory.CreateDirectory(rangeEventAssetDir);
                string targetFileName = Path.GetFileName(file);
                string x= Path.Combine(rangeEventAssetDir, targetFileName);

                return x;
            }

            RangeEventAssetFile rfe = new RangeEventAssetFile(rangeEventId, file);
            Result<string> copiedFile = rfe.CopyToRangeEvent(AssetFileDestination);

            if (copiedFile.IsFailed)
            {
                Logger.Warning("Could not copy the file over.");
                CliDisplay.PrintFailure("Could not copy the file.");

                return ReturnCodes.FAILURE;
            }

            Logger.Verbose($"Copied file '{file}' to '{copiedFile.Value}'");
            CliDisplay.PrintSuccess("Copied file to range event.");

            return ReturnCodes.SUCCESS;
        }
    }
}
