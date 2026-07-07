namespace MyLittleRangeBook.FIT.Model
{
    /// <summary>
    ///     First attempt at an abstraction around a physical Garmin Xero X1 device.
    /// </summary>
    public class XeroC1
    {
        internal XeroC1(uint serialNumber)
        {
            SerialNumber = serialNumber;
            Id           = serialNumber.ToString();
        }

        public string Id              { get; private set; }
        public uint   SerialNumber    { get; set; }
        public float  SoftwareVersion { get; set; }
        public ushort Manufacturer    { get; set; }

        public ShotSessionCollection Sessions { get; private set; } = new();

        public static XeroC1 New(int serialNumber) => New((uint)serialNumber);

        public static XeroC1 New(uint serialNumber) => new(serialNumber);

        public override string ToString() => $"Garmin Xero S/N {SerialNumber}, Software {SoftwareVersion}.";
    }
}