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
        readonly IPipeline<RangeEventAssetFile> _assetPipeline;
        public ImportRangeAssetFile(ILogger logger, ICliDisplay cliDisplay, IPipeline<RangeEventAssetFile> assetPipeline) : base(logger,
            cliDisplay)
        {
            _assetPipeline = assetPipeline;
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
        public async Task<int> CopyFileToAssetDirectory(string file, string? rangeEventId = null, CancellationToken ct = default)
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
            Result result = await _assetPipeline.ExecuteAsync(rfe, ct).ConfigureAwait(false);

            if (result.IsFailed)
            {
                CliDisplay.PrintFailure("Could not process the file.");

                return ReturnCodes.FAILURE;
            }

            CliDisplay.PrintSuccess(rangeEventId.Equals(MlrbId.Empty.ToString())
                ? "Copied file to generic range event asset."
                : $"Copied file for range event '{rangeEventId}'.");

            return ReturnCodes.SUCCESS;
        }
    }
}
