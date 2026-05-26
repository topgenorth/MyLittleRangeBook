using System.Data.Common;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEventAssets;

namespace MyLittleRangeBook
{
    /// <summary>
    /// </summary>
    /// <param name="StreamId">A unique value that represents the event .</param>
    /// <param name="StreamType"></param>
    /// <param name="Version"></param>
    /// <param name="Created"></param>
    /// <param name="Modified"></param>
    public record struct EventStream(
        string StreamId,
        string StreamType,
        int Version,
        DateTimeOffset Created,
        DateTimeOffset Modified);

    public record struct EventRow(
        string StreamId,
        string StreamType,
        string EventType,
        int Version,
        string DataJson,
        string MetadataJson,
        DateTimeOffset OccurredUtc,
        DateTimeOffset Created,
        DateTimeOffset Modified);


    /// <summary>
    ///     Defines functionality for projecting domain events related to file imports into a storage system.
    /// </summary>
    public interface IImportFileProjector
    {
        Task ProjectAsync(string toString,
            IReadOnlyList<IDomainEvent> pendingEvents,
            SqliteConnection connection,
            DbTransaction transaction,
            CancellationToken cancellationToken);
    }

    public interface IDomainEvent
    {
        MlrbId StreamId { get; }
        DateTimeOffset OccurredUtc { get; }
    }

    /// <summary>
    ///     Will serialize the domain event to JSON.
    /// </summary>
    public interface IEventSerializer
    {
        string Serialize(IDomainEvent domainEvent);
        IDomainEvent Deserialize(string rowEventType, string rowDataJson);
    }

    public interface IRangeAssetAggregateRepository
    {
        Task<Result<RangeAssetAggregate>> GetAsync(MlrbId id, CancellationToken cancellationToken = default);
        Task<Result> SaveAsync(RangeAssetAggregate aggregate, CancellationToken cancellationToken = default);
    }
}
