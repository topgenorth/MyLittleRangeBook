using Fisher.Projections.Flattened;

namespace MyLittleRangeBook.RangeEvents
{
    public record SimpleRangeEventCreated(
        DateOnly EventDate,
        string   FirearmName,
        string   RangeName,
        int      RoundsFired,
        string   AmmoDescription,
        string   Notes);

}