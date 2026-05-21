using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     The simplest way to import - copy the file into the asset directory.
    /// </summary>
    [RegisterCommands("range-assets")]
    [UsedImplicitly]
    public class ImportRangeAssetFile : MlrbCommandBase
    {
        readonly CopyFileToRangeAsset _assetCopier;
        public ImportRangeAssetFile(ILogger logger, ICliDisplay cliDisplay, IConfiguration configuration) : base(logger,
            cliDisplay)
        {
            _assetCopier = new CopyFileToRangeAsset(configuration);
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
            CliDisplay.PrintCommandHeader("Add file as range asset");
            if (!File.Exists(file))
            {
                CliDisplay.PrintFailure("File does not exit.");

                return ReturnCodes.FIT_FILE_NOT_FOUND;
            }

            if (string.IsNullOrWhiteSpace(rangeEventId))
            {
                rangeEventId = MlrbId.Empty.ToString();
                Logger.Warning("No RangeEvent specified.");
                CliDisplay.PrintWarning($"The file {file} is not assigned to any RangeEvent.");
            }

            var rfe = new RangeEventAssetFile(file, rangeEventId);
            Result<string> copiedFile = await _assetCopier.CopyFileAsync(rfe).ConfigureAwait(false);

            if (copiedFile.IsFailed)
            {
                Logger.Warning("Could not copy the file over.");
                CliDisplay.PrintFailure("Could not copy the file.");

                return ReturnCodes.FAILURE;
            }

            Logger.Verbose($"Copied file '{file}' to '{copiedFile.Value}'");
            if (rangeEventId.Equals(MlrbId.Empty.ToString()))             {
                CliDisplay.PrintSuccess("Copied file to generic range event asset.");
            }
            else
            {
                CliDisplay.PrintSuccess($"Copied file for range event '{rangeEventId}'.");
            }

            return ReturnCodes.SUCCESS;
        }
    }
}
