using Fisher.Projections;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmRangeVisitProjection: SingleStreamProjection<RangeVisitCount, Guid>
    {
        public void Apply(FirearmUsedAtRange e, RangeVisitCount view)
        {
            view.VisitCount++;
        }
    }

    public partial class FirearmRoundCountProjection:SingleStreamProjection<FirearmRoundCount, Guid>
    {
        public static FirearmRoundCount Create(FirearmCreated e)
        {
            return new FirearmRoundCount()
                   {
                       Name       = e.Name,
                       RoundCount = 0,
                   };
        }

        public void Apply(FirearmUsedAtRange e, FirearmRoundCount view)
        {
            view.RoundCount += e.RoundsFired;
        }

        public void Apply(FirearmRoundCountAltered e, FirearmRoundCount view)
        {
            view.RoundCount += e.RoundsDelta;
        }
    }
}