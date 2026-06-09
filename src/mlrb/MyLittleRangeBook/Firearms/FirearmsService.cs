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
            DapperCommandContext ctx = context with
            {
                Arguments =
                new { firearm.Id, firearm.Name, firearm.Notes, firearm.RoundsFired }
            };

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


        async Task<Result> AssociateRangeEventsWithFirearm(DapperCommandContext ctx)
        {
            try
            {
                IEnumerable<Commands.RangeEventForFirearm> rangeEvents = await Commands.RangeEventsByFirearmName
                    .QueryAsync<Commands.RangeEventForFirearm>(ctx)
                    .ConfigureAwait(false);

                var reasons = new List<IReason>();
                foreach (Commands.RangeEventForFirearm row in rangeEvents)
                {
                    try
                    {
                        DapperCommandContext c = ctx with
                        {
                            Arguments = new { FirearmsId = row.FirearmName, SimpleRangeEventId = row.SimpleRangeId }
                        };
                        int l = await Commands.AssociateFirearmWithRangeEvent.ExecuteAsync(c).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        var reason = new Success(
                            $"Failed to associate firearm {row.FirearmName} with range event {row.SimpleRangeId}: {e.Message}");
                        reasons.Add(reason);
                    }
                }

                return Result.Ok().WithReasons(reasons);
            }
            catch (Exception e)
            {
                Error? err = new Error(e.Message).CausedBy(e);

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
                                     INSERT INTO Firearms (Id,Name, Notes, Modified, Created, RoundsFired) 
                                     VALUES (@Id, @Name, @Notes, utcnow(), utcnow(), @RoundsFired) 
                                     ON CONFLICT(Name) DO UPDATE SET Notes = @Notes, Modified = utcnow(), RoundsFired=@RoundsFired
                                     RETURNING RowId
                                     """;

            const string AssociateFirearmWithRangeEventSql = """
                                                             INSERT INTO firearms_simple_range_events (firearm_id, simple_range_event_id) 
                                                             VALUES (@FirearmsId, @SimpleRangeEventId);
                                                             """;


            const string RangeEventsByFirearmNameSql = """
                                                        SELECT 
                                                            s.firearm_name AS FirearmName,
                                                            s.id AS SimpleRangeEventId
                                                        FROM simple_range_events AS s
                                                        LEFT JOIN Firearms AS f ON f.Name = s.firearm_name
                                                        WHERE f.Name IS NULL
                                                        ORDER BY s.firearm_name;                                                       
                                                       """;



            internal static readonly DapperCommand AssociateFirearmWithRangeEvent =
                new(AssociateFirearmWithRangeEventSql);

            internal static readonly DapperCommand RangeEventsByFirearmName = new(RangeEventsByFirearmNameSql);
            internal static DapperCommand SelectAll => new(SelectSql);
            internal static DapperCommand SelectActive => new(SelectActiveSql);
            internal static DapperCommand SelectById => new(SelectByIdSql);
            internal static DapperCommand DeleteById => new(DeleteSql);
            internal static DapperCommand Upsert => new(UpsertSql);

            internal record struct RangeEventForFirearm(string FirearmName, string SimpleRangeId);

        }
    }
}
