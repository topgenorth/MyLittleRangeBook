using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook
{
    public sealed class SystemTextJsonEventSerializer : IEventSerializer
    {
        // TODO [TO20260531] Need to consolidate this with MlrbJsonSerializerContext.
        readonly IReadOnlyDictionary<Type, string> _eventNames;
        readonly IReadOnlyDictionary<string, Type> _eventTypes;
        readonly JsonSerializerOptions             _jsonSerializerOptions;

        public SystemTextJsonEventSerializer(IEnumerable<Type> eventTypes)
            : this(eventTypes, CreateDefaultOptions()) { }

        public SystemTextJsonEventSerializer(
            IEnumerable<Type>     eventTypes,
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

            throw new InvalidOperationException($"Event type '{type.FullName}' is not registered.");
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
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
                                            {
                                                PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
                                                DictionaryKeyPolicy         = JsonNamingPolicy.CamelCase,
                                                PropertyNameCaseInsensitive = true,
                                                WriteIndented               = false,
                                                DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
                                                TypeInfoResolver =
                                                    JsonTypeInfoResolver.Combine(MlrbJsonSerializerContext.Default,
                                                        new DefaultJsonTypeInfoResolver()),
                                            };

            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            return options;
        }
    }
}