using System.Security.Cryptography;
using System.Text;
using ByteAether.Ulid;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook
{
    public static class UniqueIdExtensions
    {
        internal static readonly Ulid.GenerationOptions DefaultOptions = new()
        {
            Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.MonotonicIncrement
        };

        public static string NewMlrbId()
        {
            var assetId = Ulid.New(DateTimeOffset.UtcNow, DefaultOptions).ToString();

            return assetId;
        }

        public static MlrbId ToMlrbId(this EntityId entityId)
        {
            return new MlrbId(entityId.Id);
        }
        /// <summary>
        ///     Converts a Nanoid (or any string) to a Ulid deterministically.
        /// </summary>
        /// <param name="nanoid">The Nanoid string to convert.</param>
        /// <returns>A Ulid generated from the hash of the input string.</returns>
        public static Ulid FromNanoid(string nanoid)
        {
            if (string.IsNullOrWhiteSpace(nanoid))
            {
                return Ulid.Empty;
            }

            // We need a deterministic 16-byte value for the Ulid.
            // A SHA256 hash of the Nanoid string is a reliable way to get this.
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(nanoid));

            return Ulid.New(hash.AsSpan(0, 16).ToArray());
        }
    }
}
