using JetBrains.Annotations;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEvents
{
    public record SimpleRangeEvent
    {
        public SimpleRangeEvent() => Id = new MlrbId().ToString();

        public SimpleRangeEvent(DateOnly eventDateOnly)
        {
            MlrbId id = MlrbId.From(eventDateOnly);
            Id        = id.ToString();
            EventDate = id.DateTimeLocal;
        }

        public SimpleRangeEvent(DateTime eventDateTime)
        {
            Id        = MlrbId.From(eventDateTime).ToString();
            EventDate = eventDateTime.ToLocalTime();
        }

        public SimpleRangeEvent(DateTimeOffset eventDateTimeOffset)
        {
            Id        = new MlrbId(eventDateTimeOffset);
            EventDate = eventDateTimeOffset.ToLocalTime().DateTime;
        }

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
        [ValueRange(0, 10000)]
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

        public bool IsActive { get; set; } = true;

        /// <summary>
        ///     Creates a new instance of the <see cref="SimpleRangeEvent" /> class with the specified parameters.
        /// </summary>
        /// <param name="firearm">The name of the firearm used in the event.</param>
        /// <param name="rounds">The number of rounds fired during the event.</param>
        /// <param name="range">The name of the range where the event took place.</param>
        /// <param name="ammo">The description of the ammunition used in the event.</param>
        /// <param name="notes">Additional notes related to the event.</param>
        /// <param name="date">
        ///     The date of the event. If not provided, the current date is used. This is always assumed to be in
        ///     the local timezone.
        /// </param>
        /// <returns>A new instance of the <see cref="SimpleRangeEvent" /> class with the provided details.</returns>
        public static SimpleRangeEvent New(string                     firearm,
                                           [ValueRange(0, 10000)] int rounds,
                                           string                     range,
                                           string                     ammo,
                                           string                     notes,
                                           DateOnly                   date = default)
        {
            DateOnly eventDate = date == default ? DateOnly.FromDateTime(DateTime.Now) : date;
            MlrbId   id        = MlrbId.From(eventDate);
            SimpleRangeEvent sre = new()
                                   {
                                       Id              = id.ToString(),
                                       FirearmName     = firearm,
                                       RoundsFired     = rounds,
                                       RangeName       = range,
                                       AmmoDescription = ammo,
                                       Notes           = notes,
                                       EventDate       = eventDate.ToDateTime(TimeOnly.MinValue),
                                   };

            return sre;
        }
    }
}