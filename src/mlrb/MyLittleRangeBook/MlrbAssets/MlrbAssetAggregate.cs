using MyLittleRangeBook.MlrbAssets;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEventAssets
{
    /// <summary>
    /// Represents the aggregate for managing and tracking the lifecycle of assets in My Little Range Book.
    /// </summary>
    /// <remarks>
    /// <para>This class provides methods to handle asset operations such as creation, copying, fingerprinting,
    /// parsing, and associating with range events. It also manages the internal state of the asset and
    /// ensures the integrity of domain events related to asset operations.</para>
    /// <para>The stream ID is based on the name of the original source file.</para>
    /// </remarks>
    public class MlrbAssetAggregate : Aggregate
    {
        public const string DEFAULT_STREAM_TYPE_NAME = "mlrb-asset";

        MlrbAssetAggregate()
        {
        }

        public override string DefaultStreamType => DEFAULT_STREAM_TYPE_NAME;

        public DateTimeOffset Created { get; private set; }
        /// <summary>
        ///     The path to the asset after it was imported.
        /// </summary>
        public string DestinationPath { get; private set; } = string.Empty;

        /// <summary>
        ///     If the import process failed, this property contains the reason for the failure.
        /// </summary>
        public string? FailureReason { get; private set; }

        /// <summary>
        /// The content of the file.
        /// </summary>
        public byte[] FileContents { get; private set; } = [];

        /// <summary>
        ///     The MIME type of the file being imported.
        /// </summary>
        public string MimeType { get; private set; } = "application/octet-stream";

        public DateTimeOffset Modified { get; set; }
        public string? SHA256 { get; private set; }

        /// <summary>
        ///     The path to the asset that is being imported.
        /// </summary>
        public string SourceFile { get; private set; } = string.Empty;

        /// <summary>
        ///     The status of the range asset importation process.
        /// </summary>
        public string Status { get; private set; } = "Unknown";


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
                    Modified = x.OccurredUtc;
                    Id = x.StreamId;
                    Status = "Created";
                    Created = x.OccurredUtc;
                    break;
                case MlrbAssetImportStarted x:
                    Modified = x.OccurredUtc;
                    Id = x.StreamId;
                    SourceFile = x.SourcePath;
                    Status = "Started";

                    break;
                case MlrbAssetFingerprintComputed x:
                    Modified = x.OccurredUtc;
                    SHA256 = x.Sha256;
                    Status = "Fingerprinted";
                    break;
                case MlrbAssetFileCopied x:
                    Modified = x.OccurredUtc;
                    DestinationPath = x.DestinationPath;
                    Status = "FileCopied";
                    FileContents = x.FileContents;
                    break;
                case MlrbAssetParsed x:
                    Modified = x.OccurredUtc;
                    MimeType = x.MimeType;
                    Status = "Parsed";
                    break;
                case MlrbAssetImportFailed x:
                    Modified = x.OccurredUtc;
                    FailureReason = x.Reason;
                    Status = "Failed";
                    break;

                case MlrbAssetStoredInDatabase x:
                    // No state change for this event, but it could be used for auditing or other purposes.
                    Modified = x.OccurredUtc;

                    break;

                case MlrbAssetImportCompleted x:
                    Modified = x.OccurredUtc;
                    Status = "Completed";
                    break;

                case MlrbAssetUpdatedFromFile x :
                    Modified = x.OccurredUtc;
                    Status = $"Updated";
                    SourceFile = x.FileName;
                    FileContents = x.FileContents;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown event type `{e.GetType().Name}`.");
            }
        }

        public void Copied(string destinationPath, byte[] contents, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetFileCopied(Id, destinationPath, contents, nowUtc));
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
            Raise(new MlrbAssetFingerprintComputed(Id, sha256, fileSize, nowUtc));
        }

        public void ImportComplete(DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetImportCompleted(Id, nowUtc));
        }

        public void Parsed(string mimeType, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetParsed(Id, mimeType, nowUtc));
        }

        public void StoredInDatabase(byte[] fileContents, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetStoredInDatabase(Id, fileContents, nowUtc));
        }

        public MlrbAssetRow ToMlrbAssetRow()
        {
            var row = new MlrbAssetRow(Id,
                SourceFile,
                this.DestinationPath,
                this.MimeType,
                this.FileContents,
                this.Created,
                this.Modified);

            return row;
        }

        /// <summary>
        /// Updates the MlrbAssetAggregate with a new file's data.
        /// </summary>
        /// <param name="fromFile">The path of the source file being used for the update.</param>
        /// <param name="fileContents">The binary content of the file used for the update.</param>
        /// <param name="nowUtc">The timestamp representing when the update occurred, in UTC.</param>
        public void Update(string fromFile, byte[] fileContents, DateTimeOffset nowUtc)
        {
            Raise(new MlrbAssetUpdatedFromFile(Id, fromFile, fileContents, nowUtc));
        }

        /// <summary>
        /// Raise when we create the asset.
        /// </summary>
        /// <param name="StreamId"></param>
        /// <param name="OccurredUtc"></param>
        [EventType("mlrb-asset-created")]
        public record struct MlrbAssetCreated(MlrbId StreamId, DateTimeOffset OccurredUtc)
            : IDomainEvent;


        /// <summary>
        /// We started importing the asset (a file).
        /// </summary>
        /// <param name="StreamId"></param>
        /// <param name="SourcePath"></param>
        /// <param name="OccurredUtc"></param>
        [EventType("mlrb-asset-import-started")]
        public record struct MlrbAssetImportStarted(MlrbId StreamId, string SourcePath, DateTimeOffset OccurredUtc)
            : IDomainEvent;

        [EventType("mlrb-asset-copied")]
        public record struct MlrbAssetFileCopied(MlrbId StreamId, string DestinationPath, byte[] FileContents, DateTimeOffset OccurredUtc)
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

        [EventType("mlrb-asset-updated-from-file")]
        public record struct MlrbAssetUpdatedFromFile(
            MlrbId StreamId,
            string FileName,
            byte[] FileContents,
            DateTimeOffset OccurredUtc) : IDomainEvent;
    }
}
