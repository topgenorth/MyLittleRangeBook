using System.Data.Common;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook
{
    public sealed class SystemTextJsonEventSerializer : IEventSerializer
    {
        readonly IReadOnlyDictionary<Type, string> _eventNames;
        readonly IReadOnlyDictionary<string, Type> _eventTypes;
        readonly JsonSerializerOptions _jsonSerializerOptions;

        public SystemTextJsonEventSerializer(IEnumerable<Type> eventTypes)
            : this(eventTypes, CreateDefaultOptions())
        {
        }

        public SystemTextJsonEventSerializer(
            IEnumerable<Type> eventTypes,
            JsonSerializerOptions jsonSerializerOptions)
        {
            ArgumentNullException.ThrowIfNull(eventTypes);
            ArgumentNullException.ThrowIfNull(jsonSerializerOptions);

            Type[] types = eventTypes.Distinct().ToArray();

            _eventTypes = types.ToDictionary(
                static t => t.GetCustomAttribute<EventTypeAttribute>()?.Name ?? t.Name,
                static t => t,
                StringComparer.Ordinal);

            _eventNames = types.ToDictionary(
                static t => t,
                static t => t.GetCustomAttribute<EventTypeAttribute>()?.Name ?? t.Name);

            _jsonSerializerOptions = jsonSerializerOptions;
        }

        public string GetEventType(object @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            Type type = @event.GetType();

            if (_eventNames.TryGetValue(type, out string? eventType))
            {
                return eventType;
            }

            throw new InvalidOperationException(
                $"Event type '{type.FullName}' is not registered.");
        }


        public object Deserialize(string eventType, string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            if (!_eventTypes.TryGetValue(eventType, out Type? runtimeType))
            {
                throw new InvalidOperationException(
                    $"Unknown event type '{eventType}'.");
            }

            object? deserialized = JsonSerializer.Deserialize(
                json,
                runtimeType,
                _jsonSerializerOptions);

            return deserialized
                   ?? throw new InvalidOperationException(
                       $"Deserialization returned null for event type '{eventType}'.");
        }

        public string Serialize(object @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            Type runtimeType = @event.GetType();

            return JsonSerializer.Serialize(@event, runtimeType, _jsonSerializerOptions);
        }

        static JsonSerializerOptions CreateDefaultOptions()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                TypeInfoResolver =
                    JsonTypeInfoResolver.Combine(MlrbJsonContext.Default, new DefaultJsonTypeInfoResolver())
            };

            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            return options;
        }
    }

    /// <summary>
    ///     Represents an attribute used to associate a name with a specific event type.
    ///     This attribute is intended to be applied to structures that represent domain events,
    ///     facilitating their identification and serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class EventTypeAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

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
    public interface IRangeAssetProjector
    {
        Task ProjectAsync(string rangeAssetId,
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
        string GetEventType(object @event);
        string Serialize(object domainEvent);
        object Deserialize(string rowEventType, string rowDataJson);
    }
}
