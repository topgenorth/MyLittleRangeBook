using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.EventSourcing;
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

            var processor = new SimpleRangeEventProcessor();
            try
            {
                Result<IEnumerable<SimpleRangeEvent>> rangeEvents =
                    await _simpleRangeEventRepository.GetSimpleRangeEventsAsync(context).ConfigureAwait(false);

                IEnumerable<IGrouping<string, SimpleRangeEvent>>? orderedRangeEvents = rangeEvents.Value
                  ?.OrderBy(e => e.FirearmName).ThenBy(e => e.EventDate).GroupBy(e => e.FirearmName);

                foreach (IGrouping<string, SimpleRangeEvent> x in orderedRangeEvents)
                {
                    CliDisplay.PrintInfo($"Processing range events for firearm {x.Key}");
                    var domainEvents = x.SelectMany(processor.ToFirearmDomainEvents).ToList();

                    Logger.Verbose($"Found {domainEvents.Count} domain events for firearm {x.Key}");
                }

                await context.CommitAsync().ConfigureAwait(false);
                CliDisplay.PrintSuccess("Rebuild the firearms table.");
            }
            catch (Exception ex)
            {
                await context.RollbackAsync().ConfigureAwait(false);
                Logger.Error(ex, "There was an unexpected exception tying to rebuild the firearms table");
                CliDisplay.PrintFailure("There was an unexpected exception tying to rebuild the firearms table");
                Error err = ex.ToError("Unexpected exception trying to rebuild the firearms table.");
                reasons.Add(err);
            }

            PressEnterToContinue();
            return returnCode;
        }
    }
}