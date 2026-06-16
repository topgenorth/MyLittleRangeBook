using System.Globalization;
using ConsoleAppFramework;
using FluentResults;
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
        public UpdateFirearmsFromRangeEventsCommand(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper,
            IFirearmsService firearmsService,
            IFirearmAggregateRepository firearmAggregateRepo
        ) : base(logger, display, sqliteHelper, firearmsService, firearmAggregateRepo)
        {
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
            DapperCommandContext ctx;
            var returnCode = -1;

            #region Get the database context

            try
            {
                await using var scopedConn = await SqliteHelper.GetScopedDatabaseConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);

                ctx = new DapperCommandContext(scopedConn,CancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                CliDisplay.PrintFailure("Failed to get a connection to the database.");
                Logger.Fatal(ex, "Failed to get a connection to the database.");
                returnCode = ReturnCodes.FAILURE;

                goto ExitFunction;
            }

            #endregion

            var r1 = await FirearmAggregateRepository
                .GetNewFirearmNamesFromSimpleRangeEventsAsync(ctx)
                .ConfigureAwait(false);

            if (r1.IsFailed)
            {
                Logger.Fatal("Unexpected exception trying to retrieve new firearm names. {error}", r1.Errors[0]);
                CliDisplay.PrintFailure("Unexpected exception trying to retrieve new firearm names.");
                returnCode = ReturnCodes.FAILURE;
                goto ExitFunction;
            }

            if (r1.Value is null || !r1.Value!.Any())
            {
                CliDisplay.PrintSuccess("No new firearms found.");
                returnCode = ReturnCodes.SUCCESS;
                goto ExitFunction;
            }

            var firearmNames = r1.Value!;

            foreach (var row in firearmNames)
            {
                // ReSharper disable once JoinDeclarationAndInitializer
                FirearmAggregate firearmAgg;
                #region Step 1: Get (or create) the firearm aggregate

                var r2 = await FirearmAggregateRepository
                    .GetOrCreateByNameAsync(row.FirearmName, cancellationToken: cancellationToken, row.CreatedUtc)
                    .ConfigureAwait(false);
                if (r2.IsFailed)
                {
                    Logger.Error("Unexpected exception trying to create the aggregate for {firearmName} - moving on. {error}.", row.FirearmName, r2.Errors[0]);
                    CliDisplay.PrintFailure($"Unexpected exception trying to create the aggregate for {row.FirearmName}. {r2.Errors[0]}.");
                    continue;
                }

                firearmAgg = r2.Value!;

                Logger.Verbose("Working on {firearmId}:{firearmName}.", firearmAgg.Id, firearmAgg.Name);
                #endregion

                #region Step 2: Get a list of all SimpleRangeEvents for the firearm name with the round count (that are not already associated).
                var listOfRangeEvents = await FirearmAggregateRepository
                    .GetSimpleRangeEventRoundCountsByFirearmNameAsync(ctx, row.FirearmName)
                    .ConfigureAwait(false);
                foreach (var roundCountRow in listOfRangeEvents)
                {
                    // THe order here matters.  First associate the range event, the record rounds fired.
                    firearmAgg.AssociateWithSimpleRangeEvent(roundCountRow.SimpleRangeEventId, roundCountRow.CreatedUtc);
                    if (roundCountRow.RoundsFired > 0)
                    {
                        firearmAgg.MoreRoundsFired(roundCountRow.RoundsFired, roundCountRow.CreatedUtc);
                    }
                }

                var r3 = await FirearmAggregateRepository.SaveAsync(firearmAgg, ctx.CancellationToken)
                    .ConfigureAwait(false);
                if (r3.IsFailed)
                {
                    Logger.Warning("Failed to create firearm aggregate for {firearmName} - skipping this one. {error}", row.FirearmName, r3.Errors[0]);
                    CliDisplay.PrintFailure($"Unexpected errors with firearm {row.FirearmName} - skipping it.");
                    continue;
                }
                #endregion

                #region Step 3: Upsert the Firearms table with the FirearmAggregate

                var r4 = await FirearmsService.UpsertAsync(ctx, firearmAgg).ConfigureAwait(false);
                if (r4.IsFailed)
                {
                    CliDisplay.PrintWarning($"Failed to update the `firearm` table for {row.FirearmName}.");
                    Logger.Warning("There was a problem trying to update the firearm table {firearmName}. {error}",
                        row.FirearmName, r4.Errors[0]);
                    continue;
                }
                #endregion

                CliDisplay.PrintSuccess($"Created firearm aggregate and `firearm` record for {firearmAgg.Id}:{firearmAgg.Name}, {firearmAgg.RoundsFired} rounds fired.");
            }

            returnCode = ReturnCodes.SUCCESS;

            ExitFunction:
            PressEnterToContinue();

            return returnCode;
        }

        private static class Commands
        {
            private const string InsertSimpleRangeEventFirearmAssociationSql = """
                                                                               INSERT INTO firearms_simple_range_events (firearm_id, simple_range_event_id)
                                                                               VALUES (@FirearmId, @SimpleRangeEventId) 
                                                                               RETURNING row_id;
                                                                               """;

            internal static readonly DapperCommand InsertAssociationCommand = new(InsertSimpleRangeEventFirearmAssociationSql);
        }
    }
}