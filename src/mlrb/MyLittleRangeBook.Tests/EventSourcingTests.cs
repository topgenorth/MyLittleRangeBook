using System.Text.Json.Serialization;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Tests
{
    public class EventSourcingTests
    {
        [Fact]
        public void Should_not_serialize_MetaDataJson()
        {
            // Arrange
            Type                          eventType   = typeof(TestEvent);
            SystemTextJsonEventSerializer serializer  = new(new[] { eventType });
            MlrbId                        streamId    = new();
            DateTimeOffset                occurredUtc = DateTimeOffset.UtcNow;
            string                        data        = "test data";

            TestEvent evt  = new(streamId, data, occurredUtc) { MetadataJson = "{test_json=\"yes\"}" };
            string    json = serializer.Serialize(evt);
            var jo = System.Text.Json.Nodes.JsonNode.Parse(json);
            jo.ShouldNotBeNull();

        }

        [Fact]
        public void Should_Deserialize_Event_With_EventTypeAttribute()
        {
            // Arrange
            Type                          eventType   = typeof(TestEvent);
            SystemTextJsonEventSerializer serializer  = new(new[] { eventType });
            MlrbId                        streamId    = new();
            DateTimeOffset                occurredUtc = DateTimeOffset.UtcNow;
            string                        data        = "test data";

            TestEvent evt           = new(streamId, data, occurredUtc);
            string    json          = serializer.Serialize(evt);
            string    eventTypeName = serializer.GetEventType(evt);

            // Act
            object deserialized = serializer.Deserialize(eventTypeName, json);

            // Assert
            deserialized.ShouldNotBeNull();
            deserialized.ShouldBeOfType<TestEvent>();
            TestEvent deserializedEvt = (TestEvent)deserialized;
            deserializedEvt.StreamId.ShouldBe(streamId);
            deserializedEvt.Data.ShouldBe(data);
            deserializedEvt.OccurredUtc.ShouldBe(occurredUtc);
        }

        [EventType("test-event")]
        record struct TestEvent(MlrbId StreamId, string Data, DateTimeOffset OccurredUtc)
            : IDomainEvent, IHaveMetadataJson
        {
            [JsonIgnore] public string? MetadataJson { get; set; }
        }
    }
}