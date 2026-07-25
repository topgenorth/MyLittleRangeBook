using MyLittleRangeBook.Models;

// ReSharper disable once CheckNamespace
namespace MyLittleRangeBook.EventSourcing
{

    #region Firearm / Notes associations
    public class FirearmAssociatedWithNoteSuccess(MlrbId firearmId, MlrbId noteId)
        : Success($"Associated firearm {firearmId} with note {noteId}")
    {
        public MlrbId FirearmId = firearmId;
        public MlrbId NoteId    = noteId;
    }

    public class FirearmAssociatedWithNoteError(MlrbId firearmId, MlrbId noteId)
        : Error($"Failed to disassociated firearm {firearmId} from note {noteId}")
    {
        public MlrbId FirearmId = firearmId;
        public MlrbId NoteId    = noteId;

    }
    public class FirearmDisassociatedWithNoteError(MlrbId firearmId, MlrbId noteId)
        : Error($"Failed to disassociated firearm {firearmId} from note {noteId}")
    {
        public MlrbId FirearmId = firearmId;
        public MlrbId NoteId    = noteId;
    }
    public class FirearmDisassociatedWithNoteSuccess(MlrbId firearmId, MlrbId noteId)
        : Success($"Disassociated firearm {firearmId} from note {noteId}")
    {
        public MlrbId FirearmId = firearmId;
        public MlrbId NoteId    = noteId;

    }




    #endregion

    #region Firearm/Range Event associations
    public class FirearmAssociatedWithRangeEventSuccess(MlrbId firearmId, MlrbId rangeEventId)
        : Success($"Associated  firearm {firearmId} to range event {rangeEventId}.")
    {
        public MlrbId FirearmId    { get; } = firearmId;
        public MlrbId RangeEventId { get; } = rangeEventId;
    }

    public class FirearmAssociatedToRangeEventError(MlrbId firearmId, MlrbId rangeEventId)
        : Error($"Failed to associate firearm {firearmId} to range event {rangeEventId}.")
    {
        public MlrbId FirearmId    { get; } = firearmId;
        public MlrbId RangeEventId { get; } = rangeEventId;
    }

    public class FirearmDisassociatedFromRangeEventSuccess(MlrbId firearmId, MlrbId rangeEventId)
        : Success($"Dissociated firearm {firearmId} from range event {rangeEventId}.")
    {
        public MlrbId FirearmId    { get; } = firearmId;
        public MlrbId RangeEventId { get; } = rangeEventId;
    }

    public class FirearmDisassociatedFromRangeEventError(MlrbId firearmId, MlrbId rangeEventId)
        : Error($"Failed to dissociated firearm {firearmId} from range event {rangeEventId}.")
    {
        public MlrbId FirearmId    { get; } = firearmId;
        public MlrbId RangeEventId { get; } = rangeEventId;
    }
    #endregion


    #region Firearm/Asset associations
    public class FirearmAssociatedWithAssetSuccess(MlrbId firearmId, MlrbId assetId)
        : Success($"Associated firearm {firearmId} to range event {assetId}.")
    {
        public MlrbId FirearmId { get; } = firearmId;
        public MlrbId AssetId   { get; } = assetId;
    }

    public class FirearmAssociatedWithAssetError(MlrbId firearmId, MlrbId assetId)
        : Error($"Failed to associate firearm {firearmId} to range event {assetId}.")
    {
        public MlrbId FirearmId { get; } = firearmId;
        public MlrbId AssetId   { get; } = assetId;
    }

    public class FirearmDisassociatedFromAssetSuccess(MlrbId firearmId, MlrbId assetId)
        : Success($"Disassociated firearm {firearmId} from range event {assetId}.")
    {
        public MlrbId FirearmId { get; } = firearmId;
        public MlrbId AssetId   { get; } = assetId;
    }

    public class FirearmDisassociatedFromAssetError(MlrbId firearmId, MlrbId assetId)
        : Error($"Failed to dissociated firearm {firearmId} from range event {assetId}.")
    {
        public MlrbId FirearmId { get; } = firearmId;
        public MlrbId AssetId   { get; } = assetId;
    }
    #endregion

    public class FirearmsTableUpdatedFromEventStreamSuccess(string name, MlrbId firearmId)
        : Success($"Updated the firearms table from event stream: {name} (ID: {firearmId})")
    {
        public string Name      { get; } = name;
        public MlrbId FirearmId { get; } = firearmId;
    }

    public class FirearmsTableUpdatedFromEventStreamError(MlrbId firearmId)
        : Error($"Failed to update the firearms table from the event stream: {firearmId}")
    {
        public MlrbId FirearmId { get; } = firearmId;
    }

    public class FailedToGetFirearmEventStream(string name, MlrbId firearmId)
        : Error($"Failed to get the firearm event stream for {name} (ID: {firearmId})")
    {
        public MlrbId FirearmId = firearmId;
        public string Name { get; } = name;
    }

    public class FirearmEventStreamCreatedReason(string name, MlrbId firearmId)
        : Success($"Created new firearm event stream: {name} (ID: {firearmId})")
    {
        public string Name      { get; } = name;
        public MlrbId FirearmId { get; } = firearmId;
    }

    public class FirearmEventStreamLoadedSuccess(MlrbId firearmId)
        : Success($"Loaded firearm event stream from database:{firearmId}")
    {
        public MlrbId FirearmId { get; } = firearmId;
    }

    public class FirearmEventStreamLoadedError(MlrbId firearmId)
        : Success("Failed to loa firearm event stream from database: firearmId.)")
    {
        public MlrbId FirearmId { get; } = firearmId;
    }
}