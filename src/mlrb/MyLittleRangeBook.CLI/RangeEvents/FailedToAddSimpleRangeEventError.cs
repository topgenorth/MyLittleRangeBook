namespace MyLittleRangeBook.RangeEvents
{
    public class FailedToAddSimpleRangeEventError : MlrbBaseError
    {
        public FailedToAddSimpleRangeEventError() : base("Unexpected error trying to add SimpleRangeEvent.",
            ReturnCodes.RANGE_EVENT_FAILED_TO_CREATE)
        {
        }
    }
}
