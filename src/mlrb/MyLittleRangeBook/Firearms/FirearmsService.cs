using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Firearms
{
    public class FirearmDoesNotExistError : Error
    {
        public FirearmDoesNotExistError(string mlrbId)
            : base($"Firearm with id `{mlrbId}` was not found")
        {
            Metadata.Add("MlrbId", mlrbId);
        }
    }

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
                await Commands.s_deleteById
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
                                           new
                                           {
                                               firearm.Id,
                                               firearm.Name,
                                               firearm.Notes,
                                               firearm.RoundsFired,
                                               Modified = DateTimeOffset.UtcNow,
                                               firearm.Created,
                                           },
                                       };

            try
            {
                long l = await Commands.s_upsert.ExecuteScalarAsync<long>(ctx).ConfigureAwait(false);
                firearm.RowId = l;
                EntityId upsertId = new(firearm.Id!, l);

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
            Firearm f = firearmAggregate.ToFirearm();
            f.Modified = DateTimeOffset.UtcNow;
            return await UpsertAsync(context, f).ConfigureAwait(false);
        }


        public async Task<Result<IEnumerable<Firearm>>> GetFirearmsAsync(
            DapperCommandContext context,
            bool                 activeOnly = true)
        {
            try
            {
                DapperCommand cmd = activeOnly ? Commands.s_selectActive : Commands.s_selectAll;

                IEnumerable<Firearm> firearms = await cmd.QueryAsync<Firearm>(context).ConfigureAwait(false);

                return Result.Ok(firearms);
            }
            catch (Exception e)
            {
                Error err = new($"Could not get Firearms: {e.Message}");
                err.CausedBy(e);

                return Result.Fail(err);
            }
        }

        /// <summary>
        ///     Retrieve the firearm record from the database.  Task
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <returns>
        ///     A successful result with the Firearm object. If there is an error or the firearm does not exist, a failed
        ///     result is returned. If the firearm does not exist then the result will have an <c cref="FirearmDoesNotExistError" />.
        /// </returns>
        public async Task<Result<Firearm>> GetFirearmAsync(DapperCommandContext context, string id)
        {
            DapperCommand        cmd = Commands.s_selectById;
            DapperCommandContext ctx = context with { Arguments = new { Id = id } };
            try
            {
                Firearm? f = await cmd.QuerySingleAsync<Firearm?>(ctx).ConfigureAwait(false);

                return f is null
                           ? Result.Fail<Firearm>(new FirearmDoesNotExistError(id))
                           : Result.Ok(f);
            }
            catch (InvalidOperationException ioex)
            {
                var err = new FirearmDoesNotExistError(id).CausedBy(ioex);
                return Result.Fail<Firearm>(err);
            }
            catch (Exception e)
            {
                Error er = e.ToError().Enrich(id, null);
                return Result.Fail<Firearm>(er);
            }
        }
    }
}