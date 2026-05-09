using ConsoleAppFramework;

namespace MyLittleRangeBook.CLI.Console
{
    /// <summary>
    ///     This attribute allows us to parse a date from the command line and return a <c cref="DateOnly" />. If the user does
    ///     not provide a date, then it defaults to today. If the user provides an invalid date, then it also defaults to today.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RangeTripDateParserAttribute : Attribute, IArgumentParser<DateOnly>
    {
        public static bool TryParse(ReadOnlySpan<char> s, out DateOnly result)
        {
            if (s.IsEmpty)
            {
                result = DateOnly.FromDateTime(DateTime.Now);

                return true;
            }

            if (!DateOnly.TryParse(s, out result))
            {
                result = DateOnly.FromDateTime(DateTime.Now);
            }

            return true;
        }
    }
}
