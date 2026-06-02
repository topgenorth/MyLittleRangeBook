using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    public class MlrbAssetAggregate : Aggregate
    {
        public const string DEFAULT_STREAM_TYPE_NAME = "mlrb-asset";

        MlrbAssetAggregate()
        {
        }

        public override string DefaultStreamType => DEFAULT_STREAM_TYPE_NAME;

        /// <summary>
        ///     The path to the asset that is being imported.
        /// </summary>
        public string SourcePath { get; private set; } = string.Empty;

        /// <summary>
        ///     The path to the asset after it was imported.
        /// </summary>
        public string DestinationPath { get; private set; } = string.Empty;

        /// <summary>
        ///     The status of the range asset importation process.
        /// </summary>
        public string Status { get; private set; } = "Unknown";

        public string? SHA256 { get; private set; }

        /// <summary>
        ///     The MIME type of the file being imported.
        /// </summary>
        public string MimeType { get; private set; } = "application/octet-stream";

        /// <summary>
        ///     If the import process failed, this property contains the reason for the failure.
        public string? FailureReason { get; private set; }

        /// <summary>
        ///     The ID of the range event that this asset is associated with, if any. This will be set when the asset is added to a
        ///     range event.
        /// </summary>
        public MlrbId RangeEventId { get; private set; } = MlrbId.Empty;

        /// <summary>
        ///     Creating a new MlrbAssetAggregate.
        /// </summary>
        /// <param name="streamId"></param>
        /// <returns></returns>
        public static MlrbAssetAggregate New(MlrbId streamId)
        {
            var agg = new MlrbAssetAggregate();
            agg.Raise(new MlrbAssetCreated(streamId, DateTimeOffset.UtcNow));

            return agg;
        }

        /// <summary>
        ///     This factory method is used when creating a new aggregate for a file that is being imported as a range asset. The
        ///     sourcePath is used to generate the aggregate Id, which is the same as the asset Id and event stream Id.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="utcNow"></param>
        /// <returns></returns>
        public static MlrbAssetAggregate New(string sourcePath, DateTimeOffset utcNow)
        {
            var fileInfo = new FileInfo(sourcePath);
            var id = MlrbId.FromFile(fileInfo);

            MlrbAssetAggregate agg = New(id);

            agg.Raise(new MlrbAssetImportStarted(agg.Id, sourcePath, utcNow));

            return agg;
        }

        /// <summary>
        /// Creates a new instance of <see cref="MlrbAssetAggregate"/> by hydrating it with data from the given event stream.
        /// </summary>
        /// <param name="stream">The event stream used to hydrate the aggregate.</param>
        /// <returns>A new instance of <see cref="MlrbAssetAggregate"/>.</returns>
        public static MlrbAssetAggregate Create(EventStream stream)
        {
            var agg = new MlrbAssetAggregate();
            agg.Hydrate(stream);
            return agg;
        }

        public override void Apply(IDomainEvent e)
        {
            switch (e)
            {
                case MlrbAssetCreated x:
                    Id = x.StreamId;
                    Status = "Created";

                    break;
                case MlrbAssetImportStarted x:
                    Id = x.StreamId;
                    SourcePath = x.SourcePath;
                    Status = "Started";

                    break;
                case MlrbAssetFingerprintComputed x:
                    SHA256 = x.Sha256;
                    Status = "Fingerprinted";

                    break;
                case MlrbAssetFileCopied x:
                    DestinationPath = x.DestinationPath;
                    Status = "FileCopied";

                    break;
                case MlrbAssetParsed x:
                    MimeType = x.MimeType;
                    Status = "Parsed";

                    break;
                case MlrbAssetImportFailed x:
                    FailureReason = x.Reason;
                    Status = "Failed";

                    break;

                case MlrbAssetStoredInDatabase x:
                    // No state change for this event, but it could be used for auditing or other purposes.

                    break;
                case MlrbAssetAssociateWithRangeEvent x:
                    RangeEventId = x.RangeEventId;
                    Status = "AssociatedWithRangeEvent";

                    break;

                case MlrbAssetImportCompleted x:
                    Status = "Completed";

                    break;

                case MlrbAssetUpdatedFromFile x :
                    Status = $"Updated";
                    SourcePath = x.FileName;

                    break;
                default:
                    throw new InvalidOperationException($"Unknown event type `{e.GetType().Name}`.");
            }
        }

        public void AddedToRangeEvent(MlrbId rangeEventId, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetAssociateWithRangeEvent(Id, rangeEventId, nowUtc));
        }

        public void Copied(string destinationPath, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetFileCopied(Id, destinationPath, nowUtc));
        }

        public void Fail(Exception ex, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetImportFailed(Id, ex.Message, nowUtc));
        }

        public void Fail(string reason, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetImportFailed(Id, reason, nowUtc));
        }

        public void FileFingerprinted(string sha256, long fileSize, DateTimeOffset nowUtc)
        {
            if (Status is "Completed" or "Failed")
            {
                throw new InvalidOperationException($"Cannot compute fingerprint for asset in status `{Status}`.");
            }

            Raise(new MlrbAssetFingerprintComputed(Id, sha256, fileSize, nowUtc));
        }

        public void Parsed(string mimeType, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetParsed(Id, mimeType, nowUtc));
        }

        public void StoredInDatabase(byte[] fileContents, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetStoredInDatabase(Id, fileContents, nowUtc));
        }

        public void Update(string fileName, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetUpdatedFromFile(Id, fileName, nowUtc));
        }

        [EventType("mlrb-asset-created")]
        public record struct MlrbAssetCreated(MlrbId StreamId, DateTimeOffset OccurredUtc)
            : IDomainEvent;


        [EventType("mlrb-asset-import-started")]
        public record struct MlrbAssetImportStarted(MlrbId StreamId, string SourcePath, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("mlrb-asset-copied")]
        public record struct MlrbAssetFileCopied(MlrbId StreamId, string DestinationPath, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("mlrb-asset-stored-in-database")]
        public record struct MlrbAssetStoredInDatabase(
            MlrbId StreamId,
            byte[] FileContents,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("mlrb-asset-parsed")]
        public record struct MlrbAssetParsed(MlrbId StreamId, string MimeType, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("mlrb-asset-fingerprint-computed")]
        public record struct MlrbAssetFingerprintComputed(
            MlrbId StreamId,
            string Sha256,
            long FileSize,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("mlrb-asset-import-completed")]
        public record struct MlrbAssetImportCompleted(MlrbId StreamId, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("mlrb-asset-import-failed")]
        public record struct MlrbAssetImportFailed(MlrbId StreamId, string Reason, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("mlrb-asset-associated-with-range-event")]
        public record struct MlrbAssetAssociateWithRangeEvent(
            MlrbId StreamId,
            MlrbId RangeEventId,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("mlrb-asset-updated-from-file")]
        public record struct MlrbAssetUpdatedFromFile(
            MlrbId StreamId,
            string FileName,
            DateTimeOffset OccurredUtc) : IDomainEvent;
    }
}
