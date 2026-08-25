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
    public class FirearmAssociatedWithNoteSuccess(Guid firearmId, Guid noteId)
        : Success($"Associated firearm {firearmId} with note {noteId}")
    {
        public Guid FirearmId = firearmId;
        public Guid NoteId    = noteId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while associating a firearm with a note.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="noteId">The ID of the note.</param>
    public class FirearmAssociatedWithNoteError(Guid firearmId, Guid noteId)
        : Error($"Failed to associate firearm {firearmId} with note {noteId}")
    {
        public Guid FirearmId = firearmId;
        public Guid NoteId    = noteId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while disassociating a firearm from a note.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="noteId">The ID of the note.</param>
    public class FirearmDisassociatedWithNoteError(Guid firearmId, Guid noteId)
        : Error($"Failed to disassociate firearm {firearmId} from note {noteId}")
    {
        public Guid FirearmId = firearmId;
        public Guid NoteId    = noteId;
    }

    /// <summary>
    ///     Reason indicating that a firearm has been successfully disassociated from a note.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="noteId">The ID of the note.</param>
    public class FirearmDisassociatedWithNoteSuccess(Guid firearmId, Guid noteId)
        : Success($"Disassociated firearm {firearmId} from note {noteId}")
    {
        public Guid FirearmId = firearmId;
        public Guid NoteId    = noteId;
    }
    #endregion

    #region Firearm/Range Event associations
    /// <summary>
    ///     Reason indicating that a firearm has been successfully associated with a range event.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="rangeEventId">The ID of the range event.</param>
    public class FirearmAssociatedWithRangeEventSuccess(Guid firearmId, Guid rangeEventId)
        : Success($"Associated firearm {firearmId} to range event {rangeEventId}.")
    {
        public Guid FirearmId    { get; } = firearmId;
        public Guid RangeEventId { get; } = rangeEventId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while associating a firearm with a range event.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="rangeEventId">The ID of the range event.</param>
    public class FirearmAssociatedToRangeEventError(Guid firearmId, Guid rangeEventId)
        : Error($"Failed to associate firearm {firearmId} to range event {rangeEventId}.")
    {
        public Guid FirearmId    { get; } = firearmId;
        public Guid RangeEventId { get; } = rangeEventId;
    }

    /// <summary>
    ///     Reason indicating that a firearm has been successfully disassociated from a range event.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="rangeEventId">The ID of the range event.</param>
    public class FirearmDisassociatedFromRangeEventSuccess(Guid firearmId, Guid rangeEventId)
        : Success($"Dissociated firearm {firearmId} from range event {rangeEventId}.")
    {
        public Guid FirearmId    { get; } = firearmId;
        public Guid RangeEventId { get; } = rangeEventId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while disassociating a firearm from a range event.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="rangeEventId">The ID of the range event.</param>
    public class FirearmDisassociatedFromRangeEventError(Guid firearmId, Guid rangeEventId)
        : Error($"Failed to disassociate firearm {firearmId} from range event {rangeEventId}.")
    {
        public Guid FirearmId    { get; } = firearmId;
        public Guid RangeEventId { get; } = rangeEventId;
    }
    #endregion


    #region Firearm/Asset associations
    /// <summary>
    ///     Reason indicating that a firearm has been successfully associated with an asset.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="assetId">The ID of the asset.</param>
    public class FirearmAssociatedWithAssetSuccess(Guid firearmId, Guid assetId)
        : Success($"Associated firearm {firearmId} to asset {assetId}.")
    {
        public Guid FirearmId { get; } = firearmId;
        public Guid AssetId   { get; } = assetId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while associating a firearm with an asset.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="assetId">The ID of the asset.</param>
    public class FirearmAssociatedWithAssetError(Guid firearmId, Guid assetId)
        : Error($"Failed to associate firearm {firearmId} to asset {assetId}.")
    {
        public Guid FirearmId { get; } = firearmId;
        public Guid AssetId   { get; } = assetId;
    }

    /// <summary>
    ///     Reason indicating that a firearm has been successfully disassociated from an asset.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="assetId">The ID of the asset.</param>
    public class FirearmDisassociatedFromAssetSuccess(Guid firearmId, Guid assetId)
        : Success($"Disassociated firearm {firearmId} from asset {assetId}.")
    {
        public Guid FirearmId { get; } = firearmId;
        public Guid AssetId   { get; } = assetId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while disassociating a firearm from an asset.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    /// <param name="assetId">The ID of the asset.</param>
    public class FirearmDisassociatedFromAssetError(Guid firearmId, Guid assetId)
        : Error($"Failed to disassociate firearm {firearmId} from asset {assetId}.")
    {
        public Guid FirearmId { get; } = firearmId;
        public Guid AssetId   { get; } = assetId;
    }
    #endregion

    /// <summary>
    ///     Reason indicating that the firearms table has been successfully updated from an event stream.
    /// </summary>
    /// <param name="name">The name of the firearm.</param>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FirearmsTableUpdatedFromEventStreamSuccess(string name, Guid firearmId)
        : Success($"Updated the firearms table from event stream: {name} (ID: {firearmId})")
    {
        public string Name      { get; } = name;
        public Guid FirearmId { get; } = firearmId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while updating the firearms table from an event stream.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FirearmsTableUpdatedFromEventStreamError(Guid firearmId)
        : Error($"Failed to update the firearms table from the event stream: {firearmId}")
    {
        public Guid FirearmId { get; } = firearmId;
    }

    /// <summary>
    ///     Reason indicating that the system failed to retrieve the event stream for a firearm.
    /// </summary>
    /// <param name="name">The name of the firearm.</param>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FailedToGetFirearmEventStream(string name, Guid firearmId)
        : Error($"Failed to get the firearm event stream for {name} (ID: {firearmId})")
    {
        public Guid FirearmId = firearmId;
        public string Name { get; } = name;
    }

    /// <summary>
    ///     Reason indicating that a new firearm event stream has been created.
    /// </summary>
    /// <param name="name">The name of the firearm.</param>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FirearmEventStreamCreatedReason(string name, Guid firearmId)
        : Success($"Created new firearm event stream: {name} (ID: {firearmId})")
    {
        public string Name      { get; } = name;
        public Guid FirearmId { get; } = firearmId;
    }

    /// <summary>
    ///     Reason indicating that a firearm event stream has been successfully loaded from the database.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FirearmEventStreamLoadedSuccess(Guid firearmId)
        : Success($"Loaded firearm event stream from database: {firearmId}")
    {
        public Guid FirearmId { get; } = firearmId;
    }

    /// <summary>
    ///     Reason indicating that an error occurred while loading a firearm event stream from the database.
    /// </summary>
    /// <param name="firearmId">The ID of the firearm.</param>
    public class FirearmEventStreamLoadedError(Guid firearmId)
        : Error($"Failed to load firearm event stream from database: {firearmId}")
    {
        public Guid FirearmId { get; } = firearmId;
    }
}