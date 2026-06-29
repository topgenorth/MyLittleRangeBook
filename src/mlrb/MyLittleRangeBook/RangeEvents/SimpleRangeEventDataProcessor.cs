using JetBrains.Annotations;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    public interface ISimpleRangeEventDataProcessor
    {
        Task<Result> ProcessSimpleRangeEventData(DapperCommandContext       context,
                                                 string                     firearmName,
                                                 [ValueRange(0, 10000)] int roundsFired,
                                                 string                     rangeName,
                                                 string                     ammoDescription,
                                                 string                     notes,
                                                 DateOnly?                  dateOfEvent);
    }

    public class SimpleRangeEventDataProcessor : ISimpleRangeEventDataProcessor
    {
        readonly IFirearmAggregateRepository _faRepo;
        readonly IFirearmsService            _firearmsService;
        readonly ISimpleRangeEventService    _rangeEventService;

        public SimpleRangeEventDataProcessor(IFirearmAggregateRepository faRepo,
                                             ISimpleRangeEventService    rangeEventService,
                                             IFirearmsService            firearmsService)
        {
            _faRepo            = faRepo;
            _rangeEventService = rangeEventService;
            _firearmsService   = firearmsService;
        }

        public async Task<Result> ProcessSimpleRangeEventData(DapperCommandContext       context,
                                                              string                     firearmName,
                                                              [ValueRange(0, 10000)] int roundsFired,
                                                              string                     rangeName,
                                                              string                     ammoDescription,
                                                              string                     notes,
                                                              DateOnly?                  dateOfEvent)
        {
            List<IReason> reasons = [];
            (DateOnly eventDate, DateTimeOffset occurredUtc) = GetEventDate(dateOfEvent);
            Result<Firearm> r1 = await UpdateFirearmEventStream(context, firearmName, roundsFired, rangeName,
                                                                ammoDescription,
                                                                notes,
                                                                occurredUtc)
                                    .ConfigureAwait(false);
            reasons.AddRange(r1.Reasons);

            if (r1.IsSuccess)
            {
                Result<SimpleRangeEvent> r2 = await UpsertSimpleRangeEvent(context,
                                                                           firearmName,
                                                                           roundsFired,
                                                                           rangeName,
                                                                           ammoDescription,
                                                                           notes,
                                                                           occurredUtc).ConfigureAwait(false);
                reasons.AddRange(r2.Reasons);

                Result r3 = await UpsertFirearm(context, r1.Value).ConfigureAwait(false);
                reasons.AddRange(r3.Reasons);

                Result r4 = await _firearmsService
                                 .AssociateWithRangeEvent(context, r1.Value.Id!, r2.Value.Id!)
                                 .ConfigureAwait(false);

                reasons.AddRange(r4.Reasons);

                if (r4.IsSuccess && r3.IsSuccess && r2.IsSuccess)
                {
                    Success x = new("Processed data for range event that occured on " + occurredUtc.ToString("O"));
                    reasons.Add(x);
                }
            }

            return new Result().WithReasons(reasons);
        }

        internal async Task<Result> UpsertFirearm(DapperCommandContext context, Firearm f)
        {
            Result<EntityId> r1 = await _firearmsService.UpsertAsync(context, f).ConfigureAwait(false);
            IReason          reason;
            if (r1.IsSuccess)
            {
                reason = new Success($"Upserted the firearm '{f.Name}', {r1.Value.RowId}/{r1.Value.Id}.");
            }
            else
            {
                reason = new Error($"Failed to upsert the firearm '{f.Name}'.");
            }

            return new Result().WithReason(reason);
        }

        internal async Task<Result<SimpleRangeEvent>> UpsertSimpleRangeEvent(DapperCommandContext       context,
                                                                             string                     firearmName,
                                                                             [ValueRange(0, 10000)] int roundsFired,
                                                                             string                     rangeName,
                                                                             string?                    ammoDescription,
                                                                             string?                    notes,
                                                                             DateTimeOffset             eventDate)
        {
            SimpleRangeEvent sre = new(eventDate)
                                   {
                                       AmmoDescription = ammoDescription,
                                       FirearmName     = firearmName,
                                       RoundsFired     = roundsFired,
                                       Notes           = notes,
                                       RangeName       = rangeName,
                                   };

            Result<MlrbId> r1 = await _rangeEventService.UpsertAsync(context, sre)
                                                        .ConfigureAwait(false);
            List<IReason> reasons = [];
            reasons.AddRange(r1.Reasons);

            if (r1.IsFailed)
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

        internal async Task<Result<Firearm>> UpdateFirearmEventStream(DapperCommandContext       context,
                                                                      string                     firearmName,
                                                                      [ValueRange(0, 10000)] int roundsFired,
                                                                      string                     rangeName,
                                                                      string                     ammoDescription,
                                                                      string                     notes,
                                                                      DateTimeOffset             occurredUtc)
        {
            List<IReason> reasons = [];
            Result<FirearmAggregate> r1 = await _faRepo.GetOrCreateByNameAsync(context, firearmName, occurredUtc)
                                                       .ConfigureAwait(false);

            if (r1.IsFailed)
            {
                return new Result().WithReasons(r1.Reasons);
            }

            FirearmAggregate fa = r1.Value!;
            reasons.AddRange(r1.Reasons);

            try
            {
                fa.MoreRoundsFired(roundsFired, occurredUtc, RemoveSurroundingQuotes(ammoDescription));
                fa.AddNote("Range: " + rangeName,          occurredUtc, null, "range_name");
                fa.AddNote(RemoveSurroundingQuotes(notes), occurredUtc);

                Result r2 = await _faRepo.UpsertAsync(context, fa).ConfigureAwait(false);
                reasons.AddRange(r2.Reasons);

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