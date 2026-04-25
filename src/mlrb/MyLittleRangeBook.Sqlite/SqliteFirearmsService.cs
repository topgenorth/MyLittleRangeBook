using System.Data;
using Dapper;
using FluentResults;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook.Database.Sqlite
{
    public class SqliteFirearmsService : IFirearmsService
    {
        const string SelectSql = "SELECT * FROM Firearms ORDER BY Name;";
        const string DeleteSql = "DELETE FROM Firearm WHERE Id = @Id";

        const string InsertSql = """
                                 INSERT INTO Firearms (Id, Name, Notes) 
                                 VALUES (nanoid(), @Name, @Notes) 
                                 RETURNING RowId
                                 """;

        const string UpdateSql = """
                                 UPDATE Firearms 
                                 SET Name = @Name, Notes = @Note, Modified = @Modified 
                                 WHERE Id = @Id
                                 """;

        static SqliteFirearmsService()
        {
            SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        }

        private class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
        {
            public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
            {
                parameter.Value = value.ToString("O");
            }

            public override DateTimeOffset Parse(object value)
            {
                return DateTimeOffset.Parse((string)value);
            }
        }

        public async Task<Result<bool>> DeleteAsync(IDbConnection connection,
            Firearm firearm,
            CancellationToken cancellationToken = default)
        {
            if (firearm.RowId is null)
            {
                var reason = new Success($"Firearm `{firearm.Id}` does not exist.");
                reason.WithMetadata("Id", firearm.Id);
                reason.WithMetadata("RowId", firearm.RowId);

                return Result.Ok().WithSuccess(reason);
            }

            try
            {
                var cmd = new SqliteCommand(DeleteSql, (SqliteConnection)connection);
                cmd.Parameters.AddWithValue("@Id", firearm.Id);
                cmd.CommandType = CommandType.Text;
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not delete Firearm `{firearm.Id}`: {e.Message}");
                err.CausedBy(e);
                EnrichError(err, firearm);

                return Result.Fail(err);
            }

            return Result.Ok(true);
        }

        public async Task<Result<long?>> UpsertAsync(IDbConnection connection,
            Firearm firearm,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (firearm.RowId is null)
                {
                    var valuesToInsert = new { firearm.Id, firearm.Name, firearm.Notes };
                    firearm.RowId = await connection.QuerySingleAsync<long>(InsertSql, valuesToInsert);
                }
                else
                {
                    await connection.ExecuteAsync(UpdateSql, firearm);
                }

                var success = new Success($"Firearm `{firearm.Id}` saved.");
                success.WithMetadata("Id", firearm.Id);
                success.WithMetadata("RowId", firearm.RowId);
                return Result.Ok(firearm.RowId).WithSuccess(success);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not save Firearm `{firearm.Id}`: {e.Message}");
                err.CausedBy(e);
                EnrichError(err, firearm);

                return Result.Fail(err);
            }
        }

        public async Task<Result<IEnumerable<Firearm>>> GetFirearmsAsync(IDbConnection connection,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IEnumerable<Firearm> firearms = await connection.QueryAsync<Firearm>(SelectSql, cancellationToken);
                return Result.Ok(firearms);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not get Firearms: {e.Message}");
                err.CausedBy(e);
                return Result.Fail(err);
            }
        }

        static void EnrichError(Error error, Firearm firearm)
        {
            error.WithMetadata("Id", firearm.Id);
            error.WithMetadata("RowId", firearm.RowId);
        }
    }
}
