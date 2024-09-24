using NanoidDotNet;

namespace net.opgenorth.xero.Device
{
    /// <summary>
    ///     Represents a single shot that has be recorded by a Xero.
    /// </summary>
    public class Shot
    {
        public string Id { get; private set; } = Nanoid.Generate();
        public DateTime Timestamp { get; set; }
        public int ShotNumber { get; set; }
        public string Units { get; set; }
        public float Speed { get; set; }
    }
}
