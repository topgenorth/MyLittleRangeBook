namespace MyLittleRangeBook.EventSourcing
{
    /// <summary>
    ///     Will serialize the domain event to JSON.
    /// </summary>
    public interface IEventSerializer
    {
        string GetEventType(object @event);
        string Serialize(object    domainEvent);
        object Deserialize(string  rowEventType, string rowDataJson);
    }
}