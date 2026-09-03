namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     This event is raised when a "simple range event" is created from the command line.
    /// </summary>
    /// <param name="EventDate">
    ///     The date of the event.  Format should be yyyy-MM-dd.  Time is not required.  Will default to
    ///     today.
    /// </param>
    /// <param name="FirearmName">The unique name of the firearm used.</param>
    /// <param name="RangeName">The unique name of the range.</param>
    /// <param name="RoundsFired"></param>
    /// <param name="AmmoDescription">A text description of the ammo that was used.</param>
    /// <param name="Notes">A free-format entry of any notes from this range event.</param>
    public record SimpleRangeEventCreatedFromCommandLine(
        DateOnly       EventDate,
        string         FirearmName,
        string         RangeName,
        int            RoundsFired,
        string?        AmmoDescription,
        string?        Notes,
        DateTimeOffset OccurredUtc);

    /// <summary>
    ///     This event is raised when we try to create the "simple range event" from the contents of a ShotView CSV file.
    /// </summary>
    /// <param name="FileContents">The contents of the ShotView CSV file.</param>
    /// <param name="FirearmName">The unique name of the firearm used.</param>
    /// <param name="OccurredUtc">The UTC timestamp when the event occurred.</param>
    public record SimpleRangeEventCreatedFromShotViewCsv(
        string         FirearmName,
        string         FileContents,
        DateTimeOffset OccurredUtc);
}