using System;
using NanoidDotNet;

namespace net.opgenorth.xero.device
{
    /// <summary>
    ///     Represents a single shot that has be recorded by a Xero.
    /// </summary>
    public class Shot
    {
        public Shot(): this(Nanoid.Generate())
        {
        }

        public Shot(string shotId)
        {
            DateTimeUtc = DateTime.UtcNow;
            Id = shotId;
        }


        public Shot(Shot otherShot) : this()
        {
            Id = otherShot.Id;
            DateTimeUtc = otherShot.DateTimeUtc;
            ShotNumber = otherShot.ShotNumber;
            Speed = otherShot.Speed;
            CleanBore = otherShot.CleanBore;
            ColdBore = otherShot.ColdBore;
            IgnoreShot = otherShot.IgnoreShot;
            Notes = otherShot.Notes;
        }

        public string Id { get; }
        public DateTime DateTimeUtc { get; set; }
        public int ShotNumber { get; set; }
        public ShotSpeed Speed { get; set; } = ShotSpeed.Zero;

        public bool CleanBore { get; set; }
        public bool ColdBore { get; set; }
        public string? Notes { get; set; }

        public bool IgnoreShot { get; set; }

        public override string ToString() => $"#{ShotNumber}: {Speed}";
    }
}
