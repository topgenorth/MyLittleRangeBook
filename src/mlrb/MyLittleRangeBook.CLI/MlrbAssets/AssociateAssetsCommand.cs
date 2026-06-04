using System.Data.Common;
using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

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
        readonly IFirearmAggregateRepository _firearmAggregateRepository;

        public AssociateAssetsCommand(ILogger logger,
            ICliDisplay display,
            ISqliteHelper sqliteHelper,
            IFirearmAggregateRepository firearmAggregateRepository,
            IMlrbAssetAggregateRepository assetAggregateRepository) : base(logger,
            display, sqliteHelper)
        {
            _firearmAggregateRepository = firearmAggregateRepository;
            _assetAggregateRepository = assetAggregateRepository;
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
        public async Task<int> ImportRangeAssetFile(string assetId,
            string? firearmId = null,
            string? simpleRangeEventId = null,
            CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Associate asset");

            Result<MlrbAssetAggregate?> a = await _assetAggregateRepository.GetAsync(assetId, cancellationToken)
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

            await using ScopedSqliteConnection scope = await SqliteHelper
                .GetScopedDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            await AssociateFirearmAsync(scope.Connection, a.Value, firearmId, cancellationToken)
                .ConfigureAwait(false);
            await AssociateSimpleRangeEventAsync(scope.Connection, a.Value, simpleRangeEventId, cancellationToken)
                .ConfigureAwait(false);

            CliDisplay.PrintSuccess("Finished with associations.");

            PressEnterToContinue();

            return ReturnCodes.SUCCESS;
        }

        async Task AssociateFirearmAsync(SqliteConnection conn,
            MlrbAssetAggregate asset,
            string? firearmId,
            CancellationToken cancellationToken)
        {
            if (firearmId is null)
            {
                return;
            }


            try
            {
                Result<FirearmAggregate> f = await _firearmAggregateRepository.GetAsync(firearmId, cancellationToken)
                    .ConfigureAwait(false);
                if (f.IsFailed)
                {
                    Logger.Warning(
                        "Failed to create the association between {assetId}:{asset} and firearm {firearmId}, {reason}.",
                        asset.Id, asset.DestinationPath, firearmId, f.Errors[0].Message);

                    return;
                }

                var p = new { FirearmId = firearmId, AssetId = asset.Id.ToString() };
                var ctx = new DapperCommandContext(conn, null, cancellationToken, p);
                await Commands.AssociateWithFirearm.ExecuteScalarAsync<long?>(ctx).ConfigureAwait(false);

                DateTimeOffset dto = DateTimeOffset.UtcNow;
                asset.AssociatedWithFirearm(firearmId, dto);
                f.Value.AssociatedWithAsset(asset.Id, dto);

                await _firearmAggregateRepository.SaveAsync(f.Value, cancellationToken).ConfigureAwait(false);
                await _assetAggregateRepository.SaveAsync(asset, cancellationToken).ConfigureAwait(false);

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

        async Task AssociateSimpleRangeEventAsync(SqliteConnection conn,
            MlrbAssetAggregate asset,
            string? simpleRangeEventId,
            CancellationToken cancellationToken)
        {
            Logger.Warning("AssociateSimpleRangeEventAsync is not implemented.");

            return;

            await using DbTransaction trans = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (simpleRangeEventId is null)
                {
                    return;
                }


                // TODO [TO20260604] We don't have an aggregate for a simple range event.


                var p = new { SimpleRangeEventId = simpleRangeEventId, AssetId = asset.Id.ToString() };
                var ctx = new DapperCommandContext(conn, null, cancellationToken, p);
                long? l = await Commands.AssociateWithRangeEvent.ExecuteScalarAsync<long?>(ctx).ConfigureAwait(false);
                if (l is null)
                {
                    Logger.Warning("Failed to associate the firearm {simpleRangeEventId} with the asset {assetId}.",
                        simpleRangeEventId, asset.Id);
                }

                await trans.CommitAsync(cancellationToken).ConfigureAwait(false);
                CliDisplay.PrintSuccess($"Associated simple range event {simpleRangeEventId} with asset {asset.Id}.");
                Logger.Information("Associated simple range event {simpleRangeEventId} with asset {assetId}.",
                    simpleRangeEventId, asset.Id);
            }
            catch (Exception e)
            {
                CliDisplay.PrintFailure("Failed to associate simple range event. " + e.Message);
                Logger.Error(e,
                    "Failed to associate the simple range event {simpleRangeEventId} with the asset {assetId}.",
                    simpleRangeEventId, asset.Id);
                await trans.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        static class Commands
        {
            const string AssociateWithFirearmSql = """
                                                   INSERT INTO asset_files_firearms (firearm_id, asset_id)
                                                   VALUES (@FirearmId, @AssetId)
                                                   ON CONFLICT DO NOTHING 
                                                   RETURNING row_id;
                                                   """;

            const string AssociateWithRangeEventSql = """
                                                      INSERT INTO asset_files_simplerangeevents (simple_range_event_id, asset_id)
                                                      VALUES (@SimpleRangeEventId, @AssetId)
                                                      ON CONFLICT DO NOTHING 
                                                      RETURNING row_id;
                                                      """;

            internal static readonly DapperCommand AssociateWithRangeEvent = new(AssociateWithRangeEventSql);

            internal static readonly DapperCommand AssociateWithFirearm = new(AssociateWithFirearmSql);
        }
    }
}
