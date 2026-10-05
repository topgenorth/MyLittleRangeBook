using Fisher;
using Fisher.Projections;

namespace MyLittleRangeBook.RangeEvents
{
    public partial class SimpleRangeSessionProjector : EventProjection
    {
        // public static SimpleRangeSession Create(SimpleRangeSessionCreatedFromCli @event)
        // {
        //     var n = DateTimeOffset.UtcNow;
        //     SimpleRangeSession srs = new SimpleRangeSession(@event.EventDate)
        //                              {
        //                                  Id = @event.Id,
        //                                  CorrelationId = @event.CorrelationId,
        //                                  CausationId = @event.CausationId,
        //                                  FirearmName = @event.FirearmName,
        //                                  RangeName = @event.RangeName,
        //                                  RoundsFired = @event.RoundsFired,
        //                                  AmmoDescription = @event.AmmoDescription,
        //                                  Notes = @event.Notes,
        //                                  Created = n,
        //                                  Modified = n,
        //                                  IsActive = true,
        //                              };
        //     return srs;
        // }

        public void Project(SimpleRangeSessionCreatedFromCli @event, IDocumentSession ops)
        {

            SimpleRangeSession srs = new(@event.EventDate.ToUtcDateTimeOffset())
                                     {
                                         Id              = @event.Id,
                                         CorrelationId   = @event.CorrelationId,
                                         CausationId     = @event.CausationId,
                                         FirearmName     = @event.FirearmName,
                                         RangeName       = @event.RangeName,
                                         RoundsFired     = @event.RoundsFired,
                                         AmmoDescription = @event.AmmoDescription,
                                         Notes           = @event.Notes,
                                         Created         = @event.OccurredUtc,
                                         Modified        = @event.OccurredUtc,
                                         IsActive        = true,
                                     };
            ops.Store(srs);
        }

        public void Project(SimpleRangeSessionCreatedFromGui @event, IDocumentSession ops)
        {
            SimpleRangeSession srs = new(@event.EventDate.ToUtcDateTimeOffset())
                                     {
                                         Id              = @event.Id,
                                         CorrelationId   = @event.CorrelationId,
                                         CausationId     = @event.CausationId,
                                         FirearmName     = @event.FirearmName,
                                         RangeName       = @event.RangeName,
                                         RoundsFired     = @event.RoundsFired,
                                         AmmoDescription = @event.AmmoDescription,
                                         Notes           = @event.Notes,
                                         Created         = @event.Created,
                                         Modified        = @event.Modified,
                                         IsActive        = true,
                                     };
            ops.Store(srs);
        }
    }
}