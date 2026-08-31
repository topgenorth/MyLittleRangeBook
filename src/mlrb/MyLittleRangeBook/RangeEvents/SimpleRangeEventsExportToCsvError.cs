namespace MyLittleRangeBook.RangeEvents
{
    public sealed class InvalidSimpleRangeEventIdError : Error
    {
        public InvalidSimpleRangeEventIdError(Guid id) : base($"Invalid simple range event ID: {id}")
        {
            Metadata.Add(nameof(id), id);
        }

        public InvalidSimpleRangeEventIdError(string id) : this(Guid.Parse(id))
        {

        }
    }
    public sealed class SimpleRangeEventsExportToCsvError : Error
    {
        public SimpleRangeEventsExportToCsvError(string csvFileName, Exception exception) :
            base($"Could not export simple range events to CSV file `{csvFileName}`.")
        {
            Metadata.Add(nameof(csvFileName), csvFileName);
            CausedBy(exception);
        }
    }
}