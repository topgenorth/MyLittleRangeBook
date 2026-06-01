using System.Text;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public class FirearmAggregate : Aggregate
    {

        internal const string STREAM_TYPE = "firearm";

        FirearmAggregate()
        {

        }

        public override string DefaultStreamType => STREAM_TYPE;
        public long? RowId { get; set; }
        public string Name { get; set; }
        public int RoundsFired { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
        public bool IsActive { get; set; }


        public static FirearmAggregate Create(EventStream stream)
        {
            var agg = new FirearmAggregate();
            agg.Hydrate(stream);

            return agg;
        }

        public static FirearmAggregate New(string name, int roundsCount, string? notes, DateTimeOffset utcNow)
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

        public Firearm ToFirearm()
        {
            var f = new Firearm {
                Id = Id.ToString(),
                RoundsFired = RoundsFired,
                IsActive = IsActive,
                Name = Name,
                Notes = Notes,
                Modified = DateTimeOffset.UtcNow
            };

            return f;
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
                case FirearmActive x:
                    IsActive = true;

                    break;
                case FirearmCleaned x:
                    AppendToNotes($"Cleaned on {x.OccurredUtc.ToString()}.");

                    break;
                case FirearmCreated x:
                    Id = x.StreamId;
                    Name = x.Name;
                    RoundsFired = x.TotalRoundsFired;
                    if (x.Notes is not null)
                    {
                        Notes = x.Notes;
                    }

                    break;
                case FirearmInactive x:
                    IsActive = false;

                    break;
                case FirearmModified x:
                    StringBuilder sbModified = new StringBuilder("Firearm modified on ")
                        .Append(x.OccurredUtc.ToString())
                        .AppendLine()
                        .Append(x.Description);
                    AppendToNotes(sbModified.ToString());

                    break;
                case NewNoteAdded x:
                    AppendToNotes(x.NewNote);

                    break;
                case FiredMoreBullets x:
                    RoundsFired += x.Rounds;

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

                case RangeEventAssociatedWithFirearm x:
                    RoundsFired += x.RoundCount ?? 0;

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

            Raise(new FiredMoreBullets(Id, roundCount, utcNow));
        }

        public void AppendToNotes(string newNote, DateTimeOffset utcNow)
        {
            Raise(new NewNoteAdded(Id, newNote, utcNow));
        }

        public void IsInactive(bool inactive, DateTimeOffset utcNow)
        {
            if (inactive)
            {
                Raise(new FirearmInactive(Id, utcNow));
            }
            else
            {
                Raise(new FirearmActive(Id, utcNow));
            }
        }

        public void AssociateWithRangeEvent(MlrbId rangeEventId, int? roundCount, DateTimeOffset utcNow)
        {
            if (roundCount is < 0)
            {
                throw new ArgumentException("Round count must be > 0.");
            }

            Raise(new RangeEventAssociatedWithFirearm(Id, rangeEventId, roundCount, utcNow));
            if (roundCount is not null)
            {
                Raise(new FiredMoreBullets(Id, roundCount.Value, utcNow));
            }
        }

        #region Domain Events
        [EventType("firearm-barrel-changed")]
        internal record struct BarrelChanged(
            MlrbId StreamId,
            string OldBarrel,
            string NewBarrel,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-active")]
        internal record struct FirearmActive(
            MlrbId StreamId,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-created")]
        internal record struct FirearmCreated(
            MlrbId StreamId,
            string Name,
            int TotalRoundsFired,
            string? Notes,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-cleaned")]
        internal record struct FirearmCleaned(MlrbId StreamId, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-inactive")]
        internal record struct FirearmInactive(
            MlrbId StreamId,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-modification")]
        internal record struct FirearmModified(MlrbId StreamId, string Description, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-discharged-rounds")]
        internal record struct FiredMoreBullets(MlrbId StreamId, int Rounds, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-note-added")]
        internal record struct NewNoteAdded(MlrbId StreamId, string NewNote, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-sights-changed")]
        internal record struct SightingSystemChanged(
            MlrbId StreamId,
            string OldSystem,
            string NewSystem,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("range-event-associated-with-firearm")]
        internal record struct RangeEventAssociatedWithFirearm(
            MlrbId StreamId,
            MlrbId RangeEventId,
            int? RoundCount,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("asset-associated-with-firearm")]
        internal record struct AssetAssociatedWithFirearm(MlrbId StreamId, MlrbId AssetId, DateTimeOffset OccurredUtc) : IDomainEvent;
        #endregion
    }
}
