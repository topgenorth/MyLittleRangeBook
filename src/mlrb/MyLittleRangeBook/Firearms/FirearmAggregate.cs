using System.Text;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public class FirearmAggregate : Aggregate
    {
        internal const string STREAM_TYPE = "firearm";

        FirearmAggregate()
        {
            Name = "";
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
            var f = new Firearm
            {
                Id = Id.ToString(),
                RoundsFired = RoundsFired,
                IsActive = IsActive,
                Name = Name,
                Notes = Notes,
                Modified = this.Modified
            };

            return f;
        }

        public override void Apply(IDomainEvent e)
        {
            Modified = e.OccurredUtc;
            switch (e)
            {
                case FirearmActive x:
                    IsActive = true;

                    break;
                case FirearmAssociatedWithAsset x:
                    // [TO20260604] NOOP
                    break;

                case FirearmAssociatedWithRangeEvent x:
                    RoundsFired += x.RoundCount ?? 0;

                    break;
                case FirearmBarrelChanged x:
                    StringBuilder sbBarrelChange = new StringBuilder("Barrel changed from ")
                        .Append(x.OldBarrel)
                        .Append(" to ")
                        .Append(x.NewBarrel)
                        .Append('.');
                    AppendToNotes(sbBarrelChange.ToString());

                    break;
                case FirearmCleaned x:
                    AppendToNotes($"Cleaned on {x.OccurredUtc.ToString()}.");

                    break;

                case FirearmDischargeMoreRounds x:
                    RoundsFired += x.Rounds;

                    break;

                case FirearmCreated x:
                    Id = x.StreamId;
                    Name = x.Name;
                    RoundsFired = x.TotalRoundsFired;
                    Created = x.OccurredUtc;
                    AppendToNotes("Firearm created.");
                    if (x.Notes is not null)
                    {
                        AppendToNotes(x.Notes);
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

                case FirearmNoteAdded x:
                    AppendToNotes(x.NewNote);

                    break;

                case FirearmRoundCountRecalculated x:
                    RoundsFired = x.TotalRoundCount;
                    AppendToNotes("Round count recalculated.");
                    break;

                case FirearmSightingSystemChanged x:
                    StringBuilder sbSightsChanged = new StringBuilder("Changed sights from ")
                        .Append(x.OldSystem)
                        .Append(" to ")
                        .Append(x.NewSystem)
                        .Append(". ")
                        .Append(x.OccurredUtc.ToString());
                    AppendToNotes(sbSightsChanged.ToString());

                    break;
            }
        }

        /// <summary>
        /// Internal helper method to append the text to the Firearm notes.
        /// </summary>
        /// <param name="text"></param>
        void AppendToNotes(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Notes))
            {
                Notes = text;
            }
            else
            {
                StringBuilder newNotes = new StringBuilder(Notes)
                    .AppendLine("--")
                    .Append("Date: ")
                    .AppendLine(Modified.ToString("O"))
                    .AppendLine(text.Trim())
                    .AppendLine();
                Notes = newNotes.ToString();
            }
        }

        public void AppendToNotes(string text, DateTimeOffset utcNow)
        {
            Raise(new FirearmNoteAdded(Id, text, utcNow));
        }

        public void AssociatedWithAsset(MlrbId assetId, DateTimeOffset dto)
        {
            Raise(new FirearmAssociatedWithAsset(Id, assetId, dto));
        }

        public void AssociateWithRangeEvent(MlrbId rangeEventId, int? roundCount, DateTimeOffset utcNow)
        {
            if (roundCount is < 0)
            {
                throw new ArgumentException("Round count must be > 0.");
            }

            Raise(new FirearmAssociatedWithRangeEvent(Id, rangeEventId, roundCount, utcNow));
            if (roundCount is not null)
            {
                Raise(new FirearmDischargeMoreRounds(Id, roundCount.Value, utcNow));
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

            Raise(new FirearmDischargeMoreRounds(Id, roundCount, utcNow));
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

        public void TotalRoundCountRecalculated(int totalRoundCount, DateTimeOffset utcNow)
        {
            if (totalRoundCount is < 0)
            {
                throw new ArgumentException("Round count must be > 0.");
            }

            Raise(new FirearmRoundCountRecalculated(Id, totalRoundCount, utcNow));
        }

        #region Domain Events
        [EventType("firearm-barrel-changed")]
        public record struct FirearmBarrelChanged(
            MlrbId StreamId,
            string OldBarrel,
            string NewBarrel,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-active")]
        public record struct FirearmActive(
            MlrbId StreamId,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-created")]
        public record struct FirearmCreated(
            MlrbId StreamId,
            string Name,
            int TotalRoundsFired,
            string? Notes,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-cleaned")]
        public record struct FirearmCleaned(MlrbId StreamId, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-inactive")]
        public record struct FirearmInactive(
            MlrbId StreamId,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-modification")]
        public record struct FirearmModified(MlrbId StreamId, string Description, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-discharged-rounds")]
        public record struct FirearmDischargeMoreRounds(MlrbId StreamId, int Rounds, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-note-added")]
        public record struct FirearmNoteAdded(MlrbId StreamId, string NewNote, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-sights-changed")]
        public record struct FirearmSightingSystemChanged(
            MlrbId StreamId,
            string OldSystem,
            string NewSystem,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("range-event-associated-with-firearm")]
        public record struct FirearmAssociatedWithRangeEvent(
            MlrbId StreamId,
            MlrbId RangeEventId,
            int? RoundCount,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("asset-associated-with-firearm")]
        public record struct FirearmAssociatedWithAsset(MlrbId StreamId, MlrbId AssetId, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-round-count-recalculated")]
        public record struct FirearmRoundCountRecalculated(
            MlrbId StreamId,
            int TotalRoundCount,
            DateTimeOffset OccurredUtc) : IDomainEvent;
        #endregion
    }
}
