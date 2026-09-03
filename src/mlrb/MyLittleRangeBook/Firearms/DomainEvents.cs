using JasperFx.Events.Aggregation;

namespace MyLittleRangeBook.Firearms
{
    /// <summary>
    ///     A Garmin ShotView CSV file was added to a firearm.
    /// </summary>
    /// <param name="FirearmName"></param>
    /// <param name="FileContents">The contents of the ShotView file, which will be text and in CSV format.</param>
    /// <param name="OccurredUtc"></param>
    public record struct GarminShotViewFileAddedToFirearm(
        [property: NaturalKey] string FirearmName,
        string                        FileContents,
        DateTimeOffset                OccurredUtc);

    /// <summary>
    ///     Represents an event indicating that a firearm has been marked as active within the domain.
    ///     This event contains details about the firearm's unique identifier and the timestamp
    ///     when the activation was recorded.
    /// </summary>
    /// <param name="StreamId">
    ///     The unique identifier of the firearm being marked as active.
    /// </param>
    /// <param name="OccurredUtc">
    ///     The UTC timestamp when the firearm was marked as active.
    /// </param>
    public record struct FirearmActivated(
        [property: NaturalKey] string FirearmName,
        DateTimeOffset                OccurredUtc);

    /// <summary>
    ///     Represents an event indicating a change in the barrel of a firearm within the domain.
    ///     This event contains details about the firearm's unique identifier, the previous barrel,
    ///     the updated barrel, and the timestamp when the change occurred.
    /// </summary>
    /// <param name="StreamId">
    ///     The unique identifier of the firearm whose barrel has been changed.
    /// </param>
    /// <param name="OldBarrel">
    ///     The description of the firearm's previous barrel.
    /// </param>
    /// <param name="NewBarrel">
    ///     The description of the new barrel now associated with the firearm.
    /// </param>
    /// <param name="OccurredUtc">
    ///     The UTC timestamp when the barrel change was recorded.
    /// </param>
    public record struct FirearmBarrelChanged(
        [property: NaturalKey] string FirearmName,
        string                        OldBarrel,
        string                        NewBarrel,
        DateTimeOffset                OccurredUtc);

    /// <summary>
    ///     Represents an event indicating that a firearm has been cleaned.
    ///     This event includes the unique identifier of the firearm and the timestamp
    ///     indicating when the cleaning occurred.
    /// </summary>
    /// <param name="StreamId">
    ///     The unique identifier of the firearm that has been cleaned.
    /// </param>
    /// <param name="OccurredUtc">
    ///     The UTC timestamp denoting when the cleaning event was recorded.
    /// </param>
    public record struct FirearmCleaned([property: NaturalKey] string FirearmName, DateTimeOffset OccurredUtc);

    /// <summary>
    ///     Represents an event indicating the creation of a firearm within the domain.
    ///     This event contains details about the firearm's unique identifier, its name,
    ///     and the timestamp when the firearm was created.
    /// </summary>
    /// <param name="StreamId">
    ///     The unique identifier of the firearm that has been created.
    /// </param>
    /// <param name="FirearmName">
    ///     The name assigned to the newly created firearm.
    /// </param>
    /// <param name="OccurredUtc">
    ///     The UTC timestamp when the creation of the firearm was recorded.
    /// </param>
    public record struct FirearmCreated(
        [property: NaturalKey] string FirearmName,
        DateTimeOffset                OccurredUtc);

    /// <summary>
    ///     Represents an event indicating that a firearm has been marked as inactive within the domain.
    ///     This event contains the unique identifier of the firearm and the timestamp when the status change was recorded.
    /// </summary>
    /// <param name="StreamId">
    ///     The unique identifier of the firearm that has been marked as inactive.
    /// </param>
    /// <param name="OccurredUtc">
    ///     The UTC timestamp when the firearm was marked as inactive.
    /// </param>
    public record struct FirearmDeactivated(
        [property: NaturalKey] string FirearmName,
        DateTimeOffset                OccurredUtc);

    /// <summary>
    ///     Represents an event that captures modifications made to a firearm within the domain.
    ///     This event includes details about the firearm's unique identifier, a description
    ///     of the modification, and the timestamp when the modification occurred.
    /// </summary>
    /// <param name="StreamId">
    ///     The unique identifier of the firearm that has been modified.
    /// </param>
    /// <param name="Description">
    ///     A description detailing the modification made to the firearm.
    /// </param>
    /// <param name="OccurredUtc">
    ///     The UTC timestamp indicating when the modification event was recorded.
    /// </param>
    public record struct FirearmModified(
        [property: NaturalKey] string FirearmName,
        string                        Description,
        DateTimeOffset                OccurredUtc);

    /// <summary>
    ///     Represents an event indicating that a note has been added to a firearm within the domain.
    ///     This event contains details about the firearm's unique identifier, the newly added note,
    ///     and the timestamp when the note was recorded.
    /// </summary>
    /// <param name="StreamId">
    ///     The unique identifier of the firearm to which the note has been added.
    /// </param>
    /// <param name="Text">
    ///     The content of the note that has been added to the firearm.
    /// </param>
    /// <param name="OccurredUtc">
    ///     The UTC timestamp when the note was recorded.
    /// </param>
    public record struct FirearmNoteAdded(
        [property: NaturalKey] string FirearmName,
        string                        Text,
        DateTimeOffset                OccurredUtc,
        string                        NoteType = "note");

    /// <summary>
    ///     Represents an event indicating the discharge of multiple rounds from a firearm within the domain.
    ///     This event captures details about the firearm's unique identifier, the number of rounds discharged,
    ///     and the timestamp when the event occurred.
    /// </summary>
    /// <param name="StreamId">
    ///     The unique identifier of the firearm involved in the discharge event.
    /// </param>
    /// <param name="RoundsDelta">
    ///     An integer that will changed the round count for the firearm.
    /// </param>
    /// <param name="OccurredUtc">
    ///     The UTC timestamp when the discharge event was recorded.
    /// </param>
    public record struct FirearmRoundCountAltered(
        [property: NaturalKey] string FirearmName,
        int                           RoundsDelta,
        DateTimeOffset                OccurredUtc,
        string?                       AmmoDescription = null);

    /// <summary>
    ///     Represents an event indicating a change in the sighting system of a firearm within the domain.
    ///     This event contains details about the firearm's unique identifier, the previous sighting system,
    ///     the updated sighting system, and the timestamp when the change occurred.
    /// </summary>
    /// <param name="StreamId">
    ///     The unique identifier of the firearm whose sighting system has been changed.
    /// </param>
    /// <param name="OldAimingSystem">
    ///     The name of the firearm's previous sighting system.
    /// </param>
    /// <param name="NewAimingSystem">
    ///     The name of the new sighting system now associated with the firearm.
    /// </param>
    /// <param name="OccurredUtc">
    ///     The UTC timestamp when the sighting system change was recorded.
    /// </param>
    public record struct FirearmSightingSystemChanged(
        [property: NaturalKey] string FirearmName,
        string                        OldAimingSystem,
        string                        NewAimingSystem,
        DateTimeOffset                OccurredUtc);

    /// <summary>
    ///     Represents an event indicating the use of a specific type of ammunition with a firearm within the domain.
    /// </summary>
    /// <param name="FirearmName">
    ///     The name of the firearm for which ammunition was used.
    /// </param>
    /// <param name="AmmoDescription">
    ///     A description of the ammunition used.
    /// </param>
    /// <param name="Note">
    ///     An optional note providing additional context about the ammunition use.
    /// </param>
    /// <param name="OccurredUtc">
    ///     The UTC timestamp when the ammunition use was recorded.
    /// </param>
    public record struct FirearmUsedAmmo([property: NaturalKey] string FirearmName,
                                         string AmmoDescription,
                                         string? Note, DateTimeOffset OccurredUtc);

    /// <summary>
    /// Represents an event where a firearm was used at a specific range.
    /// </summary>
    /// <param name="FirearmName">The name of the firearm used at the range.</param>
    /// <param name="RangeName">The name of the range where the firearm was used.</param>
    /// <param name="RoundsFired">The number of rounds fired during the usage.</param>
    /// <param name="AmmoDescription">
    /// An optional description of the ammunition used during the session.
    /// </param>
    /// <param name="OccurredUtc">The date and time when the event occurred, in UTC format.</param>
    public record struct FirearmUsedAtRange(
        [property: NaturalKey] string FirearmName,
        string                        RangeName,
        int                           RoundsFired,
        string?                       AmmoDescription,
        DateTimeOffset                OccurredUtc);
}