using System.Text;
using JasperFx.Events.Aggregation;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public class Firearm
    {
        /// <summary>
        /// Represents the unique identifier for a firearm entity.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        ///  The name of the firearm.  This must be unique to all of the firearms that belong to you (including firearms that are "inactive").
        /// </summary>
        [NaturalKey] public string         Name             { get; set; } = "Unknown Firearm";

        /// <summary>
        /// Gets or sets the date and time when the firearm was created.
        /// </summary>
        public              DateTimeOffset Created          { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Represents the timestamp of the most recent modification made to the firearm entity.
        /// </summary>
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;
        public bool           IsActive { get; set; } = true;

        /// <summary>
        /// Holds a list of Garmin Shotview CSC files for this firearm.
        /// </summary>
        public              List<string>   ShotViewCsvFiles { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of notes associated with the firearm.
        /// </summary>
        public List<Note> Notes { get; set; } = new();



        public void Apply(FirearmActivated e) => IsActive = true;

        public void Apply(FirearmBarrelChanged e)
        {
            StringBuilder sbBarrelChange = new StringBuilder("Barrel changed from ")
                                          .Append(e.OldBarrel)
                                          .Append(" to ")
                                          .Append(e.NewBarrel)
                                          .Append('.');
            AppendTextAsNote(sbBarrelChange.ToString());
        }

        public void Apply(FirearmCleaned e) =>
            AppendTextAsNote($"Fiearm was cleaned.");

        [NaturalKeySource]
        public void Apply(FirearmCreated e) => Name = e.FirearmName;

        public void Apply(FirearmDeactivated e) => IsActive = false;

        public void Apply(FirearmModified e)
        {
            StringBuilder sbModified = new StringBuilder("Firearm modified:  ")
                                      .AppendLine()
                                      .Append(e.Description);
            AppendTextAsNote(sbModified.ToString());
        }

        public void Apply(FirearmNoteAdded e) => AppendTextAsNote(e.Text);

        public void Apply(FirearmSightingSystemChanged e)
        {
            StringBuilder sbSightsChanged = new StringBuilder("Changed sights from ")
                                           .Append(e.OldAimingSystem)
                                           .Append(" to ")
                                           .Append(e.NewAimingSystem)
                                           .Append(". ");
            AppendTextAsNote(sbSightsChanged.ToString());
        }

        public void Apply(GarminShotViewFileAddedToFirearm e) => ShotViewCsvFiles.Add(e.FileContents);

        /// <summary>
        ///     Internal helper method to append the text the Notes property of the aggregate.
        /// </summary>
        /// <param name="text"></param>
        void AppendTextAsNote(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var note = new Note() { Content = text.Trim(), NoteType = "firearm"};
            Notes.Add(note);
        }

        string CombineAllNotes()
        {
            var sb = new StringBuilder();
            foreach (var n in Notes.OrderBy(x => x.ModifiedUtc))
            {
                sb.AppendLine($"-- {n.ModifiedUtc:O}");
                sb.AppendLine(n.Content);
                sb.AppendLine(string.Empty);
            }
            return sb.ToString();
        }
    }
}