using System.Data;
using System.Data.Common;
using System.Globalization;
using ByteAether.Ulid;
using ConsoleAppFramework;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Sqlite;

namespace MyLittleRangeBook.Database
{
    [RegisterCommands("db")]
    [UsedImplicitly]
    public class DataFixupCommands : MlrbSqliteCommandBase
    {
        public DataFixupCommands(ILogger logger, ICliDisplay cliDisplay, ISqliteHelper sqliteHelper) :
            base(logger, cliDisplay, sqliteHelper)
        {
        }

        /// <summary>
        ///     Migrates SimpleRangeEventRow IDs to ULID format in the database.
        /// </summary>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>An integer representing the success or failure of the operation.</returns>
        [Command("fix-pk-ids")]
        [UsedImplicitly]
        public async Task<int> MigrateNanoidsToUlids(CancellationToken ct)
        {
            await using SqliteConnection conn = await SqliteHelper.GetDatabaseConnectionAsync(ct).ConfigureAwait(false);
            await using DbTransaction trans = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
            Logger.Information("Fixing up IDs on {databaseName}.", conn.DataSource);
            CliDisplay.PrintCommandHeader($"Fixing up IDs on {conn.DataSource}");

            Result t1 = await ChangeSimpleRangeEventIdsToUlids(conn, trans, ct).ConfigureAwait(false);
            Result t2 = await ChangeFitFileIdsToUlids(conn, trans, ct).ConfigureAwait(false);

            if (t1.IsFailed || t2.IsFailed)
            {
                await trans.RollbackAsync(ct).ConfigureAwait(false);
                Logger.Warning("One of the tasks failed - rolling back.");
                CliDisplay.PrintFailure("Failed to update IDs.");

                return ReturnCodes.FAILURE;
            }

            await trans.CommitAsync(ct).ConfigureAwait(false);
            Logger.Information("Finished with the ID fixup.");
            CliDisplay.PrintSuccess("Successfully updated IDs.");

            return ReturnCodes.SUCCESS;
        }


        async Task<Result> ChangeFitFileIdsToUlids(SqliteConnection conn,
            IDbTransaction trans,
            CancellationToken ct = default)
        {
            const string SQL = "SELECT Id, FileName FROM main.FitFiles WHERE length(Id) <> 26;";
            const string UPDATE = "UPDATE main.FitFiles SET Id = @newId WHERE Id = @oldId";

            Logger.Information("Converting any IDs that are not 26 characters on FitFiles.");
            IEnumerable<FitFileRow> rows =
                await conn.QueryAsync<FitFileRow>(SQL, transaction: trans).ConfigureAwait(false);
            var idMap = rows
                .Where(row => !Ulid.IsValid(row.Id))
                .ToDictionary<FitFileRow, string, string>(row => row.Id,
                    row => MlrbId.From(row.FitFileTime));
            Logger.Information("It seems that there are {count} IDs that need to be converted in FitFiles.", idMap.Count);

            var convertedCount = 0;
            foreach (KeyValuePair<string, string> kvp in idMap)
            {
                Logger.Verbose($"Converting FitFile ID {kvp.Key} to {kvp.Value}");
                var cmd = new DapperCommand(UPDATE, new { oldId = kvp.Key, newId = kvp.Value });
                int i = await cmd.ExecuteAsync(conn, trans, ct).ConfigureAwait(false);
                if (i < 1)
                {
                    Logger.Warning("Did not convert old ID {oldId}  to new ID {newId}", kvp.Key, kvp.Value);
                }
                else
                {
                    convertedCount++;
                }
            }

            Logger.Information("Update {update} IDs.", convertedCount);

            return Result.Ok();
        }

        /// <summary>
        ///     If we don't have a valid Ulid for the ID, then create a new one.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        async Task<Result> ChangeSimpleRangeEventIdsToUlids(SqliteConnection conn,
            IDbTransaction trans,
            CancellationToken ct = default)
        {
            Logger.Information("Converting any IDs that are not 26 characters on SimpleRangeEvents.");
            const string SQL = "SELECT Id, EventDate FROM SimpleRangeEvents WHERE length(Id) <> 26 ORDER BY EventDate;";
            const string UPDATE = "UPDATE main.SimpleRangeEvents SET Id = @newId WHERE Id = @oldId";

            IEnumerable<SimpleRangeEventRow> rows =
                await conn.QueryAsync<SimpleRangeEventRow>(SQL, transaction: trans).ConfigureAwait(false);
            var idMap = rows
                .Where(simpleRangeEventRow => !Ulid.IsValid(simpleRangeEventRow.Id))
                .ToDictionary<SimpleRangeEventRow, string, string>(simpleRangeEventRow => simpleRangeEventRow.Id,
                    simpleRangeEventRow => MlrbId.From(simpleRangeEventRow.EventDateTime));
            Logger.Information("It seems that there are {count} IDs that need to be converted.", idMap.Count);

            var convertedCount = 0;
            foreach (KeyValuePair<string, string> kvp in idMap)
            {
                Logger.Verbose($"Converting SimpleRangeEvent ID {kvp.Key} to {kvp.Value}");
                var cmd = new DapperCommand(UPDATE, new { oldId = kvp.Key, newId = kvp.Value });
                int i = await cmd.ExecuteAsync(conn, trans, ct).ConfigureAwait(false);
                if (i < 1)
                {
                    Logger.Warning("Did not convert old ID {oldId}  to new ID {newId}", kvp.Key, kvp.Value);
                }
                else
                {
                    convertedCount++;
                }
            }

            Logger.Information("Update {update} IDs.", convertedCount);

            return Result.Ok();
        }

        public sealed record FitFileRow(string Id, string FileName)
        {
            const string FitFileDateFormat = "MM-dd-yyyy_HH-mm-ss";

            public DateTime FitFileTime
            {
                get
                {
                    string withoutExtension = Path.GetFileNameWithoutExtension(FileName);

                    CultureInfo culture = CultureInfo.InvariantCulture;

                    var timestamp = DateTime.ParseExact(
                        withoutExtension,
                        FitFileDateFormat,
                        culture,
                        DateTimeStyles.AssumeLocal // ensures Kind == Local
                    );

                    return timestamp;
                }
            }
        }

        public sealed record SimpleRangeEventRow(string Id, string EventDate)
        {
            public DateTime EventDateTime => DateTime.Parse(EventDate).ToLocalTime();
        }
    }
}
