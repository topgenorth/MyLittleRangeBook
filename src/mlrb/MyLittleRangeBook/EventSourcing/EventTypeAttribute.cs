namespace MyLittleRangeBook.EventSourcing
{
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
}