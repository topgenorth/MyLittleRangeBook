using System.Data;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    public class FirearmsService : IFirearmsService
    {
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
                await Commands.DeleteById
                    .Arguments(new { firearm.Id })
                    .ExecuteAsync(connection, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
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
                // TODO [TO20260505] There is a subtle bug here:  the Modified date isn't changed when we update
                // TODO [TO20260528] Change this to use a single UPSERT command instead of separate INSERT and UPDATE commands.
                DapperCommand cmd = firearm.RowId is null
                    ? Commands.Insert.Arguments(valuesToInsert)
                    : Commands.Update.Arguments(valuesToInsert);
                long l = await cmd.ExecuteScalarAsync<long>(connection, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                firearm.RowId = l;

                var upsertId = new EntityId(firearm.Id!, l);

                return new Result<EntityId>().WithValue(upsertId);
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
                DapperCommand cmd = activeOnly ? Commands.SelectActive : Commands.SelectAll;

                IEnumerable<Firearm> firearms = await cmd
                    .QueryAsync<Firearm>(connection, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

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
            DapperCommand cmd = Commands.SelectById.Arguments(new { Id = id });
            Firearm? f = await cmd
                .QuerySingleAsync<Firearm?>(connection, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

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

        public Task<Result> UpdateFirearmsFromRangeEventsAsync(DapperCommandContext ctx)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The SQL and commands we can perform on the database
        /// </summary>
        public static class Commands
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

            public static DapperCommand SelectAll => new(SelectSql);
            public static DapperCommand SelectActive => new(SelectActiveSql);
            public static DapperCommand SelectById => new(SelectByIdSql);
            public static DapperCommand DeleteById => new(DeleteSql);
            public static DapperCommand Insert => new(InsertSql);
            public static DapperCommand Update => new(UpdateSql);
        }
    }
}
