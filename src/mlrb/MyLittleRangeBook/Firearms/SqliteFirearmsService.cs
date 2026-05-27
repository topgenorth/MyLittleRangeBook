using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public class SqliteFirearmsService : IFirearmsService
    {
        const string SelectSql = "SELECT * FROM Firearms ORDER BY Name;";
        const string SelectByIdSql = "SELECT * FROM Firearms WHERE Id=@Id;";
        const string SelectActiveSql = "SELECT * FROM Firearms WHERE IsActive=1 ORDER BY Name;";
        const string DeleteSql = "DELETE FROM Firearm WHERE Id = @Id";

        const string InsertSql = """
                                 INSERT INTO Firearms (Id, Name, Notes) 
                                 VALUES (@Id, @Name, @Notes) 
                                 RETURNING RowId
                                 """;

        const string UpdateSql = """
                                 INSERT INTO Firearms (Id,Name, Notes) 
                                 VALUES (@Id, @Name, @Notes) 
                                 ON CONFLICT(Name) DO UPDATE SET Notes = @Notes, Modified = utcnow()
                                 RETURNING RowId
                                 """;

        public async Task<Result<bool>> DeleteAsync(IDbConnection connection,
            Firearm firearm,
            CancellationToken cancellationToken = default)
        {
            if (firearm.RowId is null)
            {
                Success reason = new Success($"Nothing to delete; firearm `{firearm.Id}` does not exist.")
                    .Enrich(firearm.Id!, firearm.RowId);

                return Result.Ok().WithSuccess(reason);
            }

            try
            {
                var cmd = new SqliteCommand(DeleteSql, (SqliteConnection)connection);
                cmd.Parameters.AddWithValue("@Id", firearm.Id);
                cmd.CommandType = CommandType.Text;
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Error err = new Error($"Unexpected error trying to delete `{firearm.Id}`: {e.Message}")
                    .CausedBy(e)
                    .Enrich(firearm.Id!, firearm.RowId);

                return Result.Fail(err);
            }

            return Result.Ok(true);
        }

        public async Task<Result<EntityId>> UpsertAsync(Firearm firearm,
            IDbConnection connection,
            CancellationToken cancellationToken = default)
        {
            var valuesToInsert = new { firearm.Id, firearm.Name, firearm.Notes };

            try
            {
                string sql = firearm.RowId is null ? InsertSql : UpdateSql;
                var command = new CommandDefinition(sql, valuesToInsert, cancellationToken: cancellationToken);
                long l = await connection.ExecuteScalarAsync<long>(command);
                var upsertId = new EntityId(firearm.Id!, l);

                firearm.RowId = l;
                var success = new Success($"Firearm `{firearm.Id}` saved.");
                success.WithMetadata("Id", upsertId.Id);
                success.WithMetadata("RowId", upsertId.RowId);

                // TODO [TO20260505] There is a subtle bug here:  the Modified date isn't changed when we update
                return new Result<EntityId>().WithValue(upsertId).WithSuccess(success);
            }
            catch (Exception e)
            {
                IError err = new Error($"Could not save Firearm `{firearm.Id}`: {e.Message}")
                    .Enrich(firearm.Id!, firearm.RowId)
                    .CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result<IEnumerable<Firearm>>> GetFirearmsAsync(
            IDbConnection connection,
            bool activeOnly = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string sql = activeOnly ? SelectActiveSql : SelectSql;
                IEnumerable<Firearm> firearms = await connection.QueryAsync<Firearm>(sql, cancellationToken);

                return Result.Ok(firearms);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not get Firearms: {e.Message}");
                err.CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result<Firearm>> GetFirearmAsync(string id,
            IDbConnection connection,
            CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(SelectByIdSql, new { Id = id }, cancellationToken: cancellationToken);
            Firearm? f = await connection.QueryFirstOrDefaultAsync<Firearm>(command);

            return f is null
                ? Result.Fail<Firearm>(new Error($"Firearm with id `{id}` not found").Enrich(id, null))
                : Result.Ok(f);
        }

        public Task<Result> AssociateAssetWithFirearm(string firearmId,
            string assetId,
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
