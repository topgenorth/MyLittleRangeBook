using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook
{
    public partial class UpdateFirearmsFromRangeEventsCommand
    {
        [Command("rebuild-from-range-events"), UsedImplicitly]
        public async Task<int> RebuildFirearmsFromSimpleRangeEvents(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Rebuilding firearms from range events.");
            int returnCode = ReturnCodes.FAILURE;

            List<IReason> reasons = new();
            await using DapperCommandContext context = await DapperCommandContext
                                                            .NewAsync(SqliteHelper, cancellationToken, true)
                                                            .ConfigureAwait(false);
            try
            {
                IEnumerable<FirearmNameFromSimpleRangeEventRow> firearmNames =
                    await Commands.s_getFirearmNamesFromRangeEvents
                                  .QueryAsync<FirearmNameFromSimpleRangeEventRow>(context)
                                  .ConfigureAwait(false);
                foreach (var f in firearmNames)
                {
                    var events = await _eventSourcingService.GetDomainEvents(context, f.FirearmId).ConfigureAwait(false);
                    if (!events.Any())
                    {
                        Logger.Debug("No events for firearm {0}", f);
                    }
                    else
                    {
                        Logger.Debug("There are {0} events for firearm {1}.", events.Count(), f.FirearmId);
                    }
                }

                await context.CommitAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await context.RollbackAsync().ConfigureAwait(false);
                Error err = ex.ToError("Unexpected exception trying to rebuild the firearms table.");
                reasons.Add(err);
            }

            PressEnterToContinue();
            return returnCode;
        }
    }
}