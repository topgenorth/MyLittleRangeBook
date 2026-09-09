using JetBrains.Annotations;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Represents an event that occurs at a shooting range, capturing details such as firearm, ammunition,
    ///     rounds fired, and additional event metadata. Everything is in plain text
    /// </summary>
    public record SimpleRangeEvent
    {
        /// <summary>
        ///     Represents a simple range event containing details such as firearm used, rounds fired, range name,
        ///     ammunition details, and additional notes. Each instance is uniquely identified and automatically
        ///     timestamped upon creation.
        /// </summary>
        public SimpleRangeEvent()
        {
            Id            = Guid.CreateVersion7();
            CorrelationId = Id;
            CausationId   = Id;
        }

        /// <summary>
        ///     Represents an event occurring at a shooting range, capturing various details such as the firearm used,
        ///     ammunition description, rounds fired, range name, and additional optional notes. Provides unique identifiers
        ///     for the event, along with metadata like creation and modification timestamps.
        /// </summary>
        public SimpleRangeEvent(DateOnly eventDateOnly)
        {
            MlrbId id = MlrbId.From(eventDateOnly);
            Id            = MlrbId.From(eventDateOnly);
            CorrelationId = id;
            CausationId   = id;
            EventDate     = id.DateTimeLocal;
        }

        /// <summary>
        ///     Represents a basic range event containing information such as the event's unique identifier,
        ///     firearm name, range name, number of rounds fired, ammunition details, notes, timestamps,
        ///     and other metadata. Automatically timestamps creation and modification times.
        /// </summary>
        public SimpleRangeEvent(DateTime eventDateTime)
        {
            DateTimeOffset eventDateTimeOffset = new(eventDateTime, TimeZoneInfo.Local.GetUtcOffset(eventDateTime));
            Id            = Guid.CreateVersion7(eventDateTimeOffset);
            CorrelationId = Id;
            CausationId   = Id;
            EventDate     = eventDateTime.ToLocalTime();
        }

        /// <summary>
        ///     Represents a shooting range event with details such as firearm name, ammunition description,
        ///     rounds fired, range name, and event-specific metadata. Includes functionality for generating
        ///     unique identifiers and capturing timestamps for creation and modification.
        /// </summary>
        public SimpleRangeEvent(DateTimeOffset eventDateTimeOffset)
        {
            Id            = Guid.CreateVersion7(eventDateTimeOffset);
            CorrelationId = Id;
            CausationId   = Id;
            EventDate     = eventDateTimeOffset.ToLocalTime().DateTime;
        }

        /// <summary>
        ///     A Nanoid to uniquely identify the SimpleRangeEvent. Will be null for a new entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     A unique identifier used to associate related events in a system.
        ///     Typically aligns with the <c>Id</c> of the originating event,
        ///     enabling tracing through a sequence of related events.
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        ///     A unique identifier representing the specific cause or source
        ///     of this SimpleRangeEvent. Typically used to track the triggering
        ///     event in a sequence of related operations or processes.
        /// </summary>
        public Guid CausationId { get; set; }


        /// <summary>
        ///     A string property used to store custom header information related to the range event in JSON.
        ///     This can include metadata or additional context necessary for processing or categorizing the event.
        /// </summary>
        public string Headers { get; set; } = "";

        /// <summary>
        ///     The date that the event took place.
        /// </summary>
        public DateTime EventDate { get; set; }

        /// <summary>
        ///     Explicitly convert the EventDate into a DateTimeOffset in UTC.
        /// </summary>
        public DateTimeOffset OccurredUtc => EventDate.Kind == DateTimeKind.Utc
                                                 ? new DateTimeOffset(EventDate)
                                                 : new DateTimeOffset(DateTime.SpecifyKind(EventDate,
                                                                          DateTimeKind.Local)).ToUniversalTime();

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
        [ValueRange(-10000, 10000)]
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

        /// <summary>
        ///     Indicates whether the range event is active. This property can be used to determine
        ///     if the event is currently valid and operational within the context of the application.
        ///     By default, it is set to true.
        /// </summary>
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
        public static SimpleRangeEvent New(string                          firearm,
                                           [ValueRange(-10000, 10000)] int rounds,
                                           string                          range,
                                           string                          ammo,
                                           string                          notes,
                                           DateOnly                        date = default)
        {
            if (string.IsNullOrWhiteSpace(firearm))
            {
                throw new ArgumentException("Firearm name must be provided.");
            }

            DateOnly eventDate = date == default ? DateOnly.FromDateTime(DateTime.Now) : date;
            Guid     id        = Guid.CreateVersion7();
            SimpleRangeEvent sre = new()
                                   {
                                       Id              = id,
                                       CorrelationId   = id,
                                       CausationId     = id,
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