using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    ///     The simplest way to import - copy the file into the asset directory.
    /// </summary>
    [RegisterCommands("range-assets")]
    [UsedImplicitly]
    public class RangeAssetCommands : MlrbCommandBase
    {
        readonly IRangeAssetAggregateRepository _aggregateRepo;
        readonly IPipeline<RangeEventAssetFile> _assetPipeline;

        public RangeAssetCommands(ILogger logger,
            ICliDisplay cliDisplay,
            IPipeline<RangeEventAssetFile> assetPipeline,
            IRangeAssetAggregateRepository aggregateRepo) : base(logger,
            cliDisplay)
        {
            _assetPipeline = assetPipeline;
            _aggregateRepo = aggregateRepo;
        }

        /// <summary>
        ///     Copy the file to the asset directory for the range event.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="rangeEventId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("add")]
        [UsedImplicitly]
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task<int> CopyFileToAssetDirectory(string file,
            string? rangeEventId = null,
            CancellationToken ct = default)
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
                Logger.Verbose("No RangeEvent specified.");
                CliDisplay.PrintWarning($"The file {file} is not assigned to any RangeEvent.");
            }

            var id = MlrbId.FromFile(new FileInfo(file));
            Result<RangeAssetAggregate> aggregate = await _aggregateRepo.GetAsync(id, ct).ConfigureAwait(false);
            RangeEventAssetFile rfe = aggregate.IsSuccess ?
                new RangeEventAssetFile(file, aggregate.Value, rangeEventId) :
                new RangeEventAssetFile(file, rangeEventId);

            Result result = await _assetPipeline.ExecuteAsync(rfe, ct).ConfigureAwait(false);

            if (result.IsFailed)
            {
                CliDisplay.PrintFailure("Could not process the file.");

                return ReturnCodes.FAILURE;
            }

            if (!rfe.RangeEventId.Equals(MlrbId.Empty.ToString()))
            {
                rfe.Aggregate.AddedToRangeEvent(rfe.RangeEventId, DateTimeOffset.UtcNow);
                Logger.Verbose("Associated range asset with range event '{RangeEventId}'.", rangeEventId);
            }

            Result saveAggregate = await _aggregateRepo.SaveAsync(rfe.Aggregate, ct).ConfigureAwait(false);

            if (saveAggregate.IsFailed)
            {
                CliDisplay.PrintFailure("Could not save the range event asset.");
                Logger.Warning("There was an issue saving the event stream.");

                return ReturnCodes.FAILURE;
            }

            CliDisplay.PrintSuccess(rangeEventId.Equals(MlrbId.Empty.ToString())
                ? "Copied file to generic range event asset."
                : $"Copied file for range event '{rangeEventId}'.");

            return ReturnCodes.SUCCESS;
        }
    }
}
