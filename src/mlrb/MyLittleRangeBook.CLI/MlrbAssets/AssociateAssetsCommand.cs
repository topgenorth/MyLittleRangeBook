using ConsoleAppFramework;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.MlrbAssets
{
    /// <summary>
    ///     This command will associate a firearm and/or a simple range event with an asset.
    /// </summary>
    [RegisterCommands("assets"), UsedImplicitly]
    public class AssociateAssetsCommand : MlrbSqliteCommandBase
    {
        public AssociateAssetsCommand(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper) : base(logger,
            display, sqliteHelper)
        {
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
            await using ScopedSqliteConnection scope = await SqliteHelper
                .GetScopedDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            await AssociateFirearmAsync(scope.Connection, assetId, firearmId, cancellationToken)
                .ConfigureAwait(false);
            await AssociateSimpleRangeEventAsync(scope.Connection, assetId, simpleRangeEventId, cancellationToken)
                .ConfigureAwait(false);

            CliDisplay.PrintSuccess("Finished with associations.");
            return ReturnCodes.SUCCESS;
        }

        async Task AssociateFirearmAsync(SqliteConnection conn,
            string assetId,
            string? firearmId,
            CancellationToken cancellationToken)
        {
            if (firearmId is null)
            {
                return;
            }

            try
            {
                var p = new { FirearmId = firearmId, AssetId = assetId };
                var ctx = new DapperCommandContext(conn, null, cancellationToken) { Arguments = p };
                long? l = await Commands.AssociateWithFirearm.ExecuteScalarAsync<long?>(ctx).ConfigureAwait(false);
                if (l is null)
                {
                    Logger.Warning("Failed to associate the firearm {firearmId} with the asset {assetId}.", firearmId,
                        assetId);
                }
            }
            catch (Exception e)
            {
                CliDisplay.PrintFailure("Failed to associate firearm. " + e.Message);
                Logger.Error(e, "Failed to associate the firearm {firearmId} with the asset {assetId}.", firearmId,
                    assetId);
            }
        }

        async Task AssociateSimpleRangeEventAsync(SqliteConnection conn,
            string assetId,
            string? simpleRangeEventId,
            CancellationToken cancellationToken)
        {
            try
            {
                if (simpleRangeEventId is null)
                {
                    return;
                }

                var p = new { SimpleRangeEventId = simpleRangeEventId, AssetId = assetId };
                var ctx = new DapperCommandContext(conn, null, cancellationToken) { Arguments = p };
                long? l = await Commands.AssociateWithRangeEvent.ExecuteScalarAsync<long?>(ctx).ConfigureAwait(false);
                if (l is null)
                {
                    Logger.Warning("Failed to associate the firearm {simpleRangeEventId} with the asset {assetId}.",
                        simpleRangeEventId, assetId);
                }
            }
            catch (Exception e)
            {
                CliDisplay.PrintFailure("Failed to associate simple range event. " + e.Message);
                Logger.Error(e,
                    "Failed to associate the simple range event {simpleRangeEventId} with the asset {assetId}.",
                    simpleRangeEventId, assetId);
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
