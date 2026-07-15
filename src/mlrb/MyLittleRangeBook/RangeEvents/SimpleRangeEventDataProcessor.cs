using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.EventSourcing;
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

            Task<Result> deleteTask = _rangeEventService.DeleteAsync(context, sre);
            Task<Result> disassociateTask =
                _rangeEventService.DisassociateFromFirearm(context, firearmId, rangeEventId);
            Result<FirearmAggregate?> faResult = await _faRepo.GetAsync(context, firearmId).ConfigureAwait(false);
            Result[] operationResults = await Task
                                             .WhenAll(deleteTask, disassociateTask)
                                             .ConfigureAwait(false);
            Result<IEnumerable<FirearmAggregate?>>? r1 =
                Result.Merge(operationResults[0], operationResults[1], faResult);

            Result finalResult = new Result().WithReasons(r1.Reasons);
            if (r1.IsSuccess)
            {
                FirearmAggregate fa = faResult.Value!;
                fa.FirearmRoundCountChanged(-1 * sre.RoundsFired, offsetUtc, sre.AmmoDescription);
                fa.DisassociatedWithRangeEvent(rangeEventId, offsetUtc);
                Result r2 = await _faRepo.UpsertAsync(context, fa).ConfigureAwait(false);
                finalResult.Reasons.AddRange(r2.Reasons);
            }

            return finalResult;
        }

        public async Task<Result<MlrbId>> ProcessSimpleRangeEventData(DapperCommandContext            context,
                                                                      string                          firearmName,
                                                                      [ValueRange(-10000, 10000)] int roundsFired,
                                                                      string                          rangeName,
                                                                      string                          ammoDescription,
                                                                      string                          notes,
                                                                      DateOnly?                       dateOfEvent)
        {
            List<IReason> reasons = [];
            (DateOnly _, DateTimeOffset occurredUtc) = GetEventDate(dateOfEvent);
            Result<Firearm> r1 = await UpdateFirearmEventStream(context, firearmName, roundsFired, rangeName,
                                                                ammoDescription,
                                                                notes,
                                                                occurredUtc)
                                    .ConfigureAwait(false);
            reasons.AddRange(r1.Reasons);
            Result<MlrbId> result = new();

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

                if (r2.IsSuccess)
                {
                    Success x = new("Processed data for range event that occured on " + occurredUtc.ToString("O"));
                    reasons.Add(x);
                    result = result.WithValue(r2.Value.Id!);
                }
            }

            return result.WithReasons(reasons);
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

        internal async Task<Result<Firearm>> UpdateFirearmEventStream(DapperCommandContext            context,
                                                                      string                          firearmName,
                                                                      [ValueRange(-10000, 10000)] int roundsFired,
                                                                      string                          rangeName,
                                                                      string                          ammoDescription,
                                                                      string                          notes,
                                                                      DateTimeOffset                  eventDateUtc)
        {
            List<IReason> reasons = [];
            Result<FirearmAggregate> r1 = await _faRepo.GetOrCreateByNameAsync(context, firearmName, eventDateUtc)
                                                       .ConfigureAwait(false);

            if (r1.IsFailed)
            {
                return new Result().WithReasons(r1.Reasons);
            }

            FirearmAggregate fa = r1.Value!;
            reasons.AddRange(r1.Reasons);

            try
            {
                MlrbId rangeEventId = new(eventDateUtc);
                fa.FirearmRoundCountChanged(roundsFired, eventDateUtc, RemoveSurroundingQuotes(ammoDescription));
                fa.AddNote("Range: " + rangeName,          eventDateUtc, null, "range_name");
                fa.AddNote(RemoveSurroundingQuotes(notes), eventDateUtc);
                fa.AssociateWithSimpleRangeEvent(rangeEventId, eventDateUtc);

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