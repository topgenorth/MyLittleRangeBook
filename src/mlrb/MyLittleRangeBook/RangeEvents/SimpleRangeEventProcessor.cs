using System.Text.Json.Nodes;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    public interface ISimpleRangeEventProcessor
    {
        Task<Result> RebuildFirearmAggregate(DapperCommandContext context,
                                             SimpleRangeEvent     simpleRangeEvent);

        IReadOnlyList<IDomainEvent> ToFirearmDomainEvents(SimpleRangeEvent sre);
    }

    public class SimpleRangeEventProcessor : ISimpleRangeEventProcessor
    {
        readonly IFirearmAggregateRepository _faRepo;
        ILogger                              _logger;

        public SimpleRangeEventProcessor(IFirearmAggregateRepository faRepo, ILogger logger)
        {
            _faRepo = faRepo;
            _logger = logger;
        }

        public async Task<Result> RebuildFirearmAggregate(DapperCommandContext context,
                                                          SimpleRangeEvent     simpleRangeEvent)
        {
            List<IReason> reason = [];
            try
            {
                Result<FirearmAggregate> getResult = await _faRepo
                                                          .GetOrCreateByNameAsync(context, simpleRangeEvent.FirearmName,
                                                               simpleRangeEvent.OccurredUtc)
                                                          .ConfigureAwait(false);
                if (getResult.IsFailed)
                {
                    return Result.Fail(getResult.Errors);
                }

                FirearmAggregate          fa           = getResult.Value!;
                IReadOnlyList<IDomainEvent> domainEvents = ToFirearmDomainEvents(simpleRangeEvent);
                domainEvents.ToList().ForEach(fa.Raise);

                Result r2 = await _faRepo.UpsertAsync(context, fa).ConfigureAwait(false);
                return r2.IsFailed ? Result.Fail(r2.Errors) : Result.Ok();
            }
            catch (Exception e)
            {
                Error err = e.ToError("Unexpected exception trying to rebuild the firearm aggregate.")
                             .Enrich(MlrbId.FromString(simpleRangeEvent.FirearmName));
                return Result.Fail(err);
            }
        }


        public IReadOnlyList<IDomainEvent> ToFirearmDomainEvents(SimpleRangeEvent sre)
        {
            string metaDataJson = new JsonObject { ["simple_range_event_id"] = sre.Id! }.ToJsonString();


            FirearmAggregate fa = FirearmAggregate.New(sre.FirearmName, sre.OccurredUtc);
            fa.AssociateWithSimpleRangeEvent(sre.Id!, sre.OccurredUtc);
            fa.MoreRoundsFired(sre.RoundsFired, sre.OccurredUtc, metaDataJson);

            if (!string.IsNullOrWhiteSpace(sre.Notes))
            {
                fa.AppendToNotes(sre.Notes, sre.OccurredUtc, metaDataJson);
            }

            if (!string.IsNullOrWhiteSpace(sre.AmmoDescription))
            {
                fa.AppendToNotes(sre.AmmoDescription, sre.OccurredUtc, metaDataJson);
            }

            IEnumerable<IDomainEvent> events =
                fa.DequeueUncommittedEvents().Where(e => e is not FirearmAggregate.FirearmCreated);
            return events.ToArray();
        }
    }
}