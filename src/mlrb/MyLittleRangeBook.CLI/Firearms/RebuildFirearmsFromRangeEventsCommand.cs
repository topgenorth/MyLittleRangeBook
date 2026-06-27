using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook
{
    public partial class UpdateFirearmsFromRangeEventsCommand
    {
        [Command("rebuild-from-range-events")]
        [UsedImplicitly]
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
                Result<IEnumerable<SimpleRangeEvent>> rangeEvents =
                    await _simpleRangeEventRepository.GetSimpleRangeEventsAsync(context).ConfigureAwait(false);

                IEnumerable<IGrouping<string, SimpleRangeEvent>>? rangeEventsByFirearm = rangeEvents.Value?
                   .OrderBy(e => e.FirearmName)
                   .ThenBy(e => e.EventDate)
                   .GroupBy(e => e.FirearmName);

                foreach (IGrouping<string, SimpleRangeEvent> x in rangeEventsByFirearm)
                {
                    CliDisplay.PrintInfo($"Processing range events for firearm {x.Key}");
                    foreach (SimpleRangeEvent sre in x)
                    {
                        Result r = await _simpleRangeEventProcessor.RebuildFirearmAggregate(context, sre)
                                                                   .ConfigureAwait(false);
                        reasons.AddRange(r.Reasons);
                    }

                    if (!reasons.OfType<Error>().Any())
                    {
                        continue;
                    }

                    foreach (var e in reasons)
                    {
                        Logger.Warning("{0}: {1}", x.Key, e.Message);
                    }
                    CliDisplay.PrintFailure($"Failed to process range events for firearm {x.Key}.");

                }

                await context.RollbackAsync().ConfigureAwait(false);

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