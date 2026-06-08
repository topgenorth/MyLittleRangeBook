using System.Data;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Cartridges
{
    public class CartridgesService : ICartridgesService
    {
        public async Task<Result> DeleteAsync(IDbConnection connection,
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
                if (cartridge.Id != null || cartridge.Id != MlrbId.Empty)
                {
                    var ctx = new DapperCommandContext(connection, CancellationToken: cancellationToken,
                        Arguments: new { cartridge.Id });
                    await Commands.DeleteCommand.ExecuteAsync(ctx).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Error err = new Error($"Unexpected error trying to delete `{cartridge.Id}`: {e.Message}")
                    .CausedBy(e)
                    .Enrich(cartridge.Id!, cartridge.RowId);

                return Result.Fail(err);
            }

            return Result.Ok();
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
            DapperCommand cmd = Commands.UpsertCommand;
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
                var ctx = new DapperCommandContext(connection, CancellationToken: cancellationToken);
                DapperCommand cmd = activeOnly ? Commands.SelectActiveCommand : Commands.SelectCommand;
                IEnumerable<Cartridge> cartridges = await cmd.QueryAsync<Cartridge>(ctx).ConfigureAwait(false);

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
            var ctx = new DapperCommandContext(connection, Arguments: new { Id = id },
                CancellationToken: cancellationToken);
            Cartridge? c = await Commands.SelectByIdCommand.QuerySingleOrDefaultAsync<Cartridge>(ctx)
                .ConfigureAwait(false);

            return c is null
                ? Result.Fail<Cartridge>(new Error($"Cartridge with id `{id}` not found").Enrich(id, null))
                : Result.Ok(c);
        }

         static class Commands
        {
            const string DeleteSql = "DELETE FROM cartridges WHERE Id = @Id";

            const string SelectSql = """
                                     SELECT
                                         row_id As RowId,
                                         id AS Id,
                                         name AS Name, 
                                         common_name AS CommonName,
                                         diameter_metric as ProjectileDiameterMetric,
                                         diameter_imperial as ProjectileDiameterImperial,
                                         suitable_for_rifle as SuitableForRifle,
                                         suitable_for_pistol as SuitableForPistol,
                                         is_active as IsActive,
                                         created AS Created,
                                         modified AS Modified
                                     FROM cartridges 
                                     ORDER BY name;
                                     """;

            const string SelectByIdSql = """
                                         SELECT
                                             row_id As RowId,
                                             id AS Id,
                                             name AS Name, 
                                             common_name AS CommonName,
                                             diameter_metric as ProjectileDiameterMetric,
                                             diameter_imperial as ProjectileDiameterImperial,
                                             suitable_for_rifle as SuitableForRifle,
                                             suitable_for_pistol as SuitableForPistol,
                                             is_active as IsActive,
                                             created AS Created,
                                             modified AS Modified
                                         FROM cartridges 
                                         WHERE id=@Id
                                         ORDER BY name;
                                         """;

            const string SelectActiveSql = """
                                           SELECT
                                               row_id As RowId,
                                               id AS Id,
                                               name AS Name, 
                                               common_name AS CommonName,
                                               diameter_metric as ProjectileDiameterMetric,
                                               diameter_imperial as ProjectileDiameterImperial,
                                               suitable_for_rifle as SuitableForRifle,
                                               suitable_for_pistol as SuitableForPistol,
                                               is_active as IsActive,
                                               created AS Created,
                                               modified AS Modified
                                           FROM cartridges 
                                           WHERE is_active=1 
                                           ORDER BY name;
                                           """;

            const string UpsertSql = """
                                     INSERT INTO cartridges (id , 
                                                             name , 
                                                             common_name, 
                                                             diameter_metric, 
                                                             diameter_imperial, 
                                                             suitable_for_rifle, 
                                                             suitable_for_pistol, 
                                                             is_active) 
                                     VALUES (@Id, @Name, @CommonName, @ProjectileDiameterMetric, @ProjectileDiameterImperial, @SuitableForRifle, @SuitableForPistol, @IsActive) 
                                     ON CONFLICT(name) DO 
                                         UPDATE SET common_name = @CommonName, 
                                         diameter_metric = @ProjectileDiameterMetric, 
                                         diameter_imperial = @ProjectileDiameterImperial, 
                                         suitable_for_rifle = @SuitableForRifle, 
                                         suitable_for_pistol = @SuitableForPistol, 
                                         is_active = @IsActive, 
                                         modified = utcnow()
                                     RETURNING row_id
                                     """;

            internal static readonly DapperCommand DeleteCommand = new(DeleteSql);
            internal static readonly DapperCommand UpsertCommand = new(UpsertSql);
            internal static readonly DapperCommand SelectActiveCommand = new(SelectActiveSql);
            internal static readonly DapperCommand SelectCommand = new(SelectSql);
            internal static readonly DapperCommand SelectByIdCommand = new(SelectByIdSql);
        }
    }
}
