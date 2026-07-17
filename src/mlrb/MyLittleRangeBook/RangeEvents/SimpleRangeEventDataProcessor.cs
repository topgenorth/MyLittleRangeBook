using JetBrains.Annotations;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Takes the data from a simple range event and processes it
    /// </summary>
    public interface ISimpleRangeEventDataProcessor
    {
        Task<Result<MlrbId>> ProcessSimpleRangeEventData(DapperCommandContext            context,
                                                         string                          firearmName,
                                                         [ValueRange(-10000, 10000)] int roundsFired,
                                                         string                          rangeName,
                                                         string                          ammoDescription,
                                                         string                          notes,
                                                         DateOnly?                       dateOfEvent);

        Task<Result> DeleteSimpleRangeEvent(DapperCommandContext context, SimpleRangeEvent sre);
    }

    public class SimpleRangeEventDataProcessor : ISimpleRangeEventDataProcessor
    {
        readonly IFirearmAggregateRepository _faRepo;
        readonly ISimpleRangeEventService    _rangeEventService;

        public SimpleRangeEventDataProcessor(IFirearmAggregateRepository faRepo,
                                             ISimpleRangeEventService    rangeEventService)
        {
            _faRepo            = faRepo;
            _rangeEventService = rangeEventService;
        }

        public async Task<Result> DeleteSimpleRangeEvent(DapperCommandContext context, SimpleRangeEvent sre)
        {
            DateTimeOffset offsetUtc    = DateTimeOffset.UtcNow;
            MlrbId         firearmId    = MlrbId.FromString(sre.FirearmName);
            MlrbId         rangeEventId = sre.Id!;

            Result rDelete = await _rangeEventService.DeleteAsync(context, sre).ConfigureAwait(false);
            if (!rDelete.IsSuccess)
            {
                return rDelete;
            }

            Result<FirearmAggregate?> rGetFirearmAggregate =
                await _faRepo.GetAsync(context, firearmId).ConfigureAwait(false);

            FirearmAggregate fa = rGetFirearmAggregate.Value!;
            fa.FirearmRoundCountChanged(-1 * sre.RoundsFired, offsetUtc, sre.AmmoDescription);
            fa.DisassociatedWithRangeEvent(rangeEventId, offsetUtc);
            Result rSaveEventStream = await _faRepo.UpsertAsync(context, fa).ConfigureAwait(false);

            return Result.Merge(rDelete, rSaveEventStream);
        }

        public async Task<Result<MlrbId>> ProcessSimpleRangeEventData(DapperCommandContext            context,
                                                                      string                          firearmName,
                                                                      [ValueRange(-10000, 10000)] int roundsFired,
                                                                      string                          rangeName,
                                                                      string                          ammoDescription,
                                                                      string                          notes,
                                                                      DateOnly?                       dateOfEvent)
        {
            (DateOnly _, DateTimeOffset occurredUtc) = GetEventDate(dateOfEvent);
            Result<MlrbId> result = new();

            Result<SimpleRangeEvent> rUpsertRangeEvent = await UpsertSimpleRangeEvent(context,
                                                             firearmName,
                                                             roundsFired,
                                                             rangeName,
                                                             ammoDescription,
                                                             notes,
                                                             occurredUtc).ConfigureAwait(false);

            result.Reasons.AddRange(rUpsertRangeEvent.Reasons);
            MlrbId? rangeEventId = null;
            if (rUpsertRangeEvent.IsSuccess)
            {
                Success x = new("Saved the range range event that occured on " + occurredUtc.ToString("O"));
                result.Reasons.Add(x);
                rangeEventId = rUpsertRangeEvent.Value.Id!;
            }


            Result<Firearm> upsertFirearmEventStreamResult = await UpdateFirearmEventStream(context,
                                                                     firearmName,
                                                                     roundsFired,
                                                                     rangeName,
                                                                     ammoDescription,
                                                                     notes,
                                                                     occurredUtc,
                                                                     rangeEventId)
                                                                .ConfigureAwait(false);

            result = Result.Merge(result, upsertFirearmEventStreamResult);

            if (upsertFirearmEventStreamResult.IsSuccess)
            {
                result = result.WithValue(rUpsertRangeEvent.Value.Id!);
            }

            return result;
        }


        internal async Task<Result<SimpleRangeEvent>> UpsertSimpleRangeEvent(DapperCommandContext context,
                                                                             string               firearmName,
                                                                             [ValueRange(-10000, 10000)]
                                                                             int roundsFired,
                                                                             string         rangeName,
                                                                             string?        ammoDescription,
                                                                             string?        notes,
                                                                             DateTimeOffset eventDate)
        {
            SimpleRangeEvent sre = new(eventDate)
                                   {
                                       AmmoDescription = ammoDescription,
                                       FirearmName     = firearmName,
                                       RoundsFired     = roundsFired,
                                       Notes           = notes,
                                       RangeName       = rangeName,
                                   };

            Result<MlrbId> rUpsertRangeEvent = await _rangeEventService.UpsertAsync(context, sre)
                                                                       .ConfigureAwait(false);
            List<IReason> reasons = [];
            reasons.AddRange(rUpsertRangeEvent.Reasons);

            if (rUpsertRangeEvent.IsFailed)
            {
                Error err = new("Failed to upsert the range event.");
                reasons.Add(err);
            }
            else
            {
                Success success = new("Upserted the range event.");
                reasons.Add(success);
            }

            return new Result<SimpleRangeEvent>().WithValue(sre).WithReasons(reasons);
        }

        async Task<Result<Firearm>> UpdateFirearmEventStream(DapperCommandContext            context,
                                                             string                          firearmName,
                                                             [ValueRange(-10000, 10000)] int roundsFired,
                                                             string                          rangeName,
                                                             string                          ammoDescription,
                                                             string                          notes,
                                                             DateTimeOffset                  eventDateUtc,
                                                             MlrbId?                         rangeEventId)
        {
            List<IReason> reasons = [];
            Result<FirearmAggregate> rGetFirearmAggregate =
                await _faRepo.GetOrCreateByNameAsync(context, firearmName, eventDateUtc)
                             .ConfigureAwait(false);

            if (rGetFirearmAggregate.IsFailed)
            {
                return new Result().WithReasons(rGetFirearmAggregate.Reasons);
            }

            FirearmAggregate fa = rGetFirearmAggregate.Value!;
            reasons.AddRange(rGetFirearmAggregate.Reasons);

            try
            {
                fa.FirearmRoundCountChanged(roundsFired, eventDateUtc, RemoveSurroundingQuotes(ammoDescription));
                fa.AddNote("Range: " + rangeName,          eventDateUtc, null, "range_name");
                fa.AddNote(RemoveSurroundingQuotes(notes), eventDateUtc);
                if (rangeEventId is not null)
                {
                    fa.AssociateWithSimpleRangeEvent(rangeEventId.Value, eventDateUtc);
                }

                Result rUpsertFirearmAggregate = await _faRepo.UpsertAsync(context, fa).ConfigureAwait(false);
                reasons.AddRange(rUpsertFirearmAggregate.Reasons);

                return new Result().WithReasons(reasons).ToResult(fa.ToFirearm());
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToError());
            }
        }

        static (DateOnly eventDateOnly, DateTimeOffset occurredUtc) GetEventDate(DateOnly? eventDate)
        {
            DateOnly dateOnly;
            if (eventDate is null)
            {
                DateTime d = DateTime.Now;
                dateOnly = DateOnly.FromDateTime(d);
            }
            else
            {
                dateOnly = eventDate.Value;
            }

            DateTime       localDateTime = dateOnly.ToDateTime(TimeOnly.FromDateTime(DateTime.Now));
            DateTimeOffset occuredUtc    = new DateTimeOffset(localDateTime).ToUniversalTime();


            return (dateOnly, occuredUtc);
        }

        /// <summary>
        ///     Will strip the double quotes from the start and end of to the string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static string RemoveSurroundingQuotes(string value) =>
            value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"')
                ? value[1..^1]
                : value;
    }
}