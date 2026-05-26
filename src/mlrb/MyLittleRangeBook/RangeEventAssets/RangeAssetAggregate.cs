using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    public class RangeAssetAggregate
    {
        public const string DefaultStreamType = "range-asset-import";
        readonly List<IDomainEvent> _uncommitted = [];

        RangeAssetAggregate()
        {

        }


        public MlrbId Id { get; private set; } = MlrbId.Empty;
        public string SourcePath { get; private set; } = string.Empty;
        public string DestinationPath { get; private set; }= string.Empty;
        public string Status { get; private set; }= "Unknown";
        public string? SHA256 { get; private set; }
        public string MimeType { get; private set; } = "application/octet-stream";
        public string? FailureReason { get; private set; }
        public int Version { get; private set; } = -1;

        public string StreamType { get; private set; } = DefaultStreamType;

        public MlrbId RangeEventId { get; private set; } = MlrbId.Empty;

        /// <summary>
        ///     Createing a new RangeAssetAggregate..
        /// </summary>
        /// <param name="streamId"></param>
        /// <returns></returns>
        public static RangeAssetAggregate New(MlrbId streamId)
        {
            var agg = new RangeAssetAggregate();
            agg.Raise(new RangeAssetCreated(streamId, DateTimeOffset.UtcNow));

            return agg;
        }

        /// <summary>
        ///     This factory method is used when creating a new aggregate for a file that is being imported as a range asset. The
        ///     sourcePath is used to generate the aggregate Id, which is the same as the asset Id and event stream Id.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="utcNow"></param>
        /// <returns></returns>
        public static RangeAssetAggregate New(string sourcePath, DateTimeOffset utcNow)
        {

            var fileInfo = new FileInfo(sourcePath);
            var id = MlrbId.FromFile(fileInfo);

            RangeAssetAggregate agg = New(id);

            agg.Raise(new RangeAssetImportStarted(agg.Id, sourcePath, utcNow));

            return agg;
        }

        public static RangeAssetAggregate Create(EventStream stream)
        {
            var agg = new RangeAssetAggregate()
            {
                Id = stream.StreamId,
                StreamType = stream.StreamType,
                Version = stream.Version,
                Status = "Unknown"
            };

            return agg;
        }

        public void Parsed(string mimeType, DateTimeOffset nowUtc)
        {
            Raise(new RangeAssetParsed(Id, mimeType, nowUtc));
        }

        public void Copied(string destinationPath, DateTimeOffset nowUtc)
        {
            Raise(new RangeAssetCopied(Id, destinationPath, nowUtc));
        }

        public void StoredInDatabase(byte[] fileContents, DateTimeOffset nowUtc)
        {
            Raise(new RangeAssetStoredInDatabase(Id, fileContents, nowUtc));
        }

        public void AddedToRangeEvent(MlrbId rangeEventId, DateTimeOffset nowUtc)
        {
            Raise(new RangeAssetAssociateWithRangeEvent(Id, rangeEventId, nowUtc));
        }

        public void FileFingerprinted(string sha256, long fileSize, DateTimeOffset nowUtc)
        {
            if (Status is "Completed" or "Failed")
            {
                throw new InvalidOperationException($"Cannot compute fingerprint for asset in status `{Status}`.");
            }

            Raise(new RangeAssetFingerprintComputed(Id, sha256, fileSize, nowUtc));
        }

        public void Fail(Exception ex, DateTimeOffset nowUtc)
        {
            Raise(new RangeAssetImportFailed(Id, ex.Message, nowUtc));
        }

        public void Fail(string reason, DateTimeOffset nowUtc)
        {
            Raise(new RangeAssetImportFailed(Id, reason, nowUtc));
        }

        public IReadOnlyList<IDomainEvent> DequeueUncommittedEvents()
        {
            IDomainEvent[] events = _uncommitted.ToArray();
            _uncommitted.Clear();

            return events;
        }

        public void Apply(IDomainEvent e)
        {
            switch (e)
            {
                case RangeAssetCreated x:
                    Id = x.StreamId;
                    Status = "Created";
                    break;
                case RangeAssetImportStarted x:
                    Id = x.StreamId;
                    SourcePath = x.SourcePath;
                    Status = "Started";

                    break;
                case RangeAssetFingerprintComputed x:
                    SHA256 = x.Sha256;
                    Status = "Fingerprinted";
                    break;
                case RangeAssetCopied x:
                    DestinationPath = x.DestinationPath;
                    Status = "FileCopied";
                    break;
                case RangeAssetParsed x:
                    MimeType = x.mimeType;
                    Status = "Parsed";
                    break;
                case RangeAssetImportFailed x:
                    FailureReason = x.Reason;
                    Status = "Failed";

                    break;

                case RangeAssetStoredInDatabase x:
                    // No state change for this event, but it could be used for auditing or other purposes.

                    break;
                case RangeAssetAssociateWithRangeEvent x:
                    RangeEventId = x.RangeEventId;
                    Status = "AssociatedWithRangeEvent";
                    break;

                case RangeAssetImportCompleted x:
                    Status = "Completed";
                    break;
                default:
                    throw new InvalidOperationException($"Unknown event type `{e.GetType().Name}`.");
            }

        }

        public void Raise(IDomainEvent e)
        {
            Apply(e);
            _uncommitted.Add(e);
            Version++;
        }

        public void ClearUncommittedEvents()
        {
            _uncommitted.Clear();
        }

        [EventType("range-asset-created")]
        internal record struct RangeAssetCreated(MlrbId StreamId, DateTimeOffset OccurredUtc)
            : IDomainEvent;


        [EventType("range-asset-import-started")]
        internal record struct RangeAssetImportStarted(MlrbId StreamId, string SourcePath, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("range-asset-copied")]
        internal record struct RangeAssetCopied(MlrbId StreamId, string DestinationPath, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("range-asset-stored-in-database")]
        internal record struct RangeAssetStoredInDatabase(
            MlrbId StreamId,
            byte[] FileContents,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("range-asset-parsed")]
        internal record struct RangeAssetParsed(MlrbId StreamId, string mimeType, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("range-asset-fingerprint-computed")]
        internal record struct RangeAssetFingerprintComputed(
            MlrbId StreamId,
            string Sha256,
            long FileSize,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("range-asset-import-completed")]
        internal record struct RangeAssetImportCompleted(MlrbId StreamId, DateTimeOffset OccurredUtc) : IDomainEvent;

        [EventType("range-asset-import-failed")]
        internal record struct RangeAssetImportFailed(MlrbId StreamId, string Reason, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("range-asset-associated-with-range-event")]
        internal record struct RangeAssetAssociateWithRangeEvent(
            MlrbId StreamId,
            MlrbId RangeEventId,
            DateTimeOffset OccurredUtc) : IDomainEvent;
    }
}
