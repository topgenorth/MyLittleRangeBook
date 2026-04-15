using System.Data;
using Dapper;
using FluentResults;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using NanoidDotNet;

namespace MyLittleRangeBook.Database.Sqlite
{
    public class SqliteSimpleRangeLogService : ISimpleRangeLogService
    {
        const string SelectSql = """
                                 SELECT *
                                 FROM SimpleRangeEvents 
                                 ORDER BY EventDate, FirearmName, RangeName;
                                 """;

        const string DeleteSql = "DELETE FROM SimpleRangeEvents WHERE Id = @Id;";

        const string InsertSql = """
                                 INSERT INTO SimpleRangeEvents (Id, EventDate, FirearmName, RangeName, RoundsFired, AmmoDescription, Notes, Created, Modified)
                                 VALUES (@Id, @EventDate, @FirearmName, @RangeName, @RoundsFired, @AmmoDescription, @Notes, @Created, @Modified)
                                 RETURNING RowId;
                                 """;

        const string UpdateSql = """
                                 UPDATE SimpleRangeEvents 
                                 SET EventDate = @EventDate, FirearmName = @FirearmName, RangeName = @RangeName, 
                                     RoundsFired = @RoundsFired, AmmoDescription = @AmmoDescription, Notes = @Notes, 
                                     Modified = @Modified
                                 WHERE Id = @Id;
                                 """;

        public async Task<Result<bool>> DeleteAsync(IDbConnection connection,
            SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default)
        {
            if (simpleRangeEvent.RowId is null)
            {
                var reason = new Success($"SimpleRangeEvent `{simpleRangeEvent.Id}` does not exist.");
                reason.WithMetadata("Id", simpleRangeEvent.Id);
                reason.WithMetadata("RowId", simpleRangeEvent.RowId);

                return Result.Ok().WithSuccess(reason);
            }

            try
            {
                var cmd = new SqliteCommand(DeleteSql, (SqliteConnection)connection);
                cmd.Parameters.AddWithValue("@Id", simpleRangeEvent.Id);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not delete SimpleRangeEvent `{simpleRangeEvent.Id}`: {e.Message}");
                err.CausedBy(e);
                EnrichError(err, simpleRangeEvent);

                return Result.Fail(err);
            }

            return Result.Ok(true);
        }

        public async Task<Result<long?>> UpsertAsync(IDbConnection connection,
            SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default)
        {
            simpleRangeEvent.Modified = DateTimeOffset.UtcNow;
            try
            {
                simpleRangeEvent.Id ??= await Nanoid.GenerateAsync();

                if (simpleRangeEvent.RowId is null)
                {
                    long? rowId = await connection.QuerySingleAsync<long>(InsertSql, simpleRangeEvent);
                    simpleRangeEvent.RowId = rowId;
                }
                else
                {
                    await connection.ExecuteAsync(UpdateSql, simpleRangeEvent);
                }

                var reason = new Success($"SimpleRangeEvent `{simpleRangeEvent.Id}` saved.");
                reason.WithMetadata("Id", simpleRangeEvent.Id);
                reason.WithMetadata("RowId", simpleRangeEvent.RowId);

                return Result.Ok(simpleRangeEvent.RowId).WithSuccess(reason);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not save SimpleRangeEvent `{simpleRangeEvent.Id}`: {e.Message}");
                err.CausedBy(e);
                EnrichError(err, simpleRangeEvent);

                return Result.Fail(err);
            }
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(IDbConnection connection,
            CancellationToken cancellationToken = default)
        {
            var conn = (SqliteConnection)connection;

            try
            {
                var rangeEvents = await conn.QueryAsync<SimpleRangeEvent>(SelectSql, cancellationToken);
                return Result.Ok(rangeEvents);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not retrieve SimpleRangeEvents from database: {e.Message}");
                err.CausedBy(e);

                return Result.Fail(err);
            }
        }

        static void EnrichError(Error error, SimpleRangeEvent simpleRangeEvent)
        {
            error.WithMetadata("Id", simpleRangeEvent.Id);
            error.WithMetadata("RowId", simpleRangeEvent.RowId);
        }
    }
}
