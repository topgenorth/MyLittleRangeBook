using ByteAether.Ulid;

namespace MyLittleRangeBook
{
    public static class UniqueIdExtensions
    {
        internal static readonly Ulid.GenerationOptions DefaultOptions = new()
        {
            Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.MonotonicIncrement
        };

        public static string NewId()
        {
            var assetId = Ulid.New(DateTimeOffset.UtcNow, DefaultOptions).ToString();

            return assetId;
        }
    }
}
