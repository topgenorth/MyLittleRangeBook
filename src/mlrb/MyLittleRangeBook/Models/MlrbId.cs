using ByteAether.Ulid;

namespace MyLittleRangeBook.Models
{
    public record EntityId(string Id, long? RowId);

    /// <summary>
    ///     This is a unique ID value that is sortable by time.
    /// </summary>
    public record MlrbId
    {
        public static readonly MlrbId Empty = new(Ulid.Empty);

        readonly Ulid _id;

        MlrbId(Ulid id)
        {
            _id = id;
        }

        public MlrbId(EntityId id) : this(id.Id)
        {
        }

        public MlrbId() : this(DateTimeOffset.UtcNow)
        {
        }

        public MlrbId(byte[] value)
        {
            if (Ulid.IsValid(value))
            {
                _id = Ulid.Parse(value);
            }
            else
            {
                throw new ArgumentException("Byte array is not a valid Ulid.");
            }
        }

        public MlrbId(string value)
        {
            if (Ulid.IsValid(value))
            {
                _id = Ulid.Parse(value);
            }
            else
            {
                _id = UniqueIdExtensions.FromNanoid(value);
            }
        }

        public MlrbId(DateTimeOffset dto)
        {
            _id = Ulid.New(dto, UniqueIdExtensions.DefaultOptions);
        }

        public static implicit operator string(MlrbId d)
        {
            return d._id.ToString();
        }

        public static implicit operator byte[](MlrbId d)
        {
            return d._id.ToByteArray();
        }

        public static implicit operator Ulid(MlrbId d)
        {
            return d._id;
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public byte[] ToByteArray()
        {
            return _id.ToByteArray();
        }
    }
}
