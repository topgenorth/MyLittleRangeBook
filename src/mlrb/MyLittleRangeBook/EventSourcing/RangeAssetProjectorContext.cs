using System.Data.Common;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.EventSourcing
{
    [Obsolete("Switching to DapperCommandContext")]
    public record RangeAssetProjectorContext(
        SqliteConnection            Connection,
        DbTransaction               Transaction,
        MlrbId                      RangeAssetId,
        IReadOnlyList<IDomainEvent> PendingEvents,
        CancellationToken           CancellationToken = default);
}