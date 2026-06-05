using System.Data;
using System.Data.Common;
using ConsoleAppFramework;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Database
{
    [RegisterCommands("db"), UsedImplicitly]
    public class DataFixupCommands : MlrbSqliteCommandBase
    {
        readonly string[] _tablesToUpdate = ["Cartridges", "Firearms"];
        public DataFixupCommands(ILogger logger, ICliDisplay cliDisplay, ISqliteHelper sqliteHelper) :
            base(logger, cliDisplay, sqliteHelper)
        {
        }

        [Command("maintenance"), UsedImplicitly]
        public async Task<int> SqliteMainteance(CancellationToken cancellationToken)
        {
            CliDisplay.PrintCommandHeader("SQLite Maintenance.");
            await using ScopedSqliteConnection scope = await SqliteHelper.GetScopedDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);

            CliDisplay.Console.WriteLine("WAL checkpoint stuff");
            await SqliteHelper.CheckpointWalAsync(scope.Connection).ConfigureAwait(false);

            CliDisplay.Console.WriteLine("Vacuum ");
            await SqliteHelper.VacuumAync(scope.Connection).ConfigureAwait(false);

            CliDisplay.Console.WriteLine("Integrity check stuff");
            var x  = await SqliteHelper.IntegrityCheckAsync(scope.Connection).ConfigureAwait(false);
            Logger.Information("Database integrity check passed with result: {result}", x);

            CliDisplay.PrintSuccess("SQLite maintenance finished.");
            return ReturnCodes.SUCCESS;
        }

        /// <summary>
        ///     Migrates SimpleRangeEventRow IDs to ULID format in the database.
        /// </summary>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>An integer representing the success or failure of the operation.</returns>
        [Command("fix-pk-ids"), UsedImplicitly]
        public async Task<int> MigrateNanoidsToUlids(CancellationToken ct)
        {
            await using SqliteConnection conn = await SqliteHelper.GetDatabaseConnectionAsync(ct).ConfigureAwait(false);
            await using DbTransaction trans = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
            Logger.Information("Fixing up IDs on {databaseName}.", conn.DataSource);
            CliDisplay.PrintCommandHeader($"Fixing up IDs on {conn.DataSource}");

            Logger.Information("Skipping {tableName} - already done.", "SimpleRangeEvents");

            foreach (string tableName in _tablesToUpdate)
            {
                Result t = await UpdateIdsToUlids(conn, trans, tableName, ct).ConfigureAwait(false);
                if (!t.IsFailed)
                {
                    continue;
                }

                await trans.RollbackAsync(ct).ConfigureAwait(false);
                CliDisplay.PrintFailure($"There were issues fixing up ids on the {tableName}.");
                Logger.Warning("Could not update the IDs on {tableName}: {reason}.", tableName, t.Errors[0]);

                return ReturnCodes.FAILURE;
            }

            await trans.CommitAsync(ct).ConfigureAwait(false);
            Logger.Information("Finished with the ID fixup.");
            CliDisplay.PrintSuccess("Successfully updated IDs.");

            return ReturnCodes.SUCCESS;
        }

        async Task<Result> UpdateIdsToUlids(SqliteConnection conn,
            IDbTransaction trans,
            string tableName,
            CancellationToken ct)
        {
            var sql = $"SELECT Id from {tableName};";
            var update = $"UPDATE {tableName} SET Id=@newId WHERE Id=@oldId";
            Logger.Information("Updating IDs on: {tableName}.", tableName);
            IEnumerable<SimpleRow> rows =
                await conn.QueryAsync<SimpleRow>(sql, transaction: trans).ConfigureAwait(false);

            var convertedCount = 0;
            foreach (SimpleRow row in rows)
            {
                if (row.OldId.Equals(row.NewId, StringComparison.Ordinal))
                {
                    Logger.Verbose("Skipping {tableName} ID {id} - already a MrlbId.", tableName, row.OldId);
                    continue;
                }

                Logger.Verbose("Converting {tableName} ID from {oldId}, {newId}.", tableName, row.OldId, row.NewId);

                var ctx = new DapperCommandContext(conn, trans, ct, new { oldId = row.OldId, newId = row.NewId });
                var cmd = new DapperCommand(update);
                int i = await cmd.ExecuteAsync(ctx).ConfigureAwait(false);
                if (i < 1)
                {
                    Logger.Warning("Did not convert old {tableName} ID {oldId}  to new ID {newId}", tableName,
                        row.OldId, row.NewId);
                }
                else
                {
                    convertedCount++;
                }
            }

            Logger.Information("Updated {update} IDs on table: {tableName}.", convertedCount, tableName);

            return Result.Ok();
        }

        /// <summary>
        ///     If we don't have a valid Ulid for the ID, then create a new one.  Use the date of the event as
        ///     part of the ID generation.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        async Task<Result> ChangeSimpleRangeEventIdsToUlids(SqliteConnection conn,
            IDbTransaction trans,
            CancellationToken ct = default)
        {
            const string TABLENAME = "SimpleRangeEvents";
            Logger.Information("Updating IDs on: {tableName}.", TABLENAME);

            const string SQL = "SELECT Id, EventDate FROM SimpleRangeEvents;";
            const string UPDATE = "UPDATE SimpleRangeEvents SET Id = @newId WHERE Id = @oldId";

            IEnumerable<SimpleRangeEventRow> rows =
                await conn.QueryAsync<SimpleRangeEventRow>(SQL, transaction: trans).ConfigureAwait(false);

            var convertedCount = 0;
            foreach (SimpleRangeEventRow row in rows)
            {
                if (row.OldId.Equals(row.NewId, StringComparison.Ordinal))
                {
                    Logger.Verbose("Skipping {tableName} ID {id} - already a MrlbId.", TABLENAME, row.OldId);
                    continue;
                }
                Logger.Verbose("Converting {tableName} ID from {oldId}, {newId}.", TABLENAME, row.OldId, row.NewId);

                var ctx = new DapperCommandContext(conn, trans, ct, new { oldId = row.OldId, newId = row.NewId });
                var cmd = new DapperCommand(UPDATE );
                int i = await cmd.ExecuteAsync(ctx).ConfigureAwait(false);
                if (i < 1)
                {
                    Logger.Warning("Did not convert old {tableName} ID {oldId}  to new ID {newId}", TABLENAME,
                        row.OldId, row.NewId);
                }
                else
                {
                    convertedCount++;
                }
            }

            Logger.Information("Updated {update} IDs on table: {tableName}.", convertedCount, TABLENAME);


            return Result.Ok();
        }

        /// <summary>
        ///     This is the simplest case possible - the Id is some kind of string that we will convert.
        /// </summary>
        /// <param name="Id"></param>
        record struct SimpleRow(string Id)
        {
            internal string OldId => Id;
            internal string NewId => MlrbId.FromString(Id).ToString();
        }

        record struct SimpleRangeEventRow(string Id, string EventDate)
        {
            internal string OldId => Id;
            internal string NewId => MlrbId.From(EventDateTime).ToString();
            public DateTime EventDateTime => DateTime.Parse(EventDate).ToLocalTime();
        }
    }
}
