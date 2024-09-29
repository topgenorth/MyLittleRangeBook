using System.Collections.Generic;
using System.Dynamic;
using NanoidDotNet;

namespace net.opgenorth.xero.device
{
    /// <summary>
    ///     First attempt at an abstraction around a physical Garmin Xero X1 device.
    /// </summary>
    public class XeroC1
    {
        internal XeroC1(uint serialNumber)
        {
            SerialNumber = serialNumber;
            Id = Nanoid.Generate();
            Sessions = new object();
        }
        public string Id { get; private set; }

        public static XeroC1 New(int serialNumber)
        {
            return New((uint)serialNumber);
        }
        public static XeroC1 New(uint serialNumber)
        {
            return new XeroC1(serialNumber);
        }
        public uint SerialNumber { get; set; }
        public float SoftwareVersion { get; set; }
        public ushort Manufacturer { get; set; }

        public object Sessions { get; private set; } 

        public override string ToString()
        {
            return $"Garmin Xero S/N {SerialNumber}, Software {SoftwareVersion}.";
        }
    }
}
