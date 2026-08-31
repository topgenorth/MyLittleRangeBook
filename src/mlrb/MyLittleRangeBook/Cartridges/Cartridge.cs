using JasperFx;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Cartridges
{
    public record Cartridge
    {
        public Cartridge()
        {
        }

        /// <summary>
        /// Creates a new Cartridge instance, with the ID related to the name of the cartridge.  SAAMI name is a good choice.
        /// </summary>
        /// <param name="uniqueName"></param>
        /// <exception cref="ArgumentException"></exception>
        public Cartridge(string uniqueName)
        {
           ArgumentException.ThrowIfNullOrWhiteSpace(uniqueName);
           Name = uniqueName;
           Id = MlrbId.FromString(uniqueName);
        }

        /// <summary>
        ///     An ID to uniquely identify the Cartridge.
        /// </summary>
        public Guid Id { get; set; } = new MlrbId();

        /// <summary>
        ///     The name of the Cartridge. This must be unique across all cartridges.
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
        public bool SuitableForRifle { get; set; } = true;

        /// <summary>
        ///     Whether the cartridge is suitable for pistols.
        /// </summary>
        public bool SuitableForPistol { get; set; } = true;

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
            return $"{Id} {Name}";
        }
    }
}
