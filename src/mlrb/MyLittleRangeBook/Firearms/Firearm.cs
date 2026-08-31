using System.Text;
using JasperFx.Events.Aggregation;
using MyLittleRangeBook.EventSourcing;

namespace MyLittleRangeBook.Firearms
{
    public class Firearm
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [NaturalKey] public string         Name     { get; set; } = "Unknown Firearm";
        public              string?        Notes    { get; set; }
        public              DateTimeOffset Created  { get; set; } = DateTimeOffset.UtcNow;
        public              DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;
        public              bool           IsActive { get; set; } = true;

        public void Apply(FirearmActivated e) => IsActive = true;

        public void Apply(FirearmBarrelChanged e)
        {
            StringBuilder sbBarrelChange = new StringBuilder("Barrel changed from ")
                                          .Append(e.OldBarrel)
                                          .Append(" to ")
                                          .Append(e.NewBarrel)
                                          .Append('.');
            AppendToFirearmAggregateNoteSummary(sbBarrelChange.ToString());
        }

        public void Apply(FirearmCleaned e) =>
            AppendToFirearmAggregateNoteSummary($"Cleaned on {e.OccurredUtc.ToString()}.");

        [NaturalKeySource]
        public void Apply(FirearmCreated e) => Name = e.Name;

        public void Apply(FirearmDeactivated e) => IsActive = false;

        public void Apply(FirearmModified e)
        {
            StringBuilder sbModified = new StringBuilder("Firearm modified on ")
                                      .Append(e.OccurredUtc.ToString())
                                      .AppendLine()
                                      .Append(e.Description);
            AppendToFirearmAggregateNoteSummary(sbModified.ToString());
        }

        public void Apply(FirearmNoteAdded e) => AppendToFirearmAggregateNoteSummary(e.Text);

        public void Apply(FirearmSightingSystemChanged e)
        {
            StringBuilder sbSightsChanged = new StringBuilder("Changed sights from ")
                                           .Append(e.OldAimingSystem)
                                           .Append(" to ")
                                           .Append(e.NewAimingSystem)
                                           .Append(". ")
                                           .Append(e.OccurredUtc.ToString());
            AppendToFirearmAggregateNoteSummary(sbSightsChanged.ToString());
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
                                        .AppendLine("--")
                                         // .Append("Date: ")
                                         // .AppendLine(Modified.ToString("O"))
                                        .AppendLine(text.Trim());
                Notes = newNotes.ToString();
            }
        }
    }
}