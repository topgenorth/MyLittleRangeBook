using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmAggregate
    {
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
        [EventType("firearm-barrel-changed")]
        public record struct FirearmBarrelChanged(
            MlrbId         StreamId,
            string         OldBarrel,
            string         NewBarrel,
            DateTimeOffset OccurredUtc) : IDomainEvent;

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
        [EventType("firearm-active")]
        public record struct FirearmActive(
            MlrbId         StreamId,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;


        /// <summary>
        ///     Represents an event indicating the creation of a firearm within the domain.
        ///     This event contains details about the firearm's unique identifier, its name,
        ///     and the timestamp when the firearm was created.
        /// </summary>
        /// <param name="StreamId">
        ///     The unique identifier of the firearm that has been created.
        /// </param>
        /// <param name="Name">
        ///     The name assigned to the newly created firearm.
        /// </param>
        /// <param name="OccurredUtc">
        ///     The UTC timestamp when the creation of the firearm was recorded.
        /// </param>
        [EventType("firearm-created")]
        public record struct FirearmCreated(
            MlrbId         StreamId,
            string         Name,
            DateTimeOffset OccurredUtc) : IDomainEvent;

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
        [EventType("firearm-cleaned")]
        public record struct FirearmCleaned(MlrbId StreamId, DateTimeOffset OccurredUtc) : IDomainEvent;

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
        [EventType("firearm-inactive")]
        public record struct FirearmInactive(
            MlrbId         StreamId,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

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
        [EventType("firearm-modification")]
        public record struct FirearmModified(MlrbId StreamId, string Description, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        /// <summary>
        ///     Represents an event indicating the discharge of multiple rounds from a firearm within the domain.
        ///     This event captures details about the firearm's unique identifier, the number of rounds discharged,
        ///     and the timestamp when the event occurred.
        /// </summary>
        /// <param name="StreamId">
        ///     The unique identifier of the firearm involved in the discharge event.
        /// </param>
        /// <param name="Rounds">
        ///     The number of rounds discharged from the firearm.
        /// </param>
        /// <param name="OccurredUtc">
        ///     The UTC timestamp when the discharge event was recorded.
        /// </param>
        [EventType("firearm-discharged-rounds")]
        public record struct FirearmDischargeMoreRounds(
            MlrbId         StreamId,
            int            Rounds,
            DateTimeOffset OccurredUtc,
            string?        AmmoDescription = null,
            string?        MetaDataJson    = null)
            : IDomainEvent, IHaveMetaDataJson;

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
        [EventType("firearm-note-added")]
        public record struct FirearmNoteAdded(
            MlrbId         StreamId,
            string         Text,
            DateTimeOffset OccurredUtc,
            string         NoteType     = "note",
            string?        MetaDataJson = null)
            : IDomainEvent, IHaveMetaDataJson;

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
        [EventType("firearm-sights-changed")]
        public record struct FirearmSightingSystemChanged(
            MlrbId         StreamId,
            string         OldAimingSystem,
            string         NewAimingSystem,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        /// <summary>
        ///     Represents an event indicating that a firearm has been associated with a range event in the domain.
        ///     This event carries information about the unique identifiers of both the firearm and the range event, as well as the
        ///     timestamp when the association occurred.
        /// </summary>
        /// <param name="StreamId">
        ///     The unique identifier of the firearm associated with the range event.
        /// </param>
        /// <param name="RangeEventId">
        ///     The unique identifier of the range event being associated with the firearm.
        /// </param>
        /// <param name="OccurredUtc">
        ///     The UTC timestamp when the association event occurred.
        /// </param>
        [EventType("range-event-associated-with-firearm")]
        public record struct FirearmAssociatedWithRangeEvent(
            MlrbId         StreamId,
            MlrbId         RangeEventId,
            DateTimeOffset OccurredUtc
        )
            : IDomainEvent;

        /// <summary>
        ///     Represents an event indicating that a firearm has been associated with an asset in the domain.
        ///     This event carries information pertaining to the unique identifiers of both the firearm and the asset, as well as
        ///     the timestamp of when the event occurred.
        /// </summary>
        /// <param name="StreamId">
        ///     The unique identifier of the firearm associated with the asset.
        /// </param>
        /// <param name="AssetId">
        ///     The unique identifier of the asset being associated with the firearm.
        /// </param>
        /// <param name="OccurredUtc">
        ///     The UTC timestamp when the association event occurred.
        /// </param>
        [EventType("asset-associated-with-firearm")]
        public record struct FirearmAssociatedWithAsset(MlrbId StreamId, MlrbId AssetId, DateTimeOffset OccurredUtc)
            : IDomainEvent;
    }
}