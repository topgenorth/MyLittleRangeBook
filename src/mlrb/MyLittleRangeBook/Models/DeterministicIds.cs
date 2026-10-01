using System.Security.Cryptography;
using System.Text;

namespace MyLittleRangeBook.Models
{
    /// <summary>
    ///     Provides methods to create deterministic ULIDs (Universally Unique Lexicographically Sortable Identifiers).
    ///     This static class leverages hashing and timestamp-based generation mechanisms
    ///     to create identifiers that are both reproducible and conform to specified versioning standards like Guid V7.
    /// </summary>
    public static class DeterministicUlid
    {
        /// <summary>
        ///     The default namespace that will be used.
        /// </summary>
        public const string DEFAULT_MLRBID_CATEGORY = "MyLittleRangeBook";

        /// <summary>
        ///     Generates a cryptographic hash of the provided category and value, returning the first 16 bytes.
        /// </summary>
        /// <param name="category">
        ///     The category used as a namespace for scoping the hash. It must be a non-empty string.
        /// </param>
        /// <param name="value">
        ///     The value to be hashed. This value cannot be null.
        /// </param>
        /// <returns>
        ///     A 16-byte array representing the deterministic hash of the concatenated category and value.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the category is null, empty, or consists only of whitespace.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the value parameter is null.
        /// </exception>
        public static byte[] GetBytes(
            string category,
            string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(category);
            ArgumentNullException.ThrowIfNull(value);

            string input = $"{category}:{value.Trim().ToUpperInvariant()}";
            byte[] hash  = SHA256.HashData(Encoding.UTF8.GetBytes(input));

            return hash[..16];
        }

        /// <summary>
        ///     Creates a deterministic <see cref="MlrbId" /> from a string value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="namespace">This is used to help reduce collisions.  The value will be "scoped" to a particular namespace.</param>
        /// <param name="dateTimeOffset">If missing then DateTimeOffset.UtcNow will be used.</param>
        /// <returns></returns>
        public static MlrbId MlrbIdFromString(string          value, string? @namespace = null,
                                              DateTimeOffset? dateTimeOffset = null)
        {
            byte[] bytes = GetBytes(@namespace ?? DEFAULT_MLRBID_CATEGORY, value);
            return new MlrbId(bytes, dateTimeOffset ?? DateTimeOffset.UtcNow);
        }

        /// <summary>
        ///     Creates a Guid V7 from an existing Guid and timestamp.
        /// </summary>
        public static Guid TransmuteToV7(Guid originalGuid, DateTimeOffset timestamp)
        {
            byte[] bytes  = originalGuid.ToByteArray();
            long   unixMs = timestamp.ToUnixTimeMilliseconds();

            // Unix epoch milliseconds in big-endian (48 bits / 6 bytes)
            bytes[0] = (byte)((unixMs >> 40) & 0xFF);
            bytes[1] = (byte)((unixMs >> 32) & 0xFF);
            bytes[2] = (byte)((unixMs >> 24) & 0xFF);
            bytes[3] = (byte)((unixMs >> 16) & 0xFF);
            bytes[4] = (byte)((unixMs >> 8)  & 0xFF);
            bytes[5] = (byte)(unixMs         & 0xFF);

            // Set UUID version to 7 (bits 4..7 of byte 6 / time_hi_and_version)
            bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);

            // Set variant to RFC 4122/9562 (bits 6..7 of byte 8 / clock_seq_hi_and_reserved to 10xx_xxxx)
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

            return new Guid(bytes);
        }
    }
}