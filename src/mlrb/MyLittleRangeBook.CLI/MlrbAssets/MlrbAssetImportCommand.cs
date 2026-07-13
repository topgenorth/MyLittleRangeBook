using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

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
        readonly IPipeline<MlrbAssetFile>      _assetPipeline;
        readonly IXeroCsvShotSessionParser     _csvParser;
        readonly IFirearmAggregateRepository   _faRepo;
        readonly ISqliteHelper                 _sqliteHelper;
        readonly IProjector                    _firearmsProjector;

        public MlrbAssetImportCommand(ILogger                       logger,
                                      ICliDisplay                   cliDisplay,
                                      IPipeline<MlrbAssetFile>      assetPipeline,
                                      IMlrbAssetAggregateRepository aggregateRepo,
                                      ISqliteHelper                 sqliteHelper,
                                      IFirearmAggregateRepository   faRepo,
                                      IXeroCsvShotSessionParser     csvParser,
                                      IProjector firearmsProjector) : base(logger,
                                                                                                                    cliDisplay)
        {
            _assetPipeline          = assetPipeline;
            _aggregateRepo          = aggregateRepo;
            _sqliteHelper           = sqliteHelper;
            _faRepo                 = faRepo;
            _csvParser              = csvParser;
            _firearmsProjector = firearmsProjector;
        }


        /// <summary>
        ///     Copy the file to the asset directory for the range event.
        /// </summary>
        /// <param name="file">The name of the file to import into the MLRB assets.</param>
        /// <param name="firearmName">The name of the firearm to associate with the imported asset.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("import")]
        [UsedImplicitly]
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task<int> ImportRangeAssetFile(string            file,
                                                    string?           firearmName = null,
                                                    CancellationToken ct          = default)
        {
            int                  returnCode;
            FileInfo             fileInfo = new(file);
            DapperCommandContext context = await DapperCommandContext.NewAsync(_sqliteHelper, ct).ConfigureAwait(false);
            if (!fileInfo.Exists)
            {
                CliDisplay.PrintCommandHeader("Copy file into MLRB assets.");
                CliDisplay.PrintFailure($"'{file}' does not exist.");
                returnCode = ReturnCodes.FIT_FILE_NOT_FOUND;
                goto ExitMethod;
            }

            Result<MlrbAssetAggregate?> r     = await _aggregateRepo.GetAsync(context, fileInfo).ConfigureAwait(false);
            bool                        isNew = r.IsFailed || r.Value is null;

            MlrbAssetAggregate aggregate;
            if (isNew)
            {
                CliDisplay.PrintCommandHeader("Copy file into MLRB assets.");
                aggregate = MlrbAssetAggregate.New(file, DateTimeOffset.UtcNow);
            }
            else
            {
                aggregate = r.Value!;
                CliDisplay.PrintCommandHeader($"Copy file and update MLRB asset {aggregate.Id}.");
                // TODO [TO20260602] Maybe we want to have a "refresh" or a different type of "update" event?
            }

            MlrbAssetFile assetFile = new(aggregate);

            // TODO [TO20260527] Have to update the pipeline to create the events rather than doing things.
            Result result = await _assetPipeline.ExecuteAsync(assetFile, ct).ConfigureAwait(false);

            if (result.IsFailed)
            {
                CliDisplay.PrintFailure("Could not process the file.");
                returnCode = ReturnCodes.FAILURE;
                goto ExitMethod;
            }

            if (await _csvParser.IsShotViewCsvAsync(file, ct).ConfigureAwait(false))
            {
                CliDisplay.PrintSuccess("Detected Garmin ShotView CSV file.");
                assetFile.Aggregate.Parsed(FileExtensions.MIME_TYPE_GARMIN_SHOTVIEW_FILE, DateTimeOffset.UtcNow);
            }

            // TODO [TO20260527] Need to create a projector that will update the read-model from the events.
            Result saveAggregate = await _aggregateRepo.SaveAsync(context, assetFile.Aggregate).ConfigureAwait(false);

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

                assetFile.Aggregate.Fail(saveAggregate.Errors[0].Message, DateTimeOffset.UtcNow);
                await _aggregateRepo.SaveAsync(context, assetFile.Aggregate).ConfigureAwait(false);
                returnCode = ReturnCodes.FAILURE;
                goto ExitMethod;
            }

            assetFile.Aggregate.ImportComplete(DateTimeOffset.UtcNow);
            await _aggregateRepo.SaveAsync(context, assetFile.Aggregate).ConfigureAwait(false);
            Logger.Verbose("Updated the event stream {id}, v{version}", aggregate.Id, aggregate.Version);

            if (!string.IsNullOrEmpty(firearmName))
            {
                Result r2 = await AssociateWithFirearm(context, assetFile.Aggregate.Id, firearmName!)
                               .ConfigureAwait(false);
                if (r2.IsSuccess)
                {
                    CliDisplay.PrintInfo($"{file} was associated with {firearmName}.");
                }
                else
                {
                    CliDisplay.PrintWarning($"{file} could not be associated with {firearmName}.");
                }
            }

            CliDisplay.PrintSuccess($"{file} was imported to assets.");
            returnCode = ReturnCodes.SUCCESS;

            ExitMethod:
            if (returnCode != ReturnCodes.SUCCESS)
            {
                context.Transaction?.Rollback();
            }

            PressEnterToContinue();
            return returnCode;
        }

        async Task<Result> AssociateWithFirearm(DapperCommandContext context, MlrbId assetId, string firearmName)
        {
            List<IReason> reasons = [];
            Result<FirearmAggregate> r1 =
                await _faRepo.GetOrCreateByNameAsync(context, firearmName).ConfigureAwait(false);
            reasons.AddRange(r1.Reasons);

            FirearmAggregate? fa = r1.Value;
            fa.AssociatedWithAsset(assetId, DateTimeOffset.UtcNow);
            Result r2 = await _faRepo.UpsertAsync(context, fa).ConfigureAwait(false);
            reasons.AddRange(r2.Reasons);



            return new Result().WithReasons(reasons);
        }
    }
}