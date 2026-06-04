using ConsoleAppFramework;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.MlrbAssets
{
    /// <summary>
    /// This command will associate assets with a firearm and/or a simple range event.
    /// </summary>
    [RegisterCommands("assets"), UsedImplicitly]
    public class AssociateAssetsCommand: MlrbSqliteCommandBase
    {
        public AssociateAssetsCommand(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper) : base(logger, display, sqliteHelper)
        {
        }

        /// <summary>
        /// Imports a range asset file, associating it with the specified asset, firearm, and/or range event.
        /// </summary>
        /// <param name="assetId">The identifier of the asset to be imported.</param>
        /// <param name="firearmId">The optional identifier of the firearm to associate with the asset.</param>
        /// <param name="rangeEventId">The optional identifier of the range event to associate with the asset.</param>
        /// <returns>Returns an integer indicating the status of the operation.</returns>
        [Command("associate"), UsedImplicitly]
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task<int> ImportRangeAssetFile(string assetId, string? firearmId = null, string? rangeEventId = null)
        {
            CliDisplay.PrintCommandHeader("Associate asset");
            throw new NotImplementedException();
        }
    }
}
