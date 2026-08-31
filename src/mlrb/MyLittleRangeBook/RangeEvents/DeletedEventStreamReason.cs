using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.RangeEvents
{
    public class DeletedEventStreamReason(MlrbId firearmId)
        : Success($"The event stream was deleted (ID: {firearmId})")
    {
        public MlrbId FirearmId = firearmId;
    }
}