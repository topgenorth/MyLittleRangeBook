using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.MlrbAssets
{
    /// <summary>
    ///     This command will associate a firearm and/or a simple range event with an asset.
    /// </summary>
    [RegisterCommands("assets")]
    [UsedImplicitly]
    public class AssociateAssetsCommand : MlrbSqliteCommandBase
    {
        readonly IMlrbAssetAggregateRepository _assetAggregateRepository;
        readonly IFirearmAggregateRepository   _firearmAggregateRepository;
        readonly ISimpleRangeEventService      _simpleRangeEventService;

        public AssociateAssetsCommand(ILogger                       logger,
                                      ICliDisplay                   display,
                                      ISqliteHelper                 sqliteHelper,
                                      IFirearmAggregateRepository   firearmAggregateRepository,
                                      IMlrbAssetAggregateRepository assetAggregateRepository,
                                      ISimpleRangeEventService      simpleRangeEventService) :
            base(logger, display, sqliteHelper)
        {
            ArgumentNullException.ThrowIfNull(firearmAggregateRepository);
            ArgumentNullException.ThrowIfNull(assetAggregateRepository);

            _firearmAggregateRepository = firearmAggregateRepository;
            _assetAggregateRepository   = assetAggregateRepository;
            _simpleRangeEventService    = simpleRangeEventService;
        }

        /// <summary>
        ///     Imports a range asset file, associating it with the specified asset, firearm, and/or range event.
        /// </summary>
        /// <param name="assetId">The identifier of the asset to be imported.</param>
        /// <param name="firearmId">The optional identifier of the firearm to associate with the asset.</param>
        /// <param name="simpleRangeEventId">The optional identifier of the range event to associate with the asset.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns an integer indicating the status of the operation.</returns>
        [Command("associate")]
        [UsedImplicitly]
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task<int> ImportRangeAssetFile(string            assetId,
                                                    string?           firearmId          = null,
                                                    string?           simpleRangeEventId = null,
                                                    CancellationToken cancellationToken  = default)
        {
            int returnCode;

            CliDisplay.PrintCommandHeader("Associate asset");
            DapperCommandContext context =
                await DapperCommandContext.NewAsync(SqliteHelper, cancellationToken).ConfigureAwait(false);

            try
            {
                FluentResults.Result<MlrbAssetAggregate?> a = await _assetAggregateRepository
                                                                   .GetAsync(context, assetId)
                                                                   .ConfigureAwait(false);

                if (a.IsFailed || a.Value is null)
                {
                    if (a.Errors.Any())
                    {
                        Logger.Error("Could not load Asset {assetId}: {reason}", assetId, a.Errors[0].Message);
                    }
                    else
                    {
                        Logger.Warning("Could not load Asset {assetId}.", assetId);
                    }

                    CliDisplay.PrintFailure($"Failed to load asset {assetId}.");

                    PressEnterToContinue();

                    return ReturnCodes.FAILURE;
                }

                await AssociateFirearmAsync(context, a.Value, firearmId).ConfigureAwait(false);
                await AssociateSimpleRangeEventAsync(context, a.Value, simpleRangeEventId).ConfigureAwait(false);

                CliDisplay.PrintSuccess("Finished with associations.");
                returnCode = ReturnCodes.SUCCESS;
            }
            catch (Exception e)
            {
                context.Transaction?.Rollback();
                Logger.Error(e, "Failed to make associations.");
                CliDisplay.PrintFailure("Failed to make associations. " + e.Message);
                returnCode = ReturnCodes.FAILURE;
            }

            PressEnterToContinue();

            return returnCode;
        }

        async Task AssociateFirearmAsync(DapperCommandContext context, MlrbAssetAggregate asset, string? firearmId)
        {
            if (firearmId is null)
            {
                return;
            }

            try
            {
                Result<FirearmAggregate?> f = await _firearmAggregateRepository.GetAsync(context, firearmId)
                                                                              .ConfigureAwait(false);
                if (f.IsFailed)
                {
                    Logger.Warning(
                                   "Failed to create the association between {assetId}:{asset} and firearm {firearmId}, {reason}.",
                                   asset.Id, asset.DestinationPath, firearmId, f.Errors[0].Message);

                    return;
                }

                var                  p   = new { FirearmId = firearmId, AssetId = asset.Id.ToString() };
                DapperCommandContext ctx = context with { Arguments = p };
                await Commands.AssociateWithFirearm.ExecuteScalarAsync<long?>(ctx).ConfigureAwait(false);

                DateTimeOffset dto = DateTimeOffset.UtcNow;
                asset.AssociatedWithFirearm(firearmId, dto);
                f.Value!.AssociatedWithAsset(asset.Id, dto);

                await _firearmAggregateRepository.UpsertAsync(context, f.Value).ConfigureAwait(false);
                await _assetAggregateRepository.SaveAsync(context, asset).ConfigureAwait(false);

                CliDisplay.PrintSuccess(
                                        $"Associated firearm {firearmId}:{f.Value.Name} with asset {asset.Id}:{asset.DestinationPath}");
                Logger.Information("Associated firearm {firearmId} with asset {assetId}", firearmId, asset.Id);
            }
            catch (Exception e)
            {
                CliDisplay.PrintFailure("Failed to associate firearm. " + e.Message);
                Logger.Error(e, "Failed to associate the firearm {firearmId} with the asset {assetId}.", firearmId,
                             asset.ToString());
            }
        }

        async Task AssociateSimpleRangeEventAsync(DapperCommandContext context,
                                                  MlrbAssetAggregate   asset,
                                                  string?              simpleRangeEventId)
        {
            try
            {
                if (simpleRangeEventId is null)
                {
                    return;
                }

                Result<SimpleRangeEvent> r = await _simpleRangeEventService
                                                  .GetAsync(context, asset.Id)
                                                  .ConfigureAwait(false);
                if (r.IsFailed)
                {
                    Logger.Warning(
                                   "Failed to create the association between {assetId}:{asset} and firearm {firearmId}, {reason}.",
                                   asset.Id, asset.DestinationPath, simpleRangeEventId, r.Errors[0].Message);

                    return;
                }

                var                  p = new { SimpleRangeEventId = simpleRangeEventId, AssetId = asset.Id.ToString() };
                DapperCommandContext ctx = context with { Arguments = p };

                await Commands.AssociateWithRangeEvent.ExecuteScalarAsync<long?>(ctx).ConfigureAwait(false);

                DateTimeOffset utcNow = DateTimeOffset.UtcNow;
                asset.AssociatedWithSimpleRangeEvent(simpleRangeEventId, utcNow);
                await _assetAggregateRepository.SaveAsync(context, asset).ConfigureAwait(false);

                CliDisplay.PrintSuccess(
                                        $"Associated simple range event {simpleRangeEventId} with asset {asset.Id}:{asset.DestinationPath}");
                Logger.Information("Associated simple range event {simpleRangeEventId} with {assetId}",
                                   simpleRangeEventId, asset.Id);
            }
            catch (Exception e)
            {
                CliDisplay.PrintFailure("Failed to associate simple range event. " + e.Message);
                Logger.Error(e,
                             "Failed to associate the simple range event {simpleRangeEventId} with the asset {assetId}.",
                             simpleRangeEventId, asset.Id);
            }
        }

        static class Commands
        {
            const string ASSOCIATE_WITH_FIREARM_SQL = """
                                                      INSERT INTO asset_files_firearms (firearm_id, asset_id)
                                                      VALUES (@FirearmId, @AssetId)
                                                      ON CONFLICT DO NOTHING
                                                      RETURNING row_id;
                                                      """;

            const string ASSOCIATE_WITH_RANGE_EVENT_SQL = """
                                                          INSERT INTO asset_files_simple_range_events (simple_range_event_id, asset_id)
                                                          VALUES (@SimpleRangeEventId, @AssetId)
                                                          ON CONFLICT DO NOTHING
                                                          RETURNING row_id;
                                                          """;

            internal static readonly DapperCommand AssociateWithRangeEvent = new(ASSOCIATE_WITH_RANGE_EVENT_SQL);

            internal static readonly DapperCommand AssociateWithFirearm = new(ASSOCIATE_WITH_FIREARM_SQL);
        }
    }
}