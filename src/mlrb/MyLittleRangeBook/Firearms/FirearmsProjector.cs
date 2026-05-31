using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using static MyLittleRangeBook.Firearms.FirearmAggregate;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    ///     Will update the Firearms table with the events from a given stream.
    /// </summary>
    class FirearmsProjector : IProjector
    {
        readonly ILogger _logger;
        readonly IFirearmAggregateRepository _repo;
        readonly IFirearmsService _service;

        public FirearmsProjector(IFirearmsService service, IFirearmAggregateRepository repo, ILogger logger)
        {
            _service = service;
            _repo = repo;
            _logger = logger;
        }

        public async Task<Result> ProjectAsync(DapperCommandContext context,
            MlrbId streamId,
            IEnumerable<IDomainEvent> domainEvents)
        {
            throw new NotImplementedException();
            Firearm? f = null;

            Result<Firearm> fr = await _service.GetFirearmAsync(streamId, context.Connection, context.CancellationToken)
                .ConfigureAwait(false);

            if (fr.IsFailed)
            {
                return Result.Fail(fr.Errors);
            }

            // [TO20260531] Assumes that the events are in order.
            foreach (IDomainEvent e in domainEvents)
            {
                switch (e)
                {
                    case FirearmCreated x:
                        if (f is null)
                        {
                            f = Firearm.New(x.Name);
                            f.RoundsFired = x.TotalRoundsFired;
                            f.Notes = x.Notes;
                            f.Modified = x.OccurredUtc;
                            f.Created = x.OccurredUtc;
                        }
                        else
                        {
                            _logger.Verbose("New firearm {name}", x.Name);
                        }

                        break;
                    case FirearmModified x:
                        break;
                    default:
                        _logger.Verbose("Unknown and unhandled event {event}.", e);

                        break;
                }
            }

            return Result.Ok();
        }
    }
}
