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
                await using var trans =
                    await scopedConn.Connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                ctx = new DapperCommandContext(scopedConn, trans, cancellationToken);
            }
            catch (Exception ex)
            {
                CliDisplay.PrintFailure("Failed to get a connection to the database.");
                Logger.Fatal(ex, "Failed to get a connection to the database.");
                returnCode = ReturnCodes.FAILURE;

                goto ExitFunction;
            }

            #endregion

            var firearms = await FirearmAggregateRepository.GetNewFirearmNamesFromSimpleRangeEventsAsync(ctx).ConfigureAwait(false);

            foreach (var row in firearms)
            {
                // ReSharper disable once JoinDeclarationAndInitializer
                FirearmAggregate firearmAgg;
                #region Step 1: Get (or create) the firearm aggregate

                var w = await FirearmAggregateRepository
                    .GetOrCreateByNameAsync(row.FirearmName, cancellationToken: cancellationToken, row.CreatedUtc)
                    .ConfigureAwait(false);
                if (w.IsFailed)
                {
                    Logger.Error("Unexpected exception trying to create the aggregate for {firearmName} - moving on. {error}.", row.FirearmName, w.Errors[0]);
                    CliDisplay.PrintFailure($"Unexpected exception trying to create the aggregate for  {row.FirearmName}. {w.Errors[0]}.");
                    continue;
                }

                firearmAgg = w.Value!;

                #endregion

                #region Step 2: Get a list of all SimpleRangeEvents for the firearm name with the round count.
                var list = await FirearmAggregateRepository
                    .GetSimpleRangeEventRoundCountsByFirearmNameAsync(ctx, row.FirearmName)
                    .ConfigureAwait(false);
                #endregion

                #region Step 3: Upsert the Firearms table with the FirearmAggregate
                #endregion

                CliDisplay.PrintInfo($"Created firearm aggreate and firearm  {firearmAgg.Id}:{firearmAgg.Name}, {firearmAgg.RoundsFired} rounds fired.");
            }

            returnCode = ReturnCodes.SUCCESS;

            ExitFunction:
            PressEnterToContinue();

            return returnCode;
        }

        private async Task<Result> UpdateFirearm(Firearm f, List<SimpleRangeEventFirearmRow> associations,
            CancellationToken cancellationToken)
        {
            await using var scopedConn = await SqliteHelper.GetScopedDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            await using var trans =
                await scopedConn.Connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var ctx = new DapperCommandContext(scopedConn.Connection, trans, cancellationToken);

            var x = await FirearmsService.UpsertAsync(ctx, f).ConfigureAwait(false);
            if (x.IsFailed)
            {
                await trans.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return Result.Fail(x.Errors);
            }

            try
            {
                foreach (var row in associations)
                {
                    var p = new { row.FirearmId, row.SimpleRangeEventId };
                    var ctx2 = ctx with { Arguments = p };
                    var y = Commands.InsertAssociationCommand.ExecuteScalarAsync<long>(ctx2);
                }

                await trans.CommitAsync(cancellationToken).ConfigureAwait(true);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return Result.Fail(ex.Message);
            }
        }



        private readonly record struct SimpleRangeEventFirearmRow(string FirearmId, string SimpleRangeEventId);

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