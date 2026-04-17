using ConsoleAppFramework;

namespace MyLittleRangeBook.CLI.Console
{
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
