namespace MyLittleRangeBook.Models
{
    public record SimpleRangeEvent
    {
        /// <summary>
        ///     A Nanoid to uniquely identify the SimpleRangeEvent. Will be null for a new entity.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        ///     The database row ID of the SimpleRangeEvent. Will be null for a new record.
        /// </summary>
        public long? RowId { get; set; }

        /// <summary>
        ///     The date that the event took place.
        /// </summary>
        public DateTime EventDate { get; set; }

        /// <summary>
        ///     The name of the firearm used. Should match a firearm in the Firearms table.
        /// </summary>
        public string FirearmName { get; set; } = string.Empty;

        /// <summary>
        ///     The name of the range the event took place.
        /// </summary>
        public string RangeName { get; set; } = string.Empty;

        /// <summary>
        ///     How many rounds were fired.
        /// </summary>
        public int RoundsFired { get; set; }

        /// <summary>
        ///     The description of the ammo used.
        /// </summary>
        public string? AmmoDescription { get; set; }

        /// <summary>
        ///     Any additional notes about the event.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        ///     The time (UTC) that the record was created.
        /// </summary>
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        ///     The time (UTC) that the record was last modified.
        /// </summary>
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;
    }
}
