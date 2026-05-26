using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    public class RangeAssetAggregate
    {
        readonly List<IDomainEvent> _uncommitted = [];

        public MlrbId Id { get; private set; }
        public string SourcePath { get; private set; } = "";
        public string DestinationPath { get; private set; } = "";
        public string Status { get; private set; } = "New";
        public string? SHA256 { get; private set; }
        public string MimeType { get; private set; } = "application/octet-stream";
        public string? FailureReason { get; private set; }
        public int Version { get; private set; }

        public static RangeAssetAggregate Create(string sourcePath, DateTimeOffset utcNow)
        {
            var fileInfo = new FileInfo(sourcePath);
            var agg = new RangeAssetAggregate
            {
                Id = MlrbId.FromFile(fileInfo), SourcePath = sourcePath, Status = "New", Version = 0
            };

            agg.Raise(new RangeAssetImportStarted(agg.Id, sourcePath, utcNow));

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
                case RangeAssetImportStarted x:
                    Id = x.StreamId;
                    SourcePath = x.SourcePath;
                    Status = "Started";

                    break;
                case RangeAssetFingerprintComputed x:
                    SHA256 = x.Sha256;

                    break;
                case RangeAssetCopied x:
                    DestinationPath = x.DestinationPath;

                    break;
                case RangeAssetParsed x:
                    MimeType = x.mimeType;

                    break;
                case RangeAssetImportFailed x:
                    FailureReason = x.Reason;
                    Status = "Failed";

                    break;

                case RangeAssetImportCompleted x:
                    Status = "Completed";

                    break;
            }
        }

        void Raise(IDomainEvent e)
        {
            Apply(e);
            _uncommitted.Add(e);
        }

        public void ClearUncommittedEvents()
        {
            _uncommitted.Clear();
        }

        internal record struct RangeAssetImportStarted(MlrbId StreamId, string SourcePath, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        internal record struct RangeAssetCopied(MlrbId StreamId, string DestinationPath, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        internal record struct RangeAssetStoredInDatabase(
            MlrbId StreamId,
            byte[] FileContents,
            DateTimeOffset OccurredUtc) : IDomainEvent;

        internal record struct RangeAssetParsed(MlrbId StreamId, string mimeType, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        internal record struct RangeAssetFingerprintComputed(
            MlrbId StreamId,
            string Sha256,
            long FileSize,
            DateTimeOffset OccurredUtc)
            : IDomainEvent;

        internal record struct RangeAssetImportCompleted(MlrbId StreamId, DateTimeOffset OccurredUtc) : IDomainEvent;

        internal record struct RangeAssetImportFailed(MlrbId StreamId, string Reason, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        internal record struct RangeAssetAssociateWithRangeEvent(
            MlrbId StreamId,
            MlrbId RangeEventId,
            DateTimeOffset OccurredUtc) : IDomainEvent;
    }
}
