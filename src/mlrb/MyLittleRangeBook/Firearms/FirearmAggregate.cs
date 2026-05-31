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

        public MlrbId Id { get; private set; } = MlrbId.Empty;

        public string Name { get; private set; } = string.Empty;

        public int RoundCount { get; private set; }

        public string Notes { get; private set; } = string.Empty;

        public static FirearmAggregate Create(EventStream stream)
        {
            var agg = new FirearmAggregate();
            agg.Hydrate(stream);

            return agg;
        }

        public static FirearmAggregate New(string name, int roundsCount, string notes, DateTimeOffset utcNow)
        {
            if (roundsCount < 0)
            {
                throw new ArgumentException("Round count must be > 0.");
            }

            var streamId = MlrbId.FromString(name);
            var agg = new FirearmAggregate();
            agg.Raise(new FirearmCreated(streamId, name, roundsCount, notes, utcNow));

            return agg;
        }

        public override void Apply(IDomainEvent e)
        {
            switch (e)
            {
                case BarrelChanged x:
                    StringBuilder sbBarrelChange = new StringBuilder("Barrel changed from ")
                        .Append(x.OldBarrel)
                        .Append(" to ")
                        .Append(x.NewBarrel)
                        .Append(" on ")
                        .Append(x.OccurredUtc.ToString());
                    AppendToNotes(sbBarrelChange.ToString());

                    break;
                case FirearmCleaned x:
                    AppendToNotes($"Cleaned on {x.OccurredUtc.ToString()}.");

                    break;
                case FirearmCreated x:
                    Id = x.StreamId;
                    Name = x.Name;
                    RoundCount = x.TotalRoundsFired;
                    Notes = x.Notes;

                    break;
                case Modified x:
                    StringBuilder sbModified = new StringBuilder("Firearm modified on ")
                        .Append(x.OccurredUtc.ToString())
                        .AppendLine()
                        .Append(x.Description);
                    AppendToNotes(sbModified.ToString());

                    break;
                case NewNoteAdded x:
                    AppendToNotes(x.NewNote);

                    break;
                case RoundsFired x:
                    RoundCount += x.Rounds;

                    break;

                case SightingSystemChanged x:
                    StringBuilder sbSightsChanged = new StringBuilder("Changed sights from ")
                        .Append(x.OldSystem)
                        .Append(" to ")
                        .Append(x.NewSystem)
                        .Append(". ")
                        .Append(x.OccurredUtc.ToString());
                    AppendToNotes(sbSightsChanged.ToString());

                    break;

                case UsedInRangeEvent x:
                    RoundCount += x.RoundCount ?? 0;

                    break;
            }
        }

        void AppendToNotes(string newNote)
        {
            if (string.IsNullOrWhiteSpace(newNote))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Notes))
            {
                Notes = newNote;
            }
            else
            {
                StringBuilder newNotes = new StringBuilder(Notes)
                    .AppendLine()
                    .AppendLine("--")
                    .AppendLine()
                    .AppendLine(newNote.Trim());
                Notes = newNotes.ToString();
            }
        }

        public void Cleaned(DateTimeOffset utcNow)
        {
            Raise(new FirearmCleaned(Id, utcNow));
        }

        public void MoreRoundsFired(int roundCount, DateTimeOffset utcNow)
        {
            if (roundCount < 0)
            {
                throw new ArgumentException("Round count must be > 0.");
            }

            Raise(new RoundsFired(Id, roundCount, utcNow));
        }

        public void AppendToNotes(string newNote, DateTimeOffset utcNow)
        {
            Raise(new NewNoteAdded(Id, newNote, utcNow));
        }

        public void AssociateWithRangeEvent(MlrbId rangeEventId, int? roundCount, DateTimeOffset utcNow)
        {
            if (roundCount is < 0)
            {
                throw new ArgumentException("Round count must be > 0.");
            }

            Raise(new UsedInRangeEvent(Id, rangeEventId, roundCount, utcNow));
            if (roundCount is not null)
            {
                Raise(new RoundsFired(Id, roundCount.Value, utcNow));
            }
        }

        #region Domain Events
        [EventType("firearm-barrel-changed")]
        internal record struct BarrelChanged(
            MlrbId StreamId,
            string OldBarrel,
            string NewBarrel,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-created")]
        internal record struct FirearmCreated(
            MlrbId StreamId,
            string Name,
            int TotalRoundsFired,
            string Notes,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-cleaned")]
        internal record struct FirearmCleaned(MlrbId StreamId, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-modification")]
        internal record struct Modified(MlrbId StreamId, string Description, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-note-added")]
        internal record struct NewNoteAdded(MlrbId StreamId, string NewNote, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-rounds-fired")]
        internal record struct RoundsFired(MlrbId StreamId, int Rounds, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-sights-changed")]
        internal record struct SightingSystemChanged(
            MlrbId StreamId,
            string OldSystem,
            string NewSystem,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-associated-with-range-event")]
        internal record struct UsedInRangeEvent(
            MlrbId StreamId,
            MlrbId RangeEventId,
            int? RoundCount,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;
        #endregion
    }
}
