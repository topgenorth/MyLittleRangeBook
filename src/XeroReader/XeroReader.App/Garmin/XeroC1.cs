namespace net.opgenorth.xero.Garmin;

public class XeroC1
{
    List<ShotSession> _shotSessions = new();
    
    public uint SerialNumber { get; set; }
    public float SoftwareVersion { get; set; }
    public ushort Manufacturer { get; set; }

    public List<ShotSession> Sessions => _shotSessions;

    public override string ToString()
    {
        return $"Garmin Xero S/N {SerialNumber}, Software {SoftwareVersion}.";
    }
}
