using System.Text.Json.Serialization;
using ByteAether.Ulid;

namespace MyLittleRangeBook.Models
{
    /// <summary>
    ///     This is a unique ID value that is sortable by time.
    /// </summary>
    [JsonConverter(typeof(MlrbIdJsonConverter))]
    public readonly record struct MlrbId : IComparable<MlrbId>, IEquatable<MlrbId>
    {
        // --- Constants & Static Fields ---
        internal static readonly Ulid.GenerationOptions s_defaultOptions = new()
                                                                           {
                                                                               Monotonicity = Ulid.GenerationOptions
                                                                                  .MonotonicityOptions
                                                                                  .MonotonicIncrement,
                                                                           };

        public static readonly MlrbId Empty = new(Ulid.Empty);

        // --- State ---
        readonly Ulid _id;

        // --- Constructors ---
        internal MlrbId(Ulid id) => _id = id;

        public MlrbId() : this(Guid.CreateVersion7()) { }

        public MlrbId(Guid guid) : this(Ulid.New(guid.ToByteArray())) { }

        public MlrbId(DateTimeOffset dto) : this(Ulid.New(dto, s_defaultOptions)) { }

        public MlrbId(byte[] sha256, DateTimeOffset? utcNow = null)
            : this(Ulid.New(utcNow ?? DateTimeOffset.UtcNow, sha256)) { }

        // --- Properties ---
        public Ulid           Value                   => _id;
        public DateTime       DateTimeLocal           => _id.Time.ToLocalTime().DateTime;
        public DateTimeOffset DateTimeOffset          => _id.Time;
        public int            CompareTo(MlrbId other) => _id.CompareTo(other._id);

        // --- Factory Methods ---
        /// <summary>
        ///     Creates a new <see cref="MlrbId" /> instance based on a given <see cref="DateOnly" /> value.
        /// </summary>
        public static MlrbId From(DateOnly dateOnly, TimeOnly? timeOnly = null, TimeZoneInfo? timeZone = null)
        {
            if (dateOnly == default)
            {
                return Empty;
            }

            timeZone ??= TimeZoneInfo.Local;
            TimeOnly time = timeOnly ?? TimeOnly.FromDateTime(DateTime.Now);
            DateTime dt   = dateOnly.ToDateTime(time);

            if (timeZone.IsInvalidTime(dt))
            {
                dt = dt.AddHours(1); // Adjust past DST transition gap
            }

            TimeSpan offset = timeZone.GetUtcOffset(dt);
            return new MlrbId(new DateTimeOffset(dt, offset));
        }

        /// <summary>
        ///     Creates an instance of <see cref="MlrbId" /> from the specified <see cref="DateTime" />.
        /// </summary>
        public static MlrbId From(DateTime dateTime) => new(new DateTimeOffset(dateTime));

        /// <summary>
        ///     Converts the provided string value into an instance of <see cref="MlrbId" />.
        ///     If the string is a valid ULID, it is directly converted; otherwise, a deterministic ULID is generated from its
        ///     SHA-256 hash.
        /// </summary>
        public static MlrbId FromString(string? stringValue, DateTimeOffset? dateTimeOffset = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(stringValue);
            if (Ulid.TryParse(stringValue, null, out Ulid parsedUlid))
            {
                return new MlrbId(parsedUlid);
            }

            return DeterministicUlid.MlrbIdFromString(stringValue, null, dateTimeOffset);
        }


        // --- Conversions & Formatting ---
        public          byte[] ToByteArray() => _id.ToByteArray();
        public override string ToString()    => _id.ToString();

        // --- Operators ---
        public static implicit operator Guid(MlrbId d) => d._id.ToGuid();

        public static implicit operator string(MlrbId d)          => d._id.ToString();
        public static implicit operator byte[](MlrbId d)          => d._id.ToByteArray();
        public static implicit operator Ulid(MlrbId   d)          => d._id;
        public static implicit operator MlrbId(string someString) => FromString(someString);
        public static implicit operator MlrbId(Ulid   ulid)       => new(ulid);
        public static explicit operator MlrbId(Guid   guid)       => new(guid);
    }
}