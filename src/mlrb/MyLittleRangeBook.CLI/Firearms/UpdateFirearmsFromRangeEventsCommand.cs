using System.Data.Common;
using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    [RegisterCommands("firearms")]
    public class UpdateFirearmsFromRangeEventsCommand : MlrbSqliteCommandBase
    {
        readonly IFirearmAggregateRepository _firearmAggregateRepo;
        readonly IFirearmsService _firearmsService;

        public UpdateFirearmsFromRangeEventsCommand(ILogger logger,
            ICliDisplay display,
            ISqliteHelper sqliteHelper,
            IFirearmsService firearmsService,
            IFirearmAggregateRepository firearmAggregateRepo) : base(logger, display, sqliteHelper)
        {
            _firearmsService = firearmsService;
            _firearmAggregateRepo = firearmAggregateRepo;
        }

        /// <summary>
        ///     This is a maintenance task. It will update the Firearms table based on what is in the SimpleRangeEvents table.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("update-from-rangeevents")]
        [UsedImplicitly]
        public async Task<int> UpdateFirearmsFromRangeEvents(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Import new firearms from range events.");

            int returnCode = -1;
            IEnumerable<NewFirearmWithRoundCountRow> firearms;
            try
            {
                await using SqliteConnection conn = await SqliteHelper.GetDatabaseConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
                await using DbTransaction trans =
                    await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                var ctx = new DapperCommandContext(conn, trans, cancellationToken);
                firearms = await Commands
                    .GetNewFirearmsFromRangeEvents
                    .QueryAsync<NewFirearmWithRoundCountRow>(ctx)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                CliDisplay.PrintFailure("something bad happened.");
                Logger.Error(ex, "Failed to update firearms from range events");
                returnCode = ReturnCodes.FAILURE;

                goto ExitFunction;
            }

            int importCount = 0;
            foreach (NewFirearmWithRoundCountRow row in firearms)
            {
                Result<FirearmAggregate> fa = await _firearmAggregateRepo.GetByNameAsync(row.FirearmName, cancellationToken).ConfigureAwait(false);
                if (fa.IsFailed)
                {
                    CliDisplay.PrintFailure($"Could not import firearm {row.FirearmName}");
                    continue;;
                }
                fa.Value.AppendToNotes("Imported from range events.", DateTimeOffset.UtcNow);
                if (fa.Value.Version <2)
                {
                    // [TO20260531] Assume this is a new firearm, update the round count.
                    fa.Value.MoreRoundsFired(row.TotalRoundsFired, DateTimeOffset.UtcNow);
                }

                Result x = await _firearmAggregateRepo.SaveAsync(fa.Value, cancellationToken).ConfigureAwait(false);
                if (x.IsSuccess)
                {
                    importCount++;
                }

            }

            returnCode = ReturnCodes.SUCCESS;
            CliDisplay.PrintSuccess($"Imported {importCount} firearms from range events.");

            ExitFunction:
            PressAnyKeyToContinue();

            return returnCode;
        }

        internal record struct NewFirearmWithRoundCountRow(string FirearmName, int TotalRoundsFired);

        static class Commands
        {
            const string GetNewFirearmNamesFromRangeEventsSql = """
                                                                SELECT 
                                                                    SimpleRangeEvents.FirearmName,
                                                                    COALESCE(SUM(SimpleRangeEvents.RoundsFired), 0) AS TotalRoundsFired
                                                                FROM SimpleRangeEvents
                                                                WHERE SimpleRangeEvents.FirearmName NOT IN (SELECT Name FROM Firearms)
                                                                GROUP BY SimpleRangeEvents.FirearmName
                                                                ORDER BY SimpleRangeEvents.FirearmName;
                                                                """;

            internal static readonly DapperCommand GetNewFirearmsFromRangeEvents =
                new(GetNewFirearmNamesFromRangeEventsSql);
        }
    }
}
