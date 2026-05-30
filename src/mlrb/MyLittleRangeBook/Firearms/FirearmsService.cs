using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    public class FirearmsService : IFirearmsService
    {
        public async Task<Result<bool>> DeleteAsync(DapperCommandContext context, Firearm firearm)
        {
            if (firearm.RowId is null)
            {
                Success reason = new Success($"Nothing to delete; firearm `{firearm.Id}` does not exist.")
                    .Enrich(firearm.Id!, firearm.RowId);

                return Result.Ok().WithSuccess(reason);
            }

            try
            {
                DapperCommandContext ctx = context with { Arguments = new { firearm.Id } };
                await Commands.DeleteById
                    .ExecuteAsync(ctx)
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

        public async Task<Result<EntityId>> UpsertAsync(DapperCommandContext context, Firearm firearm)
        {
            DapperCommandContext ctx = context with { Arguments = new { firearm.Id, firearm.Name, firearm.Notes } };

            try
            {
                long l = await Commands.Upsert.ExecuteScalarAsync<long>(ctx).ConfigureAwait(false);
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
            DapperCommandContext context,
            bool activeOnly = true)
        {
            try
            {
                DapperCommand cmd = activeOnly ? Commands.SelectActive : Commands.SelectAll;

                IEnumerable<Firearm> firearms = await cmd.QueryAsync<Firearm>(context).ConfigureAwait(false);

                return Result.Ok(firearms);
            }
            catch (Exception e)
            {
                var err = new Error($"Could not get Firearms: {e.Message}");
                err.CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result<Firearm>> GetFirearmAsync(DapperCommandContext context, string id)
        {
            DapperCommand cmd = Commands.SelectById;
            DapperCommandContext ctx = context with { Arguments = new { Id = id } };
            Firearm? f = await cmd.QuerySingleAsync<Firearm?>(ctx).ConfigureAwait(false);

            return f is null
                ? Result.Fail<Firearm>(new Error($"Firearm with id `{id}` not found").Enrich(id, null))
                : Result.Ok(f);
        }

        public Task<Result> AssociateAssetWithFirearm(DapperCommandContext context, string firearmId, string assetId)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> UpdateFirearmsFromRangeEventsAsync(DapperCommandContext context)
        {
            try
            {
                IEnumerable<string> firearms = await Commands.GetNewFirearmsFromRangeEvents
                    .QueryAsync<string>(context)
                    .ConfigureAwait(false);

                var result = Result.Ok();
                foreach (string firearmName in firearms)
                {
                    var f = new Firearm(firearmName);
                    Result<EntityId> l = await UpsertAsync(context, f).ConfigureAwait(false);
                    if (l.IsFailed)
                    {
                        // [TO20260529] we don't want to fail things because we couldn't add a named firearm, just pass it on as an IReason.
                        result.Reasons.Add(new Success($"Failed to add firearm {firearmName} from SimpleRangeEvents."));
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                var err = new Error($"Could not update Firearms from Range Events: {e.Message}");
                err.CausedBy(e);

                return Result.Fail(err);
            }
        }

        /// <summary>
        ///     The SQL and commands we can perform on the database
        /// </summary>
        public static class Commands
        {
            const string SelectSql = "SELECT * FROM Firearms ORDER BY Name;";
            const string SelectByIdSql = "SELECT * FROM Firearms WHERE Id=@Id;";
            const string SelectActiveSql = "SELECT * FROM Firearms WHERE IsActive=1 ORDER BY Name;";
            const string DeleteSql = "DELETE FROM Firearms WHERE Id = @Id";

            const string UpsertSql = """
                                     INSERT INTO Firearms (Id,Name, Notes, Modified, Created) 
                                     VALUES (@Id, @Name, @Notes, utcnow(), utcnow()) 
                                     ON CONFLICT(Name) DO UPDATE SET Notes = @Notes, Modified = utcnow()
                                     RETURNING RowId
                                     """;

            const string GetNewFirearmNamesFromRangeEventsSql = """
                                                                SELECT DISTINCT SimpleRangeEvents.FirearmName 
                                                                FROM SimpleRangeEvents 
                                                                    WHERE SimpleRangeEvents.FirearmName NOT IN (SELECT Name FROM Firearms)
                                                                ORDER BY SimpleRangeEvents.FirearmName; 
                                                                """;

            internal static DapperCommand GetNewFirearmsFromRangeEvents = new(GetNewFirearmNamesFromRangeEventsSql);
            public static DapperCommand SelectAll => new(SelectSql);
            public static DapperCommand SelectActive => new(SelectActiveSql);
            public static DapperCommand SelectById => new(SelectByIdSql);
            public static DapperCommand DeleteById => new(DeleteSql);
            public static DapperCommand Upsert => new(UpsertSql);
        }
    }
}
