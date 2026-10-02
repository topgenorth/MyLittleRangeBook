namespace MyLittleRangeBook.RangeEvents
{
    public sealed class InvalidSimpleRangeSessionIdError : Error
    {
        public InvalidSimpleRangeSessionIdError(Guid id) : base($"Invalid simple range session ID: {id}")
        {
            Metadata.Add(nameof(id), id);
        }

        public InvalidSimpleRangeSessionIdError(string id) : this(Guid.Parse(id))
        {

        }
    }

}