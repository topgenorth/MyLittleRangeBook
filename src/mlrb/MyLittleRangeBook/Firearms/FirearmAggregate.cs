using System.Text;
using System.Text.Json.Nodes;
using JetBrains.Annotations;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmAggregate : Aggregate
    {
        internal const string STREAM_TYPE = "firearm";

        FirearmAggregate() => Name = "";

        public override string         DefaultStreamType => STREAM_TYPE;
        public          long?          RowId             { get; set; }
        public          string         Name              { get; set; }
        public          int            RoundsFired       { get; set; }
        public          string?        Notes             { get; set; }
        public          DateTimeOffset Created           { get; set; }
        public          DateTimeOffset Modified          { get; set; }
        public          bool           IsActive          { get; set; }


        public static FirearmAggregate Create(EventStreamRow streamRow)
        {
            FirearmAggregate agg = new();
            agg.Hydrate(streamRow);

            return agg;
        }

        public static FirearmAggregate New(string name, DateTimeOffset utcNow)
        {
            MlrbId           streamId = MlrbId.FromString(name);
            FirearmAggregate agg      = new();
            agg.Raise(new FirearmCreated(streamId, name, utcNow));

            return agg;
        }

        public Firearm ToFirearm()
        {
            Firearm f = new()
                        {
                            Id          = Id.ToString(),
                            RoundsFired = RoundsFired,
                            IsActive    = IsActive,
                            Name        = Name,
                            Notes       = Notes,
                            Modified    = Modified,
                        };

            return f;
        }

        JsonObject CreateNoteMetaDataJson(string? metadataJson, string noteType = "note")
        {
            JsonObject metadata;
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                metadata = new JsonObject();
            }
            else
            {
                metadata = JsonNode.Parse(metadataJson)?.AsObject() ?? new JsonObject();
            }

            metadata["note_type"] = noteType;
            return metadata;
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
                    AddNote(sbBarrelChange.ToString(), x.OccurredUtc);

                    break;
                case FirearmCleaned x:
                    AddNote($"Cleaned on {x.OccurredUtc.ToString()}.", x.OccurredUtc);

                    break;

                case FirearmDischargeMoreRounds x:
                    RoundsFired += x.Rounds;
                    AddNote($"Ammo: {x.AmmoDescription}", x.OccurredUtc, x.MetaDataJson, "ammo_description");
                    break;

                case FirearmCreated x:
                    Id      = x.StreamId;
                    Name    = x.Name;
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
                    AppendToFirearmAggregateNoteSummary(sbModified.ToString());

                    break;

                case FirearmNoteAdded x:
                    AppendToFirearmAggregateNoteSummary(x.Text);
                    break;


                case FirearmSightingSystemChanged x:
                    StringBuilder sbSightsChanged = new StringBuilder("Changed sights from ")
                                                   .Append(x.OldAimingSystem)
                                                   .Append(" to ")
                                                   .Append(x.NewAimingSystem)
                                                   .Append(". ")
                                                   .Append(x.OccurredUtc.ToString());
                    AppendToFirearmAggregateNoteSummary(sbSightsChanged.ToString());

                    break;
            }
        }

        /// <summary>
        ///     Internal helper method to append the text the Notes property of the aggregate.
        /// </summary>
        /// <param name="text"></param>
        void AppendToFirearmAggregateNoteSummary(string text)
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

        public void AddNote(string? text, DateTimeOffset utcNow, string? metaDataJson = null, string noteType = "note")
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            Raise(new FirearmNoteAdded(Id, text, utcNow, noteType, metaDataJson));
        }

        public void AssociatedWithAsset(MlrbId assetId, DateTimeOffset dto) =>
            Raise(new FirearmAssociatedWithAsset(Id, assetId, dto));

        public void AssociateWithSimpleRangeEvent(MlrbId assetId, DateTimeOffset utcNow) =>
            Raise(new FirearmAssociatedWithRangeEvent(Id, assetId, utcNow));

        public void Cleaned(DateTimeOffset utcNow) => Raise(new FirearmCleaned(Id, utcNow));

        /// <summary>
        ///     Record the discharge of rounds for this firearm.
        /// </summary>
        /// <param name="roundCount">If 0, then nothing is done.</param>
        /// <param name="occurredUtc"></param>
        /// <param name="ammoDescription">Optional. Free format text that describes the ammo used. </param>
        /// <param name="metadataJson">Optional.  Any JSON metadata for this event.</param>
        public void MoreRoundsFired([ValueRange(0, 10000)] int roundCount, DateTimeOffset occurredUtc,
                                    string?                    ammoDescription = null,
                                    string?                    metadataJson    = null)
        {
            if (roundCount > 0)
            {
                Raise(new FirearmDischargeMoreRounds(Id, roundCount, occurredUtc, ammoDescription));
            }
            else
            {
                AddNote(ammoDescription, occurredUtc, metadataJson, "ammo_description");
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