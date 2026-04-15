namespace MyLittleRangeBook.Models
{
    public record Firearm
    {
        /// <summary>
        ///     A Nanoid to uniquely identify the Firearm. Will be null for a new entity.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        ///     The database row ID of the Firearm. Will be null for a new record.
        /// </summary>
        public long? RowId { get; set; }

        /// <summary>
        ///     The common name of the Firearm.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public string? Notes { get; set; }

        /// <summary>
        ///     The time (UTC) that the record was created.
        /// </summary>
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        ///     The time (UTC) that the record was last modified.
        /// </summary>
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;


        public override string ToString()
        {
            return $"{Id ?? "N/A"}{Name}";
        }
    }
}
