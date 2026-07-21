using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Notes
{
    public record Note
    {
        public Note() => Id = new MlrbId().ToString();

        public long? RowId { get; set; }

        public string Id { get; set; }

        public string NoteType { get; set; } = "note";

        public string Content { get; set; } = string.Empty;

        public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset ModifiedUtc { get; set; } = DateTimeOffset.UtcNow;

        public override string ToString() => $"{Id ?? "N/A"} [{NoteType}]";
    }
}