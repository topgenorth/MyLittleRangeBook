namespace MyLittleRangeBook.RangeEvents
{
    public sealed class SimpleRangeEventsExportedToCsvSuccess : Success
    {
        public SimpleRangeEventsExportedToCsvSuccess(string csvFileName, int rowCount) :
            base($"Exported {rowCount} simple range event(s) to CSV file `{csvFileName}`.")
        {
            Metadata.Add(nameof(csvFileName), csvFileName);
            Metadata.Add(nameof(rowCount),    rowCount);
        }
    }
}