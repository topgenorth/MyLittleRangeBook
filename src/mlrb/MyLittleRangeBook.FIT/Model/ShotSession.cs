using MyLittleRangeBook.CLI;
using NanoidDotNet;

namespace MyLittleRangeBook.FIT.Model
{
    /// <summary>
    ///     A session holds data about a single Xero shooting session.
    /// </summary>
    public class ShotSession
    {
        readonly ShotCollection _shots = [];

#pragma warning disable CS0414 // Field is assigned but its value is never used
        readonly uint _xeroSerialNumber;
#pragma warning restore CS0414 // Field is assigned but its value is never used

        public ShotSession() : this(Nanoid.Generate())
        {
        }

        public ShotSession(uint xeroSerialNumber)
        {
            Id = xeroSerialNumber.ToShotSessionId();
            _xeroSerialNumber = xeroSerialNumber;
            FileName = string.Empty;
            ProjectileType = "Rifle";
            Notes = string.Empty;
            ProjectileUnits = "grains";
            VelocityUnits = "m/s";
        }

        public ShotSession(string id)
        {
            Id = id;
            _xeroSerialNumber = 0; // TODO [TO20260405] Try to parse the serial number from the ID.
            FileName = string.Empty;
            ProjectileType = "Rifle";
            Notes = string.Empty;
            ProjectileUnits = "grains";
            VelocityUnits = "m/s";
        }


        public string Id { get; set; }

        public string FileName { get; set; }

        public DateTime DateTimeUtc { get; set; }
        public int ShotCount => _shots.Count;
        public int ProjectileWeight { get; set; }
        public string ProjectileType { get; set; }

        public int AverageSpeed { get; set; }

        public int ExtremeSpread { get; set; }

        public double StandardDeviation { get; set; }

        public ShotCollection Shots => _shots;
        public uint SerialNumber { get; set; }

        public string Notes { get; set; }
        public string ProjectileUnits { get; set; }

        /// <summary>
        ///     The Xero uses m/s as the default.
        /// </summary>
        public string VelocityUnits { get; set; }

        public int MaxSpeed { get; set; }
        public int MinSpeed { get; set; }

        public void AddShot(Shot shot)
        {
            if (_shots.Contains(shot))
                // TODO [TO20240928] Could check the .Id value, if these are the same, then "add" becomes "replace".
            {
                throw new ArgumentException($"The collection already has a shot number {shot.ShotNumber}");
            }

            _shots.Add(shot);
        }


        public override string ToString()
        {
            return
                $"{_shots.Count} shots. Avg: {AverageSpeed} {VelocityUnits}, Max: {MaxSpeed} {VelocityUnits}, Min: {MinSpeed} {VelocityUnits}, SD: {StandardDeviation:F1} {VelocityUnits}";
        }
    }
}
