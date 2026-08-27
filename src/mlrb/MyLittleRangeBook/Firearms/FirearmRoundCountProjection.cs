using Fisher.Projections;
using MyLittleRangeBook.EventSourcing;

namespace MyLittleRangeBook.Models
{
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
            view.RoundCount += e.RoundCount;
        }

        public void Apply(FirearmRoundCountAltered e, FirearmRoundCount view)
        {
            view.RoundCount += e.Rounds;
        }
    }
}