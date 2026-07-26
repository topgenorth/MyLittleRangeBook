using MyLittleRangeBook.Models;

// ReSharper disable once CheckNamespace
namespace MyLittleRangeBook.EventSourcing
{
    #region Firearm / Notes associations
    /// <summary>
    ///     Reason indicating that a firearm has been successfully associated with a note.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="noteId">The ID of the note.</param>
    public class FirearmAssociatedWithNoteSuccess(MlrbId firearmId, MlrbId noteId)
        : Success($"Associated firearm {firearmId} with note {noteId}")
    {
        public MlrbId FirearmId = firearmId;
        public MlrbId NoteId    = noteId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while associating a firearm with a note.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="noteId">The ID of the note.</param>
    public class FirearmAssociatedWithNoteError(MlrbId firearmId, MlrbId noteId)
        : Error($"Failed to associate firearm {firearmId} with note {noteId}")
    {
        public MlrbId FirearmId = firearmId;
        public MlrbId NoteId    = noteId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while disassociating a firearm from a note.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="noteId">The ID of the note.</param>
    public class FirearmDisassociatedWithNoteError(MlrbId firearmId, MlrbId noteId)
        : Error($"Failed to disassociate firearm {firearmId} from note {noteId}")
    {
        public MlrbId FirearmId = firearmId;
        public MlrbId NoteId    = noteId;
    }

    /// <summary>
    ///     Reason indicating that a firearm has been successfully disassociated from a note.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="noteId">The ID of the note.</param>
    public class FirearmDisassociatedWithNoteSuccess(MlrbId firearmId, MlrbId noteId)
        : Success($"Disassociated firearm {firearmId} from note {noteId}")
    {
        public MlrbId FirearmId = firearmId;
        public MlrbId NoteId    = noteId;
    }
    #endregion

    #region Firearm/Range Event associations
    /// <summary>
    ///     Reason indicating that a firearm has been successfully associated with a range event.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="rangeEventId">The ID of the range event.</param>
    public class FirearmAssociatedWithRangeEventSuccess(MlrbId firearmId, MlrbId rangeEventId)
        : Success($"Associated firearm {firearmId} to range event {rangeEventId}.")
    {
        public MlrbId FirearmId    { get; } = firearmId;
        public MlrbId RangeEventId { get; } = rangeEventId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while associating a firearm with a range event.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="rangeEventId">The ID of the range event.</param>
    public class FirearmAssociatedToRangeEventError(MlrbId firearmId, MlrbId rangeEventId)
        : Error($"Failed to associate firearm {firearmId} to range event {rangeEventId}.")
    {
        public MlrbId FirearmId    { get; } = firearmId;
        public MlrbId RangeEventId { get; } = rangeEventId;
    }

    /// <summary>
    ///     Reason indicating that a firearm has been successfully disassociated from a range event.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="rangeEventId">The ID of the range event.</param>
    public class FirearmDisassociatedFromRangeEventSuccess(MlrbId firearmId, MlrbId rangeEventId)
        : Success($"Dissociated firearm {firearmId} from range event {rangeEventId}.")
    {
        public MlrbId FirearmId    { get; } = firearmId;
        public MlrbId RangeEventId { get; } = rangeEventId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while disassociating a firearm from a range event.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="rangeEventId">The ID of the range event.</param>
    public class FirearmDisassociatedFromRangeEventError(MlrbId firearmId, MlrbId rangeEventId)
        : Error($"Failed to disassociate firearm {firearmId} from range event {rangeEventId}.")
    {
        public MlrbId FirearmId    { get; } = firearmId;
        public MlrbId RangeEventId { get; } = rangeEventId;
    }
    #endregion


    #region Firearm/Asset associations
    /// <summary>
    ///     Reason indicating that a firearm has been successfully associated with an asset.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="assetId">The ID of the asset.</param>
    public class FirearmAssociatedWithAssetSuccess(MlrbId firearmId, MlrbId assetId)
        : Success($"Associated firearm {firearmId} to asset {assetId}.")
    {
        public MlrbId FirearmId { get; } = firearmId;
        public MlrbId AssetId   { get; } = assetId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while associating a firearm with an asset.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="assetId">The ID of the asset.</param>
    public class FirearmAssociatedWithAssetError(MlrbId firearmId, MlrbId assetId)
        : Error($"Failed to associate firearm {firearmId} to asset {assetId}.")
    {
        public MlrbId FirearmId { get; } = firearmId;
        public MlrbId AssetId   { get; } = assetId;
    }

    /// <summary>
    ///     Reason indicating that a firearm has been successfully disassociated from an asset.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="assetId">The ID of the asset.</param>
    public class FirearmDisassociatedFromAssetSuccess(MlrbId firearmId, MlrbId assetId)
        : Success($"Disassociated firearm {firearmId} from asset {assetId}.")
    {
        public MlrbId FirearmId { get; } = firearmId;
        public MlrbId AssetId   { get; } = assetId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while disassociating a firearm from an asset.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="assetId">The ID of the asset.</param>
    public class FirearmDisassociatedFromAssetError(MlrbId firearmId, MlrbId assetId)
        : Error($"Failed to disassociate firearm {firearmId} from asset {assetId}.")
    {
        public MlrbId FirearmId { get; } = firearmId;
        public MlrbId AssetId   { get; } = assetId;
    }
    #endregion

    /// <summary>
    ///     Reason indicating that the firearms table has been successfully updated from an event stream.
    /// </summary>
    /// <param name="name">The name of the firearm.</param>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FirearmsTableUpdatedFromEventStreamSuccess(string name, MlrbId firearmId)
        : Success($"Updated the firearms table from event stream: {name} (ID: {firearmId})")
    {
        public string Name      { get; } = name;
        public MlrbId FirearmId { get; } = firearmId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while updating the firearms table from an event stream.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FirearmsTableUpdatedFromEventStreamError(MlrbId firearmId)
        : Error($"Failed to update the firearms table from the event stream: {firearmId}")
    {
        public MlrbId FirearmId { get; } = firearmId;
    }

    /// <summary>
    ///     Reason indicating that the system failed to retrieve the event stream for a firearm.
    /// </summary>
    /// <param name="name">The name of the firearm.</param>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FailedToGetFirearmEventStream(string name, MlrbId firearmId)
        : Error($"Failed to get the firearm event stream for {name} (ID: {firearmId})")
    {
        public MlrbId FirearmId = firearmId;
        public string Name { get; } = name;
    }

    /// <summary>
    ///     Reason indicating that a new firearm event stream has been created.
    /// </summary>
    /// <param name="name">The name of the firearm.</param>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FirearmEventStreamCreatedReason(string name, MlrbId firearmId)
        : Success($"Created new firearm event stream: {name} (ID: {firearmId})")
    {
        public string Name      { get; } = name;
        public MlrbId FirearmId { get; } = firearmId;
    }

    /// <summary>
    ///     Reason indicating that a firearm event stream has been successfully loaded from the database.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FirearmEventStreamLoadedSuccess(MlrbId firearmId)
        : Success($"Loaded firearm event stream from database: {firearmId}")
    {
        public MlrbId FirearmId { get; } = firearmId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while loading a firearm event stream from the database.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FirearmEventStreamLoadedError(MlrbId firearmId)
        : Error($"Failed to load firearm event stream from database: {firearmId}")
    {
        public MlrbId FirearmId { get; } = firearmId;
    }
}