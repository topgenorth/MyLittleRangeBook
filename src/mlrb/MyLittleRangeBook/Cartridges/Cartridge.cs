using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Cartridges
{
    public record Cartridge
    {
        public Cartridge()
        {
            Id = new MlrbId().ToString();
        }

        /// <summary>
        ///     A Nanoid to uniquely identify the Cartridge. Will be null for a new entity.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        ///     The database row ID of the Cartridge. Will be null for a new record.
        /// </summary>
        public long? RowId { get; set; }

        /// <summary>
        ///     The name of the Cartridge.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     The common name of the Cartridge.
        /// </summary>
        public string? CommonName { get; set; }

        /// <summary>
        ///     The projectile diameter in millimeters.
        /// </summary>
        public double ProjectileDiameterMetric { get; set; }

        /// <summary>
        ///     The projectile diameter in inches.
        /// </summary>
        public double ProjectileDiameterImperial { get; set; }

        /// <summary>
        ///     Whether the cartridge is suitable for rifles.
        /// </summary>
        public bool SuitableForRifle { get; set; }

        /// <summary>
        ///     Whether the cartridge is suitable for pistols.
        /// </summary>
        public bool SuitableForPistol { get; set; }

        /// <summary>
        ///     The time (UTC) that the record was created.
        /// </summary>
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        ///     The time (UTC) that the record was last modified.
        /// </summary>
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;

        public bool IsActive { get; set; } = true;

        public override string ToString()
        {
            return $"{Id ?? "N/A"} {Name}";
        }
    }
}
