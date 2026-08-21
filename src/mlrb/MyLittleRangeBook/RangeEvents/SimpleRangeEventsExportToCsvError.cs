namespace MyLittleRangeBook.RangeEvents
{
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