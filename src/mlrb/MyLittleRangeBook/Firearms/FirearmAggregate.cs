using System.Text;
using JetBrains.Annotations;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmAggregate : Aggregate
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


        public static FirearmAggregate Create(EventStreamRow streamRow)
        {
            var agg = new FirearmAggregate();
            agg.Hydrate(streamRow);

            return agg;
        }

        public static FirearmAggregate New(string name, DateTimeOffset utcNow)
        {
            var streamId = MlrbId.FromString(name);
            var agg = new FirearmAggregate();
            agg.Raise(new FirearmCreated(streamId, name, utcNow));

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
                    break;

                case FirearmAssociatedWithRangeEvent x:
                    // TODO [TO20260626] - Capture the note and ammo description as metadata.

                    break;
                case FirearmBarrelChanged x:
                    StringBuilder sbBarrelChange = new StringBuilder("Barrel changed from ")
                        .Append(x.OldBarrel)
                        .Append(" to ")
                        .Append(x.NewBarrel)
                        .Append('.');
                    AppendToNotes(sbBarrelChange.ToString(), x.OccurredUtc);

                    break;
                case FirearmCleaned x:
                    AppendToNotes($"Cleaned on {x.OccurredUtc.ToString()}.", x.OccurredUtc);

                    break;

                case FirearmDischargeMoreRounds x:
                    RoundsFired += x.Rounds;

                    break;

                case FirearmCreated x:
                    Id = x.StreamId;
                    Name = x.Name;
                    Created = x.OccurredUtc;
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


                case FirearmSightingSystemChanged x:
                    StringBuilder sbSightsChanged = new StringBuilder("Changed sights from ")
                        .Append(x.OldAimingSystem)
                        .Append(" to ")
                        .Append(x.NewAimingSystem)
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
                    .AppendLine()
                    .AppendLine("--")
                    .Append("Date: ")
                    .AppendLine(Modified.ToString("O"))
                    .AppendLine(text.Trim());
                Notes = newNotes.ToString();
            }
        }

        public void AppendToNotes(string? text, DateTimeOffset utcNow, string? metaDataJson = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            Raise(new FirearmNoteAdded(Id, text, utcNow, metaDataJson));
        }

        public void AssociatedWithAsset(MlrbId assetId, DateTimeOffset dto)
        {
            Raise(new FirearmAssociatedWithAsset(Id, assetId, dto));
        }

        public void AssociateWithSimpleRangeEvent(MlrbId assetId, DateTimeOffset utcNow)
        {
            Raise(new FirearmAssociatedWithRangeEvent(Id, assetId, utcNow));
        }

        public void Cleaned(DateTimeOffset utcNow)
        {
            Raise(new FirearmCleaned(Id, utcNow));
        }

        /// <summary>
        /// Record the discharge of rounds for this firearm.
        /// </summary>
        /// <param name="roundCount">If 0, then nothing is done.</param>
        /// <param name="utcNow"></param>
        /// <param name="metadataJson"></param>
        public void MoreRoundsFired([ValueRange(0, 10000)]int roundCount, DateTimeOffset utcNow, string? metadataJson = null)
        {
            if (roundCount > 0)
            {
                Raise(new FirearmDischargeMoreRounds(Id, roundCount, utcNow, metadataJson));
            }
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
    }
}
