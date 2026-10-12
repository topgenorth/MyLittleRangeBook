using JetBrains.Annotations;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Represents an session that occurs at a shooting range, capturing details such as firearm, ammunition,
    ///     rounds fired, and additional event metadata. Everything is in plain text
    /// </summary>
    public class SimpleRangeSession
    {
        /// <summary>
        ///     Represents a simple range event containing details such as firearm used, rounds fired, range name,
        ///     ammunition details, and additional notes. Each instance is uniquely identified and automatically
        ///     timestamped upon creation.
        /// </summary>
        public SimpleRangeSession() : this(DateTimeOffset.UtcNow) { }

        /// <summary>
        ///     Represents an event occurring at a shooting range, capturing various details such as the firearm used,
        ///     ammunition description, rounds fired, range name, and additional optional notes. Provides unique identifiers
        ///     for the event, along with metadata like creation and modification timestamps.
        /// </summary>
        [Obsolete("Use a DateTimeOffset.", true)]
        public SimpleRangeSession(DateOnly eventDateOnly) : this(eventDateOnly.ToUtcDateTimeOffset()) { }

        /// <summary>
        ///     Represents a basic range event containing information such as the event's unique identifier,
        ///     firearm name, range name, number of rounds fired, ammunition details, notes, timestamps,
        ///     and other metadata. Automatically timestamps creation and modification times.
        /// </summary>
        public SimpleRangeSession(DateTime eventDateTime) : this(eventDateTime.ToUtcDateTimeOffset()) { }

        /// <summary>
        ///     Represents a shooting range event with details such as firearm name, ammunition description,
        ///     rounds fired, range name, and event-specific metadata. Includes functionality for generating
        ///     unique identifiers and capturing timestamps for creation and modification.
        /// </summary>
        public SimpleRangeSession(DateTimeOffset eventDateTimeOffset)
        {
            Id            = Guid.CreateVersion7(eventDateTimeOffset);
            CorrelationId = Id;
            CausationId   = Id;
            EventDate     = eventDateTimeOffset.ToLocalTime().DateTime;
            IsActive      = true;
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
        public DateTimeOffset OccurredUtc { get; private set; }

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

        public SimpleRangeSession Apply(SimpleRangeSessionCreatedFromGui @event)
        {
            Id              = @event.Id;
            CorrelationId   = @event.CorrelationId;
            CausationId     = @event.CausationId;
            FirearmName     = @event.FirearmName;
            RangeName       = @event.RangeName;
            RoundsFired     = @event.RoundsFired;
            AmmoDescription = @event.AmmoDescription;
            Notes           = @event.Notes;
            Modified        = @event.Modified;
            Created         = @event.Created;
            IsActive        = true;
            return this;
        }

        public SimpleRangeSession Apply(SimpleRangeSessionCreatedFromCli @event)
        {
            Id              = @event.Id;
            CorrelationId   = @event.CorrelationId;
            CausationId     = @event.CausationId;
            FirearmName     = @event.FirearmName;
            RangeName       = @event.RangeName;
            RoundsFired     = @event.RoundsFired;
            AmmoDescription = @event.AmmoDescription;
            Notes           = @event.Notes;
            Modified        = DateTimeOffset.UtcNow;
            Created         = @event.EventDate.ToUtcDateTimeOffset();
            IsActive        = true;
            return this;
        }
    }
}