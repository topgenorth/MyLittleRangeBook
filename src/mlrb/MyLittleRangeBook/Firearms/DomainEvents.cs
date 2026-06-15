using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public partial class FirearmAggregate
    {
        [EventType("firearm-barrel-changed")]
        public record struct FirearmBarrelChanged(
            MlrbId StreamId,
            string OldBarrel,
            string NewBarrel,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-active")]
        public record struct FirearmActive(
            MlrbId StreamId,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-created")]
        public record struct FirearmCreated(
            MlrbId StreamId,
            string Name,
            int TotalRoundsFired,
            string? Notes,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-cleaned")]
        public record struct FirearmCleaned(MlrbId StreamId, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("firearm-inactive")]
        public record struct FirearmInactive(
            MlrbId StreamId,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-modification")]
        public record struct FirearmModified(MlrbId StreamId, string Description, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-discharged-rounds")]
        public record struct FirearmDischargeMoreRounds(MlrbId StreamId, int Rounds, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-note-added")]
        public record struct FirearmNoteAdded(MlrbId StreamId, string NewNote, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("firearm-sights-changed")]
        public record struct FirearmSightingSystemChanged(
            MlrbId StreamId,
            string OldSystem,
            string NewSystem,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("range-event-associated-with-firearm")]
        public record struct FirearmAssociatedWithRangeEvent(
            MlrbId StreamId,
            MlrbId RangeEventId,
            int? RoundCount,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("asset-associated-with-firearm")]
        public record struct FirearmAssociatedWithAsset(MlrbId StreamId, MlrbId AssetId, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        // [EventType("firearm-round-count-recalculated")]
        // public record struct FirearmRoundCountRecalculated(
        //     MlrbId StreamId,
        //     int TotalRoundCount,
        //     DateTimeOffset OccurredUtc) : IDomainEvent;

    }
}
