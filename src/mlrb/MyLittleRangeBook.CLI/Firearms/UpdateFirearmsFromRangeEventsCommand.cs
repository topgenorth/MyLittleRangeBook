using ConsoleAppFramework;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    [RegisterCommands("firearms")]
    public class UpdateFirearmsFromRangeEventsCommand : MlrbFirearmsCommandBase
    {
        private IProjector _projector;
        public UpdateFirearmsFromRangeEventsCommand(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper,
            IFirearmsService firearmsService, IFirearmAggregateRepository firearmAggregateRepo
            ) : base(logger, display, sqliteHelper, firearmsService, firearmAggregateRepo)
        {

        }

        /// <summary>
        ///     Recalculates the total round count per firearm based on range events.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("recalculate-round-count")]
        [UsedImplicitly]
        public async Task<int> UpdateTotalRoundCountForFirearms(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Recalculate firearm round counts.");
            try
            {
                await using var scopedConn = await SqliteHelper.GetScopedDatabaseConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);

                var ctx = new DapperCommandContext(scopedConn.Connection, CancellationToken: cancellationToken);

                IEnumerable<(string FirearmName, int TotalRounds)> firearmsWithRounds = await Commands.RoundCountCommand
                    .QueryAsync<(string FirearmName, int TotalRounds)>(ctx)
                    .ConfigureAwait(false);

                foreach (var row in firearmsWithRounds)
                {
                    #region Capture the domain events.

                    var faResult = await FirearmAggregateRepository
                        .GetOrCreateByNameAsync(row.FirearmName, cancellationToken)
                        .ConfigureAwait(false);

                    if (faResult.IsFailed)
                    {
                        Logger.Warning("Failed to retrieve aggregate for firearm '{FirearmName}'.", row.FirearmName);
                        continue;
                    }

                    var fa = faResult.Value;
                    if (fa.RoundsFired == row.TotalRounds)
                    {
                        continue;
                    }

                    fa.TotalRoundCountRecalculated(row.TotalRounds, DateTimeOffset.UtcNow);
                    var saveResult = await FirearmAggregateRepository
                        .SaveAsync(fa, cancellationToken)
                        .ConfigureAwait(false);
                    if (saveResult.IsSuccess)
                    {
                    }
                    else
                    {
                        Logger.Warning("Failed to save recalculated round count for firearm '{FirearmName}'.",
                            row.FirearmName);
                        continue;
                    }

                    Logger.Verbose("Recalculating rounds for '{FirearmName}': {OldCount} -> {NewCount}",
                        row.FirearmName, fa.RoundsFired, row.TotalRounds);

                    #endregion

                }

                CliDisplay.PrintSuccess("Firearm round count recalculation complete.");
                return ReturnCodes.SUCCESS;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while recalculating round counts.");
                CliDisplay.PrintFailure("Failed to recalculate round counts.");
                return ReturnCodes.FAILURE;
            }
            finally
            {
                PressEnterToContinue();
            }
        }



        /// <summary>
        ///     This is a maintenance task. It will update the Firearms table based on what is in the SimpleRangeEvents table.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("import-from-range-events")]
        [UsedImplicitly]
        public async Task<int> ImportFirearmsFromRangeEvents(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Import new firearms from range events.");

            var returnCode = -1;
            IEnumerable<NewFirearmWithRoundCountRow> firearms;
            try
            {
                await using var scopedConn = await SqliteHelper.GetScopedDatabaseConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
                await using var trans =
                    await scopedConn.Connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                var ctx = new DapperCommandContext(scopedConn, trans, cancellationToken);
                firearms = await Commands
                    .GetNewFirearmsFromRangeEvents
                    .QueryAsync<NewFirearmWithRoundCountRow>(ctx)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                CliDisplay.PrintFailure("something bad happened trying to figure out new firearms.");
                Logger.Error(ex, "Failed to update firearms from range events");
                returnCode = ReturnCodes.FAILURE;

                goto ExitFunction;
            }

            var importCount = 0;
            var totalCount = firearms.Count();
            foreach (var row in firearms)
            {
                var fa = await FirearmAggregateRepository.GetOrCreateByNameAsync(row.FirearmName, cancellationToken)
                    .ConfigureAwait(false);
                if (fa.IsFailed)
                {
                    CliDisplay.PrintFailure($"Could not import firearm {row.FirearmName}");
                    continue;
                }

                fa.Value.AppendToNotes("Imported from range events.", DateTimeOffset.UtcNow);
                if (fa.Value.Version < 2)
                {
                    // [TO20260531] Assume this is a new firearm, update the round count.
                    fa.Value.MoreRoundsFired(row.TotalRoundsFired, DateTimeOffset.UtcNow);
                }

                var x = await FirearmAggregateRepository.SaveAsync(fa.Value, cancellationToken).ConfigureAwait(false);
                if (!x.IsSuccess)
                {
                    Logger.Warning("Failed to save event stream for firearm '{firearm}'.", row.FirearmName);
                    continue;
                }

                try
                {
                    var f = fa.Value!.ToFirearm();
                    await using var conn = await SqliteHelper.GetDatabaseConnectionAsync(cancellationToken)
                        .ConfigureAwait(false);
                    await using var trans =
                        await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                    var ctx = new DapperCommandContext(conn, trans, cancellationToken);
                    var y = await FirearmsService.UpsertAsync(ctx, f).ConfigureAwait(false);

                    if (y.IsFailed)
                    {
                        Logger.Warning("Failed to add '{firearm}' to Firearms table.", f.Name);
                        await trans.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await trans.CommitAsync(cancellationToken).ConfigureAwait(false);
                        importCount++;
                    }
                }
                catch (Exception e2)
                {
                    Logger.Warning(e2, "Failed to update the firearms table!");
                }
            }

            returnCode = ReturnCodes.SUCCESS;
            CliDisplay.PrintSuccess($"Imported {importCount}/{totalCount} firearms from range events.");

            ExitFunction:
            PressEnterToContinue();

            return returnCode;
        }

        internal record struct NewFirearmWithRoundCountRow(string FirearmName, int TotalRoundsFired);


        private static class Commands
        {
            /// <summary>
            /// Cound the rounds fired per firearm.
            /// </summary>
            private const string RoundCountSql = """
                                                 SELECT 
                                                     firearm_name AS FirearmName, 
                                                     COALESCE(SUM(rounds_fired), 0) AS TotalRounds 
                                                 FROM simple_range_events 
                                                 GROUP BY firearm_name;
                                                 """;



            /// <summary>
            /// Retrieves the names of firearms and their associated total rounds fired,
            /// based on range events, for firearms that are not yet present in the firearms table.
            /// The results are grouped and ordered by firearm name.
            /// </summary>
            private const string GetNewFirearmNamesFromRangeEventsSql = """
                                                                        SELECT 
                                                                            simple_range_events.firearm_name AS FirearmName,
                                                                            COALESCE(SUM(simple_range_events.rounds_fired), 0) AS TotalRoundsFired
                                                                        FROM simple_range_events
                                                                        WHERE simple_range_events.firearm_name NOT IN (SELECT name FROM firearms)
                                                                        GROUP BY simple_range_events.firearm_name
                                                                        ORDER BY simple_range_events.firearm_name;
                                                                        """;

            internal static readonly DapperCommand GetNewFirearmsFromRangeEvents =
                new(GetNewFirearmNamesFromRangeEventsSql);

            internal static readonly DapperCommand RoundCountCommand = new(RoundCountSql);
         }
    }
}