using NanoidDotNet;

namespace MyLittleRangeBook.FIT.Model
{
    /// <summary>
    ///     Represents a single shot that has be recorded by a Xero.
    /// </summary>
    public class Shot
    {
        public Shot()
        {
        }

        public Shot(string shotId)
        {
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

        public string Id { get; } = Nanoid.Generate();
        public DateTimeOffset DateTimeUtc { get; set; } = FitExtensions.FitEpoch;
        public int ShotNumber { get; set; } = -1;
        public ShotSpeed Speed { get; set; } = ShotSpeed.Zero;

        public bool CleanBore { get; set; } = false;
        public bool ColdBore { get; set; } = false;
        public string? Notes { get; set; }

        public bool IgnoreShot { get; set; } = false;

        public override string ToString()
        {
            return $"#{ShotNumber}: {Speed}";
        }
    }
}
