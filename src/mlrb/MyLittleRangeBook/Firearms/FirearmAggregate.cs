using System.Text;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public class FirearmAggregate : Aggregate
    {
        FirearmAggregate()
        {
        }

        public override string DefaultStreamType { get; } = "firearm";

        public MlrbId Id { get; private set; }
        public string Name { get; private set; }
        public int RoundCount { get; private set; }
        public string Notes { get; private set; }

    public override void Apply(IDomainEvent e)
        {
            switch (e)
            {
                case FirearmCreated x:
                    Id = x.StreamId;
                    Name = x.Name;
                    RoundCount = x.TotalRoundsFired;
                    Notes = x.Notes;
                    break;
                case RoundsFired x:
                    RoundCount = RoundCount + x.Rounds;
                    break;
                case UsedInRangeEvent x:
                    // TODO [TO20260530]
                    break;
                case UpdatedNotes x:
                    Notes = x.Notes;
                    break;
            }
        }

        public static FirearmAggregate New(string name, int roundsCount, string notes, DateTimeOffset utcNow)
        {
            var streamId = MlrbId.FromString(name);
            var agg = new FirearmAggregate();
            agg.Raise(new FirearmCreated(streamId, name, roundsCount, notes, utcNow));
            return agg;
        }
        public static FirearmAggregate Create(EventStream stream)
        {
            var agg = new FirearmAggregate();
            agg.Hydrate(stream);

            return agg;
        }

        public void MoreRoundsFired(int roundCount, DateTimeOffset utcNow)
        {
            Raise(new RoundsFired(Id, roundCount, utcNow));
        }
        public void AppendToNotes(string notes, DateTimeOffset utcNow)
        {
            StringBuilder newNotes = new StringBuilder(Notes);
            newNotes.AppendLine();
            newNotes.AppendLine("--");
            newNotes.AppendLine();
            newNotes.Append(notes);
            Raise(new UpdatedNotes(Id, newNotes.ToString(), utcNow));
        }

        public void AssociateWithRangeEvent(MlrbId rangeEventId, int? roundCount, DateTimeOffset utcNow)
        {
            Raise(new UsedInRangeEvent(Id, rangeEventId, roundCount,utcNow));
            if (roundCount is not null)
            {
                Raise(new RoundsFired(Id, roundCount.Value, utcNow));
            }
        }


        [EventType("firearm-created")]
        internal record struct FirearmCreated(
            MlrbId StreamId,
            string Name,
            int TotalRoundsFired,
            string Notes,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-rounds-fired")]
        internal record struct RoundsFired(MlrbId StreamId, int Rounds, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-associated-with-range-event")]
        internal record struct UsedInRangeEvent(MlrbId StreamId, MlrbId RangeEventId, int? RoundsFired, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-")]
        internal record struct UpdatedNotes(MlrbId StreamId, string Notes, DateTimeOffset OccurredUtc) : IDomainEvent;

    }
}
