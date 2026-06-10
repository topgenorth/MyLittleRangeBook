using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmsService : IFirearmsService
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

        public async Task<Result<EntityId>> UpsertAsync(DapperCommandContext context, FirearmAggregate firearmAggregate)
        {
            // [TO20260610] the aggregate id (stream id) is the same as the firearm id.
            Result<Firearm> fResult = await GetFirearmAsync(context, firearmAggregate.Id.ToString()).ConfigureAwait(false);

            Firearm f = fResult.IsFailed ? firearmAggregate.ToFirearm() : fResult.Value;

            f.RoundsFired = firearmAggregate.RoundsFired;
            f.Notes = firearmAggregate.Notes;
            return await UpsertAsync(context, f).ConfigureAwait(false);
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
            try
            {
                Firearm? f = await cmd.QuerySingleAsync<Firearm?>(ctx).ConfigureAwait(false);

                return f is null
                    ? Result.Fail<Firearm>(new Error($"Firearm with id `{id}` not found").Enrich(id, null))
                    : Result.Ok(f);
            }
            catch (InvalidOperationException ioex)
            {
                // [TO20260610] This probably means that the firearm doesn't exist.
                Error? err = new Error($"Firearm with id `{id}` not found.").Enrich(id, null).CausedBy(ioex);
                return Result.Fail<Firearm>(err);
            }
            catch (Exception e)
            {
                Error? err = new Error($"Unexpected error trying to retrieve firearm  id `{id}`.").Enrich(id, null).CausedBy(e);
                return Result.Fail<Firearm>(err);
            }
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



    }
}
