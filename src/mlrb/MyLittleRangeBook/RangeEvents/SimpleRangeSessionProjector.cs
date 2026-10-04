using Fisher;
using Fisher.Projections;

namespace MyLittleRangeBook.RangeEvents
{
    public partial class SimpleRangeSessionProjector : EventProjection
    {
        public static SimpleRangeSession Create(SimpleRangeSessionCreatedFromGui @event)
        {
            SimpleRangeSession srs = new SimpleRangeSession(@event.EventDate)
                                     {
                                         Id = @event.Id,
                                         CorrelationId = @event.CorrelationId,
                                         CausationId = @event.CausationId,
                                         FirearmName = @event.FirearmName,
                                         RangeName = @event.RangeName,
                                         RoundsFired = @event.RoundsFired,
                                         AmmoDescription = @event.AmmoDescription,
                                         Notes = @event.Notes,
                                         Created = @event.Created,
                                         Modified = @event.Modified,
                                         IsActive = true,
                                     };
            return srs;
        }

        // public void Project(SimpleRangeSessionCreatedFromGui @event, IDocumentSession ops)
        // {
        //     var srs = Create(@event);
        //     ops.Store(srs);
        // }
    }
}