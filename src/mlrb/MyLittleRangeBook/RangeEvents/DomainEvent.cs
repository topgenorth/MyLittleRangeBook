using Fisher.Projections.Flattened;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    /// This event is raised when a "simple range event" is created from the command line.
    /// </summary>
    /// <param name="EventDate">The date of the event.  Format should be yyyy-MM-dd.  Time is not required.  Will default to today.</param>
    /// <param name="FirearmName">The unique name of the firearm used.</param>
    /// <param name="RangeName">The unique name of the range.</param>
    /// <param name="RoundsFired"></param>
    /// <param name="AmmoDescription">A text description of the ammo that was used.</param>
    /// <param name="Notes">A free-format entry of any notes from this range event.</param>
    public record SimpleRangeEventCreatedFromCommandLine(
        DateOnly EventDate,
        string   FirearmName,
        string   RangeName,
        int      RoundsFired,
        string   AmmoDescription,
        string   Notes);

}