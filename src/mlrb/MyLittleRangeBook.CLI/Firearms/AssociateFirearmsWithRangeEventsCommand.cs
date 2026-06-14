using ConsoleAppFramework;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    /// <summary>
    /// Represents a command to associate firearms with range events.
    /// </summary>
    /// <remarks>
    /// This command processes relevant data to establish associations between firearms and range events.
    /// It extends the <see cref="MlrbFirearmsCommandBase"/> to leverage shared logic for firearm-related operations.
    /// </remarks>
    [RegisterCommands("firearms")]
    public class AssociateFirearmsWithRangeEventsCommand: MlrbFirearmsCommandBase
    {
        record struct SimpleRangeEventFirearmRow(
            string FirearmId,
            string SimpleRangeEventId,
            string FirearmName,
            int RoundsFired,
            string Created)
        {
            internal DateTimeOffset CreatedUtc => DateTimeOffset.Parse(Created);
        };

        internal static class Commands
        {
            /// <summary>
            /// Insert records in the junction table between firearms and simple_range_events.
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

        public AssociateFirearmsWithRangeEventsCommand(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper, IFirearmsService firearmsService, IFirearmAggregateRepository firearmAggregateRepo) : base(logger, display, sqliteHelper, firearmsService, firearmAggregateRepo)
        {
        }


        /// <summary>
        /// Associates firearms with range events by processing relevant data and establishing the association.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests during the execution.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains an integer status code indicating the success or failure of the command.</returns>
        [Command("associate-with-range-events"), UsedImplicitly]
        public async Task<int> AssociateWithRangeEvent(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Associate firearms with range events.");
            int returnCode = -1;
            IEnumerable<SimpleRangeEventFirearmRow> rows;
            try
            {
                await using var sc = await SqliteHelper
                    .GetScopedDatabaseConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
                var ctx = new DapperCommandContext(sc, null, cancellationToken);
                rows= await Commands.NewAssociationsCommand.QueryAsync<SimpleRangeEventFirearmRow>(ctx).ConfigureAwait(false);

                foreach (var row in rows)
                {
                    var fa = await FirearmAggregateRepository.GetAsync(row.FirearmId, cancellationToken)
                        .ConfigureAwait(false);
                    if (fa.IsFailed || fa.Value is null)
                    {
                        CliDisplay.PrintFailure($"Could not find firearm {row.FirearmId}:{row.FirearmName}.");
                        continue;
                    }

                    fa.Value!.AssociateWithRangeEvent(row.SimpleRangeEventId, row.RoundsFired, row.CreatedUtc);

                    var x = await FirearmAggregateRepository.SaveAsync(fa.Value!, cancellationToken).ConfigureAwait(false);
                    if (x.IsFailed)
                    {
                        Logger.Warning("Failed to save event stream for firearm {0}:{1}", row.FirearmId, row.FirearmName);
                        continue;
                    }

                    var f = fa.Value!.ToFirearm();
                    var y = await FirearmsService.UpsertAsync(ctx, f).ConfigureAwait(false);
                    if (y.IsFailed)
                    {
                        Logger.Warning("Failed to associate range events to firearm {0}:{1}", row.FirearmId, row.FirearmName);
                        continue;
                    }

                    var z = await Commands.InsertAssociationCommand.ExecuteAsync(ctx).ConfigureAwait(false);
                }

            }
            catch (Exception e)
            {
                Logger.Error(e, "Unexpected exception trying to associate range events to firearms.");
                returnCode = ReturnCodes.FAILURE;
                goto ExitFunction;
            }

            ExitFunction:
            PressEnterToContinue();
            return returnCode;
        }
    }
}