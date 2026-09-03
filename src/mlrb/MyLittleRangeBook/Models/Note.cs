namespace MyLittleRangeBook.Models
{
    /// <summary>
    ///     A note represents that can be added to an existing document. It's basically just a string.
    /// </summary>
    public record Note
    {
        public Note() => Id = Guid.CreateVersion7();


        /// <summary>
        ///     Gets or sets the unique identifier for the note.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the note, indicating the category or classification
        /// of the note content. The default value is "note".
        /// </summary>
        public string NoteType { get; set; } = "note";

        /// <summary>
        /// Gets or sets the content of the note. Represents the main body of the note as a string.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC date and time when the note was created.
        /// </summary>
        public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the UTC date and time when the note was last modified.
        /// </summary>
        public DateTimeOffset ModifiedUtc { get; set; } = DateTimeOffset.UtcNow;

        public override string ToString() => $"{Id} [{NoteType}]";
    }
}