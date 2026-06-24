using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.EventSourcing
{
    public interface IDomainEvent
    {
        MlrbId         StreamId    { get; }
        DateTimeOffset OccurredUtc { get; }
    }
}