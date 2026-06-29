using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.MlrbAssets
{
    /// <summary>
    ///     Represents the aggregate for managing and tracking the lifecycle of assets in My Little Range Book.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class provides methods to handle asset operations such as creation, copying, fingerprinting,
    ///         parsing, and associating with range events. It also manages the internal state of the asset and
    ///         ensures the integrity of domain events related to asset operations.
    ///     </para>
    ///     <para>The stream ID is based on the name of the original source file.</para>
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
        ///     The content of the file.
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
        ///     Creates a new instance of <see cref="MlrbAssetAggregate" /> by hydrating it with data from the given event stream.
        /// </summary>
        /// <param name="streamRow">The event stream used to hydrate the aggregate.</param>
        /// <returns>A new instance of <see cref="MlrbAssetAggregate" />.</returns>
        public static MlrbAssetAggregate Create(EventStreamRow streamRow)
        {
            var agg = new MlrbAssetAggregate();
            agg.Hydrate(streamRow);

            return agg;
        }

        public override void Apply(IDomainEvent e)
        {
            Modified = e.OccurredUtc;
            switch (e)
            {
                case MrlbAssetAssociatedWithFirearm x:
                    // [TO20260604] Do we need this?
                    break;
                case MlrbAssetAssociatedWithSimpleRangeEvent x:
                    // [TO20260604] Do we need this?
                    break;
                case MlrbAssetCreated x:
                    Id = x.StreamId;
                    Status = "Created";
                    Created = x.OccurredUtc;

                    break;
                case MlrbAssetImportStarted x:
                    Id = x.StreamId;
                    SourceFile = x.SourcePath;
                    Status = "Started";

                    break;
                case MlrbAssetFingerprintComputed x:
                    SHA256 = x.Sha256;
                    Status = "Fingerprinted";

                    break;
                case MlrbAssetFileCopied x:
                    DestinationPath = x.DestinationPath;
                    Status = "FileCopied";
                    FileContents = x.FileContents;

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
                    // [TO20260604] Do we need this?
                    break;

                case MlrbAssetImportCompleted x:
                    Modified = x.OccurredUtc;
                    Status = "Completed";

                    break;

                case MlrbAssetUpdatedFromFile x:
                    Modified = x.OccurredUtc;
                    Status = "Updated";
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
                DestinationPath,
                MimeType,
                FileContents,
                Created,
                Modified);

            return row;
        }

        public void AssociatedWithFirearm(string firearmId, DateTimeOffset occurredUtc)
        {
            Raise(new MrlbAssetAssociatedWithFirearm(Id, firearmId, occurredUtc));
        }


        /// <summary>
        ///     Raise when we create the asset.
        /// </summary>
        /// <param name="StreamId"></param>
        /// <param name="OccurredUtc"></param>
        [EventType("mlrb-asset-created")]
        public record struct MlrbAssetCreated(MlrbId StreamId, DateTimeOffset OccurredUtc,string? MetadataJson = null)
            : IDomainEvent;

        /// <summary>
        ///     We started importing the asset (a file).
        /// </summary>
        /// <param name="StreamId"></param>
        /// <param name="SourcePath"></param>
        /// <param name="OccurredUtc"></param>
        [EventType("mlrb-asset-import-started")]
        public record struct MlrbAssetImportStarted(MlrbId StreamId, string SourcePath, DateTimeOffset OccurredUtc,string? MetadataJson = null)
            : IDomainEvent;

        [EventType("mlrb-asset-copied")]
        public record struct MlrbAssetFileCopied(
            MlrbId         StreamId,
            string         DestinationPath,
            byte[]         FileContents,
            DateTimeOffset OccurredUtc,string? MetadataJson = null)
            : IDomainEvent;

        [EventType("mlrb-asset-stored-in-database")]
        public record struct MlrbAssetStoredInDatabase(
            MlrbId         StreamId,
            byte[]         FileContents,
            DateTimeOffset OccurredUtc,string? MetadataJson = null) : IDomainEvent;

        [EventType("mlrb-asset-parsed")]
        public record struct MlrbAssetParsed(MlrbId StreamId, string MimeType, DateTimeOffset OccurredUtc,string? MetadataJson = null)
            : IDomainEvent;

        [EventType("mlrb-asset-fingerprint-computed")]
        public record struct MlrbAssetFingerprintComputed(
            MlrbId         StreamId,
            string         Sha256,
            long           FileSize,
            DateTimeOffset OccurredUtc,string? MetadataJson = null)
            : IDomainEvent;

        [EventType("mlrb-asset-associated-with-firearm")]
        public record struct MrlbAssetAssociatedWithFirearm(
            MlrbId         StreamId,
            MlrbId         FirearmId,
            DateTimeOffset OccurredUtc,string? MetadataJson = null) : IDomainEvent;

        [EventType("mlrb-asset-associated-with-simple-range-event")]
        public record struct MlrbAssetAssociatedWithSimpleRangeEvent(
            MlrbId         StreamId,
            MlrbId         SimpleRangEventId,
            DateTimeOffset OccurredUtc,string? MetadataJson = null) : IDomainEvent;

        [EventType("mlrb-asset-import-completed")]
        public record struct MlrbAssetImportCompleted(MlrbId StreamId, DateTimeOffset OccurredUtc,string? MetadataJson = null) : IDomainEvent;

        [EventType("mlrb-asset-import-failed")]
        public record struct MlrbAssetImportFailed(MlrbId StreamId, string Reason, DateTimeOffset OccurredUtc,string? MetadataJson = null)
            : IDomainEvent;

        [EventType("mlrb-asset-updated-from-file")]
        public record struct MlrbAssetUpdatedFromFile(
            MlrbId         StreamId,
            string         FileName,
            byte[]         FileContents,
            DateTimeOffset OccurredUtc,string? MetadataJson = null) : IDomainEvent;

        public void AssociatedWithSimpleRangeEvent(MlrbId simpleRangeEventId, DateTimeOffset utcNow)
        {
            Raise(new MlrbAssetAssociatedWithSimpleRangeEvent(Id, simpleRangeEventId, utcNow));
        }
    }
}
