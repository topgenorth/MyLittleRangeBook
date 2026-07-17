using MyLittleRangeBook.Models;

// ReSharper disable once CheckNamespace
namespace MyLittleRangeBook.EventSourcing
{
    public class FirearmEventStreamProjectionSuccess(string name, MlrbId firearmId) : Success($"Projection updated for firearm: {name} (ID: {firearmId})")
    {
        public string Name { get; } = name;
        public MlrbId FirearmId { get; } = firearmId;
    }

    public class FailedToProjectFirearmStreamError(MlrbId firearmId, string? name = null) : Error($"Failed to project the firearm stream for {name ?? "unknown"} (ID: {firearmId})")
    {
        public string Name      { get; } = name ?? "unknown";
        public MlrbId FirearmId { get; } = firearmId;
    }
    public class FailedToGetFirearmEventStream(string name, MlrbId firearmId) : Error($"Failed to get the firearm event stream for {name} (ID: {firearmId})")
    {
        public string Name { get; } = name;
        public MlrbId FirearmId = firearmId;
    }
    public class FirearmEventStreamCreatedReason(string name, MlrbId firearmId)
        : Success($"Created new firearm event stream: {name} (ID: {firearmId})")
    {
        public string Name   { get; } = name;
        public MlrbId FirearmId { get; } = firearmId;
    }

    public class FirearmEventStreamLoadedReason(string name, MlrbId firearmId)
        : Success($"Loaded firearm event stream from database: {name} (ID: {firearmId})")
    {
        public string Name   { get; } = name;
        public MlrbId FirearmId { get; } = firearmId;
    }

}