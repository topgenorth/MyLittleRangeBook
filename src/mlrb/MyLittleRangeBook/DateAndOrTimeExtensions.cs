namespace MyLittleRangeBook
{
    public static class DateAndOrTimeExtensions
    {
        static readonly TimeOnly s_noon = TimeOnly.Parse("12:00:00");

        /// <summary>
        /// Converts a <see cref="DateOnly"/> to a DateTimeOffset in UTC <see cref="DateTimeOffset"/>.
        /// Assumes local timezone and a time component of 12:00 (noon) on the given date.
        /// </summary>
        public static DateTimeOffset ToUtcDateTimeOffset(this DateOnly date)
        {

            // Construct a DateTime at noon in the local kind
            var localDateTime         = date.ToDateTime(s_noon);
            var localDateTimeWithKind = DateTime.SpecifyKind(localDateTime, DateTimeKind.Local);

            // Convert to DateTimeOffset in local time, then to UTC
            var localOffset = TimeZoneInfo.Local.GetUtcOffset(localDateTimeWithKind);
            return new DateTimeOffset(localDateTimeWithKind, localOffset);
        }

        /// <summary>
        /// Converts a <see cref="DateOnly"/> to a DateTimeOffset UTC <see cref="DateTimeOffset"/>.
        /// Assumes local timezone and a time component of 12:00 (noon) on the given date.
        /// </summary>
        public static DateTimeOffset ToUtcDateTimeOffset(this DateTime date)
        {
            var localDateTimeWithKind = DateTime.SpecifyKind(date, DateTimeKind.Local);

            // Convert to DateTimeOffset in local time, then to UTC
            var localOffset = TimeZoneInfo.Local.GetUtcOffset(localDateTimeWithKind);
            return new DateTimeOffset(localDateTimeWithKind, localOffset);
        }
    }
}