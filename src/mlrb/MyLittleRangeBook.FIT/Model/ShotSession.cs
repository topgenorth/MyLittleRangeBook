using ByteAether.Ulid;
using Dynastream.Fit;
using File = Dynastream.Fit.File;

namespace MyLittleRangeBook.FIT.Model
{
    /// <summary>
    ///     A session holds data about a single Xero shooting session.
    /// </summary>
    public class ShotSession
    {
        internal const int EXPECTED_FILE_TYPE = 54; // TODO [TO20260414] Change the name to a FITMessageType

#pragma warning disable CS0414 // Field is assigned but its value is never used
        readonly uint _xeroSerialNumber;
#pragma warning restore CS0414 // Field is assigned but its value is never used


        public ShotSession(DeviceInfoMesg msg)
        {
            FileName = string.Empty;
            ProjectileType = "Rifle";
            Notes = string.Empty;
            ProjectileUnits = "grains";
            VelocityUnits = "m/s";

            SerialNumber = msg.GetSerialNumber() ?? 0;
            TimeCreated = msg.GetTimestampUtc();
            Manufacturer = msg.GetManufacturer();
            Product = msg.GetProduct();
            Id = Ulid.New(TimeCreated);
            SoftwareVersion = msg.GetSoftwareVersion().ToString() ?? "Unknown";

        }
        public ShotSession(FileIdMesg fileIdMesg)
        {
            File? fitType = fileIdMesg.GetType();
            if (EXPECTED_FILE_TYPE != (int)fitType!)
            {
                throw new Exception($"Expected FIT File type {EXPECTED_FILE_TYPE}, received {fitType}.");
            }
            FileName = string.Empty;
            ProjectileType = "Rifle";
            Notes = string.Empty;
            ProjectileUnits = "grains";
            VelocityUnits = "m/s";
            SoftwareVersion = "Unknown";

            SerialNumber = fileIdMesg.GetSerialNumber() ?? 0;
            TimeCreated = fileIdMesg.GetTimeCreatedUtc();
            Product = fileIdMesg.GetProduct();
            Manufacturer = fileIdMesg.GetManufacturer();
            Type = (byte)fitType;
            Id = Ulid.New(TimeCreated);
        }

        /// <summary>
        /// </summary>
        /// <param name="id">Must be unique value to identify the shot session.</param>
        public ShotSession(string id)
        {
            Id = id;
            _xeroSerialNumber = 0; // TODO [TO20260405] Try to parse the serial number from the ID.
            FileName = string.Empty;
            ProjectileType = "Rifle";
            Notes = string.Empty;
            ProjectileUnits = "grains";
            VelocityUnits = "m/s";
            SoftwareVersion = "Unknown";
        }


        public string Id { get; set; }

        public string FileName { get; set; }

        public DateTimeOffset DateTimeUtc { get; set; }
        public int ShotCount => Shots.Count;
        public int ProjectileWeight { get; set; }
        public string ProjectileType { get; set; }

        public int AverageSpeed { get; set; }

        public int ExtremeSpread { get; set; }

        public double StandardDeviation { get; set; }

        public ShotCollection Shots { get; } = [];

        public uint SerialNumber { get; set; }

        public ushort? Manufacturer { get; set; }
        public ushort? Product { get; set; }
        public byte? Type { get; set; }
        public string SoftwareVersion { get; set; }
        public DateTimeOffset TimeCreated { get; set; }

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
            if (Shots.Contains(shot))
                // TODO [TO20240928] Could check the .Id value, if these are the same, then "add" becomes "replace".
            {
                throw new ArgumentException($"The collection already has a shot number {shot.ShotNumber}");
            }

            Shots.Add(shot);
        }


        public override string ToString()
        {
            return
                $"{Shots.Count} shots. Avg: {AverageSpeed} {VelocityUnits}, Max: {MaxSpeed} {VelocityUnits}, Min: {MinSpeed} {VelocityUnits}, SD: {StandardDeviation:F1} {VelocityUnits}";
        }
    }
}
