using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;
using Xunit;

namespace MyLittleRangeBook.Tests;

public class EventSourcingTests
{
    [EventType("test-event")]
    public record struct TestEvent(MlrbId StreamId, string Data, DateTimeOffset OccurredUtc) : IDomainEvent;

    [Fact]
    public void Should_Deserialize_Event_With_EventTypeAttribute()
    {
        // Arrange
        var eventType = typeof(TestEvent);
        var serializer = new SystemTextJsonEventSerializer(new[] { eventType });
        var streamId = new MlrbId();
        var occurredUtc = DateTimeOffset.UtcNow;
        var data = "test data";
        
        var evt = new TestEvent(streamId, data, occurredUtc);
        var json = serializer.Serialize(evt);
        var eventTypeName = serializer.GetEventType(evt);

        // Act
        var deserialized = serializer.Deserialize(eventTypeName, json);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.ShouldBeOfType<TestEvent>();
        var deserializedEvt = (TestEvent)deserialized;
        deserializedEvt.StreamId.ShouldBe(streamId);
        deserializedEvt.Data.ShouldBe(data);
        deserializedEvt.OccurredUtc.ShouldBe(occurredUtc);
    }
}
