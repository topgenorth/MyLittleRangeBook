namespace MyLittleRangeBook.Recipes
{
    /// <summary>
    /// Represents an event generated when a new recipe is added from the command-line interface.
    /// </summary>
    /// <remarks>
    /// This record captures the details of an operation that involves creating a new recipe from
    /// data supplied via the command-line. It includes identifiers used for tracing the event,
    /// the input JSON that describes the recipe, and the timestamp of when the event occurred.
    /// </remarks>
    /// <param name="Id">
    /// A unique identifier for this event.
    /// </param>
    /// <param name="CorrelationId">
    /// An identifier used to correlate this event with others in the same process or workflow.
    /// </param>
    /// <param name="CausationId">
    /// An identifier representing the event or command that triggered this operation.
    /// </param>
    /// <param name="JSON">
    /// The JSON string containing the recipe details.
    /// </param>
    /// <param name="OccurredUtc">
    /// The timestamp (in UTC) indicating when this event occurred.
    /// </param>
    public record struct NewRecipeFromCommandLine(
        Guid           Id,
        Guid           CorrelationId,
        Guid           CausationId,
        string         JSON,
        DateTimeOffset OccurredUtc);


    /// <summary>
    /// Represents an event that occurs when deserialization of a recipe JSON fails.
    /// </summary>
    /// <remarks>
    /// This record is used to capture the necessary details about an error encountered during the
    /// deserialization of recipe JSON data. It includes information such as the unique identifier
    /// for the event, correlation and causation identifiers for tracing the context, the JSON string
    /// being deserialized, the exception encountered, and the UTC timestamp of when the error occurred.
    /// </remarks>
    /// <param name="Id">
    /// The unique identifier for this specific error event.
    /// </param>
    /// <param name="CorrelationId">
    /// The identifier used to correlate this event with other related events in the same workflow or process.
    /// </param>
    /// <param name="CausationId">
    /// The identifier representing an event that caused or triggered this error.
    /// </param>
    /// <param name="JSON">
    /// The JSON string that failed the deserialization process.
    /// </param>
    /// <param name="Exception">
    /// The exception that was thrown during the deserialization process.
    /// </param>
    /// <param name="OccurredUtc">
    /// The timestamp, in UTC, indicating when this error occurred.
    /// </param>
    public record struct ErrorDeserializingRecipeJson(
        Guid           Id,
        Guid           CorrelationId,
        Guid           CausationId,
        string         JSON,
        Exception      Exception,
        DateTimeOffset OccurredUtc);
}