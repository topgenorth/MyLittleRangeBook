using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    /// <summary>
    ///     Represents a command to associate firearms with range events.
    /// </summary>
    /// <remarks>
    ///     This command processes relevant data to establish associations between firearms and range events.
    ///     It extends the <see cref="MlrbFirearmsCommandBase" /> to leverage shared logic for firearm-related operations.
    /// </remarks>
    [RegisterCommands("firearms")]
    public class SyncRangeEventsToFirearms : MlrbFirearmsCommandBase
    {
        public SyncRangeEventsToFirearms(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper,
            IFirearmsService firearmsService, IFirearmAggregateRepository firearmAggregateRepo) : base(logger, display,
            sqliteHelper, firearmsService, firearmAggregateRepo)
        {
        }

        async Task<Result> AssociateSimpleRangeEventToFirearmAsync(SimpleRangeEventFirearmRow row, CancellationToken ct)
        {
            try
            {
                var fa = await FirearmAggregateRepository.GetAsync(row.FirearmId, ct)
                    .ConfigureAwait(false);
                if (fa.IsFailed || fa.Value is null)
                {
                    var err = new Error("Could not find an event stream for firearm.");
                    return Result.Fail(err);
                }
                fa.Value!.AssociateWithRangeEvent(row.SimpleRangeEventId, row.RoundsFired, row.CreatedUtc);

                var x = await FirearmAggregateRepository
                    .SaveAsync(fa.Value!, ct)
                    .ConfigureAwait(false);
                if (x.IsFailed)
                {
                    return x;
                }

                await using var scopedConn = await SqliteHelper.GetScopedDatabaseConnectionAsync(ct).ConfigureAwait(false);
                var p = new {FirearmId = row.FirearmId, SimpleRangeEventId = row.SimpleRangeEventId};
                var ctx = new DapperCommandContext(scopedConn.Connection, CancellationToken: ct, Arguments: p);
                var y = await Commands.InsertAssociationCommand.ExecuteAsync(ctx).ConfigureAwait(false);

                Logger.Verbose("Update junction table `firearms_simple_range_event`, rowsAffected {0}.", y);
            }
            catch (Exception e)
            {
                var err = new Error("Unexpected exception trying to associate firearm with range event").CausedBy(e);
                return Result.Fail(err);
            }

            return Result.Ok();
        }

        /// <summary>
        ///     Associates firearms with range events by processing relevant data and establishing the association.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests during the execution.</param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result contains an integer status code indicating the
        ///     success or failure of the command.
        /// </returns>
        [Command("sync-from-range-events"), UsedImplicitly]
        public async Task<int> DoWOrk(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Sync firearms from range events.");
            var returnCode = -1;
            IEnumerable<SimpleRangeEventFirearmRow> rows;

            try
            {
                await using var scopedConn = await
                    SqliteHelper.GetScopedDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);
                var ctx = new DapperCommandContext(scopedConn.Connection, CancellationToken: cancellationToken);
                rows = await Commands.NewAssociationsCommand.QueryAsync<SimpleRangeEventFirearmRow>(ctx)
                    .ConfigureAwait(false);

            }
            catch (Exception e)
            {
                CliDisplay.PrintFailure("Failed to retrieve the new range event records.");
                returnCode = ReturnCodes.FAILURE;
                goto ExitFunction;
            }

            try
            {
                foreach (var row in rows)
                {
                    var r1 = await AssociateSimpleRangeEventToFirearmAsync(row, cancellationToken).ConfigureAwait(false);
                    if (r1.IsFailed)
                    {
                        Logger.Warning("Could not associate the range event with the firearm. Ignoreing.");
                        continue;
                    }

                }

                CliDisplay.PrintSuccess("Successfully sync'd new range events with firearms.");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unexpected exception trying to associate range events to firearms.");
                CliDisplay.PrintFailure("Failed to sync firearms from range events.");
                returnCode = ReturnCodes.FAILURE;
            }

            ExitFunction:
            PressEnterToContinue();
            return returnCode;
        }

        private record struct SimpleRangeEventFirearmRow(
            string FirearmId,
            string SimpleRangeEventId,
            string FirearmName,
            int RoundsFired,
            string Created)
        {
            internal DateTimeOffset CreatedUtc => DateTimeOffset.Parse(Created);
        }

        internal static class Commands
        {
            /// <summary>
            ///     Insert records in the junction table between firearms and simple_range_events.
            /// </summary>
            private const string SimpleRangeEventsWithoutFirearmsSql = """
                                                                       SELECT f.id AS FirearmId, s.id AS SimpleRangeEventId, s.firearm_name AS FirearmName,
                                                                              s.rounds_fired AS RoundsFired, s.created AS Created
                                                                       FROM simple_range_events s
                                                                                LEFT JOIN firearms f ON f.name = s.firearm_name
                                                                       WHERE s.id NOT IN (SELECT simple_range_event_id FROM firearms_simple_range_events)
                                                                       ORDER BY f.name;
                                                                       """;

            private const string InsertAssociationSql = """
                                                        INSERT INTO firearms_simple_range_events (firearm_id, simple_range_event_id)
                                                        VALUES (@firearmId, @simpleRangeEventId);
                                                        """;

            internal static readonly DapperCommand NewAssociationsCommand = new(SimpleRangeEventsWithoutFirearmsSql);
            internal static readonly DapperCommand InsertAssociationCommand = new(InsertAssociationSql);
        }
    }
}