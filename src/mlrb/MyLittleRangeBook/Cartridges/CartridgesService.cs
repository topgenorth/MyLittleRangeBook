using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Cartridges
{
    public class CartridgesService : ICartridgesService
    {
        const string SelectSql = "SELECT * FROM Cartridges ORDER BY Name;";
        const string SelectByIdSql = "SELECT * FROM Cartridges WHERE Id=@Id;";
        const string SelectActiveSql = "SELECT * FROM Cartridges WHERE IsActive=1 ORDER BY Name;";
        const string DeleteSql = "DELETE FROM Cartridges WHERE Id = @Id";

        // const string InsertSql = """
        //                          INSERT INTO Cartridges (Id,
        //                                                  Name,
        //                                                  CommonName,
        //                                                  ProjectileDiameterMetric,
        //                                                  ProjectileDiameterImperial,
        //                                                  SuitableForRifle,
        //                                                  SuitableForPistol,
        //                                                  IsActive)
        //                          VALUES (@Id, @Name, @CommonName, @ProjectileDiameterMetric, @ProjectileDiameterImperial, @SuitableForRifle, @SuitableForPistol, @IsActive)
        //                          RETURNING RowId
        //                          """;

        const string UpsertSql = """
                                 INSERT INTO Cartridges (Id, 
                                                         Name, 
                                                         CommonName, 
                                                         ProjectileDiameterMetric, 
                                                         ProjectileDiameterImperial, 
                                                         SuitableForRifle, 
                                                         SuitableForPistol, 
                                                         IsActive) 
                                 VALUES (@Id, @Name, @CommonName, @ProjectileDiameterMetric, @ProjectileDiameterImperial, @SuitableForRifle, @SuitableForPistol, @IsActive) 
                                 ON CONFLICT(Name) DO 
                                     UPDATE SET CommonName = @CommonName, 
                                     ProjectileDiameterMetric = @ProjectileDiameterMetric, 
                                     ProjectileDiameterImperial = @ProjectileDiameterImperial, 
                                     SuitableForRifle = @SuitableForRifle, 
                                     SuitableForPistol = @SuitableForPistol, 
                                     IsActive = @IsActive, 
                                     Modified = utcnow()
                                 RETURNING RowId
                                 """;

        public async Task<Result<bool>> DeleteAsync(IDbConnection connection,
            Cartridge cartridge,
            CancellationToken cancellationToken = default)
        {
            if (cartridge.RowId is null)
            {
                Success reason = new Success($"Nothing to delete; cartridge `{cartridge.Id}` does not exist.")
                    .Enrich(cartridge.Id!, cartridge.RowId);

                return Result.Ok().WithSuccess(reason);
            }

            try
            {
                var cmd = new SqliteCommand(DeleteSql, (SqliteConnection)connection);
                cmd.Parameters.AddWithValue("@Id", cartridge.Id);
                cmd.CommandType = CommandType.Text;
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception e)
            {
                Error err = new Error($"Unexpected error trying to delete `{cartridge.Id}`: {e.Message}")
                    .CausedBy(e)
                    .Enrich(cartridge.Id!, cartridge.RowId);

                return Result.Fail(err);
            }

            return Result.Ok(true);
        }

        public async Task<Result<EntityId>> UpsertAsync(IDbConnection connection,
            Cartridge cartridge,
            CancellationToken cancellationToken = default)
        {
            var valuesToInsert = new
            {
                cartridge.Id,
                cartridge.Name,
                cartridge.CommonName,
                cartridge.ProjectileDiameterMetric,
                cartridge.ProjectileDiameterImperial,
                cartridge.SuitableForRifle,
                cartridge.SuitableForPistol,
                cartridge.IsActive
            };
            var ctx = new DapperCommandContext(connection, null, cancellationToken, valuesToInsert);
            var cmd = new DapperCommand(UpsertSql);
            try
            {
                long l = await cmd.ExecuteScalarAsync<long>(ctx)
                    .ConfigureAwait(false);

                var upsertId = new EntityId(cartridge.Id!, l);
                cartridge.RowId = l;

                var success = new Success($"Cartridge `{cartridge.Id}` saved.");
                success.WithMetadata("Id", upsertId.Id);
                success.WithMetadata("RowId", upsertId.RowId);

                return new Result<EntityId>().WithValue(upsertId).WithSuccess(success);
            }
            catch (Exception e)
            {
                IError err = new Error($"Could not save Cartridge `{cartridge.Id}`: {e.Message}")
                    .Enrich(cartridge.Id!, cartridge.RowId)
                    .CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result<IEnumerable<Cartridge>>> GetCartridgesAsync(
            IDbConnection connection,
            bool activeOnly = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string sql = activeOnly ? SelectActiveSql : SelectSql;
                IEnumerable<Cartridge> cartridges = await connection.QueryAsync<Cartridge>(sql, cancellationToken);

                return Result.Ok(cartridges);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not get Cartridges: {e.Message}");
                err.CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result<Cartridge>> GetCartridgeAsync(IDbConnection connection,
            string id,
            CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(SelectByIdSql, new { Id = id }, cancellationToken: cancellationToken);
            Cartridge? c = await connection.QueryFirstOrDefaultAsync<Cartridge>(command);

            return c is null
                ? Result.Fail<Cartridge>(new Error($"Cartridge with id `{id}` not found").Enrich(id, null))
                : Result.Ok(c);
        }
    }
}
