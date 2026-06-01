using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.RangeEventAssets;

namespace MyLittleRangeBook.MlrbAssets
{
    /// <summary>
    ///     The simplest way to import - copy the file into the asset directory.
    /// </summary>
    [RegisterCommands("assets")]
    [UsedImplicitly]
    public class MlrbAssetImportCommand : MlrbCommandBase
    {
        readonly IMlrbAssetAggregateRepository _aggregateRepo;
        readonly IPipeline<MlrbAssetFile> _assetPipeline;

        public MlrbAssetImportCommand(ILogger logger,
            ICliDisplay cliDisplay,
            IPipeline<MlrbAssetFile> assetPipeline,
            IMlrbAssetAggregateRepository aggregateRepo) : base(logger,
            cliDisplay)
        {
            _assetPipeline = assetPipeline;
            _aggregateRepo = aggregateRepo;
        }


        /// <summary>
        ///     Copy the file to the asset directory for the range event.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="rangeEventId">The ID of a range event to associate the asset with.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("import")]
        [UsedImplicitly]
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task<int> ImportRangeAssetFile(string file,
            string? rangeEventId = null,
            CancellationToken ct = default)
        {
            CliDisplay.PrintCommandHeader("Import a file as a range asset.");
            if (string.IsNullOrWhiteSpace(rangeEventId))
            {
                rangeEventId = MlrbId.Empty.ToString();
                Logger.Verbose("No RangeEvent specified.");
                CliDisplay.PrintWarning($"The file {file} is not assigned to any RangeEvent.");
            }

            var fileInfo = new FileInfo(file);
            Result<MlrbAssetAggregate?> r = await _aggregateRepo.GetAsync(fileInfo, ct).ConfigureAwait(false);
            bool isNew = r.IsFailed || r.Value is null;

            MlrbAssetAggregate aggregate;
            if (isNew)
            {
                aggregate = MlrbAssetAggregate.New(file, DateTimeOffset.UtcNow);
                Logger.Verbose("Import a new range asset from  {file}", file);
            }
            else
            {
                aggregate = r.Value!;
                Logger.Verbose("Update an existing range asset {id} from  {file}, v{version}.",
                    file,
                    aggregate.Id,
                    aggregate.Version);;
            }

            MlrbAssetFile assetFile = new MlrbAssetFile(file, aggregate, rangeEventId);

            // TODO [TO20260527] Have to update the pipeline to create the events rather than doing things.
            Result result = await _assetPipeline.ExecuteAsync(assetFile, ct).ConfigureAwait(false);

            if (result.IsFailed)
            {
                CliDisplay.PrintFailure("Could not process the file.");

                return ReturnCodes.FAILURE;
            }

            if (!assetFile.RangeEventId.Equals(MlrbId.Empty.ToString()))
            {
                assetFile.Aggregate.AddedToRangeEvent(assetFile.RangeEventId, DateTimeOffset.UtcNow);
                Logger.Verbose("Associated range asset with range event '{RangeEventId}'.", rangeEventId);
            }

            // TODO [TO20260527] Need to create a projector that will update the read-model from the events.
            Result saveAggregate = await _aggregateRepo.SaveAsync(assetFile.Aggregate, ct).ConfigureAwait(false);

            if (saveAggregate.IsFailed)
            {
                IError? err = saveAggregate.Errors[0];
                CliDisplay.PrintFailure($"Could not save the range event asset {err.Message}.");

                if (err.Reasons[0] is ExceptionalError ex)
                {
                    Logger.Warning(ex.Exception, "There was an issue saving the event stream: {message}.", err.Message);
                }
                else
                {
                    Logger.Warning("There was an issue saving the event stream: {message}", err.Message);
                }

                return ReturnCodes.FAILURE;
            }

            Logger.Verbose("Updated the event stream {id}, v{version}", aggregate.Id, aggregate.Version);

            CliDisplay.PrintSuccess(rangeEventId.Equals(MlrbId.Empty.ToString())
                ? "Copied file to generic range event asset."
                : $"Copied file for range event '{rangeEventId}'.");

            return ReturnCodes.SUCCESS;
        }
    }
}
