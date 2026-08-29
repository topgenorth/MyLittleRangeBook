using Fisher;
using Fisher.Linq;
using Fisher.Projections;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmUsedAtRangeProjection : EventProjection
    {
        public RangeVisitCount Create(SimpleRangeEventCreated e)
        {
            RangeVisitCount r = new()
                                {
                                    Id         = Guid.CreateVersion7(),
                                    Name       = e.RangeName,
                                    VisitCount = 1,
                                    MostRecentVisit =
                                        new DateTimeOffset(e.EventDate.ToDateTime(new TimeOnly(12, 0),
                                                                    DateTimeKind.Utc), TimeSpan.Zero),
                                };
            return r;
        }

        public async Task Project(FirearmUsedAtRange e, IDocumentOperations ops)
        {
            FirearmRoundCount firearmRoundCount = await UpdateFirearmRoundCount(e, ops);

            ops.Store(firearmRoundCount);
        }

        static async Task<FirearmRoundCount> UpdateFirearmRoundCount(FirearmUsedAtRange e, IDocumentOperations ops)
        {
            FirearmRoundCount? firearmRoundCount = await ops.Query<FirearmRoundCount>().FirstOrDefaultAsync(x => x.Name == e.FirearmName);
            if (firearmRoundCount is null)
            {
                firearmRoundCount = new FirearmRoundCount
                                    {
                                        Id         = Guid.CreateVersion7(),
                                        Name       = e.FirearmName,
                                        RoundCount = e.RoundsFired
                                    };
            }
            else
            {
                firearmRoundCount.RoundCount += e.RoundsFired;
            }

            return firearmRoundCount;
        }
    }
}