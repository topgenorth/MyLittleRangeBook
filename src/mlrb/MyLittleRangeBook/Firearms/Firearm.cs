using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Firearms
{
    public record Firearm
    {
        public Firearm(string firearmName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(firearmName);
            Id   = MlrbId.FromString(firearmName);
            Name = firearmName;
        }

        /// <summary>
        ///     prefer to use to firearmName constructor.
        /// </summary>
        public Firearm() => Id = new MlrbId().ToString();

        /// <summary>
        ///     An id to uniquely identify the Firearm. Will be null for a new entity. Should be same as stream id.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        ///     The database row ID of the Firearm. Will be null for a new record.
        /// </summary>
        public long? RowId { get; set; }

        /// <summary>
        ///     The common name of the Firearm.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public int     RoundsFired { get; set; } = 0;
        public string? Notes       { get; set; }

        /// <summary>
        ///     The time (UTC) that the record was created.
        /// </summary>
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        ///     The time (UTC) that the record was last modified.
        /// </summary>
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;

        public bool IsActive { get; set; } = true;

        /// <summary>
        ///     Creates a new instance of a <see cref="Firearm" /> with the specified name.
        /// </summary>
        /// <param name="name">The name of the firearm. Must not be null, empty, or whitespace.</param>
        /// <returns>A new <see cref="Firearm" /> instance with the specified name and an ID generated from the name.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name" /> is null, empty, or whitespace.</exception>
        public static Firearm New(string name)
        {
            return new Firearm(name);
        }

        public override string ToString() => $"{Id ?? "N/A"}{Name}";
    }
}