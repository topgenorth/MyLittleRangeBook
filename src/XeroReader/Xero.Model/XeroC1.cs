using System.Collections.Generic;
using NanoidDotNet;

namespace net.opgenorth.xero.device
{
    /// <summary>
    ///     First attempt at an abstraction around a physical Garmin Xero X1 device.
    /// </summary>
    public class XeroC1
    {
        public string Id { get; private set; } = Nanoid.Generate();

        public uint SerialNumber { get; set; }
        public float SoftwareVersion { get; set; }
        public ushort Manufacturer { get; set; }

        public ShotCollection Sessions { get; } = new ShotCollection();

        public override string ToString()
        {
            return $"Garmin Xero S/N {SerialNumber}, Software {SoftwareVersion}.";
        }
    }
}
