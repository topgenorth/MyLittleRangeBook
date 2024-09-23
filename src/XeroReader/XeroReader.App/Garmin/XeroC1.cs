namespace net.opgenorth.xero.Garmin
{
    /// <summary>
    ///     First attempt at an abstraction around a physical Garmin Xero X1 device.
    /// </summary>
    public class XeroC1
    {
        public uint SerialNumber { get; set; }
        public float SoftwareVersion { get; set; }
        public ushort Manufacturer { get; set; }

        public List<ShotSession> Sessions { get; } = new();

        public override string ToString()
        {
            return $"Garmin Xero S/N {SerialNumber}, Software {SoftwareVersion}.";
        }
    }
}
