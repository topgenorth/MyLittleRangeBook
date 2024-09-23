using Dynastream.Fit;
using net.opgenorth.xero.Garmin;

namespace net.opgenorth.xero;

public class SimpleFitReader
{
    readonly ILogger _logger;
    public SimpleFitReader(ILogger logger)
    {
        _logger = logger;
    }

    void ParseDeviceInfoMesg(XeroC1 xero, DeviceInfoMesg? msg)
    {
        if (msg is null)
        {
            return;
        }

        if (!msg.Name.Equals("DeviceInfo", StringComparison.OrdinalIgnoreCase)) return;
        xero.SoftwareVersion = msg?.GetSoftwareVersion() ?? 0.0f;
        xero.SerialNumber = msg?.GetSerialNumber() ?? 0;
        xero.Manufacturer = msg?.GetManufacturer() ?? 0;
    }

    void ParseChronoShotSessionMessage(ShotSession shotSession,
        ChronoShotSessionMesg? msg)
    {
        if (msg is null) return;
        var dt = msg.GetTimestamp()
            .GetDateTime();
        shotSession.SessionTimestamp = dt;
        shotSession.ProjectileWeight = msg.GetGrainWeight() ?? 0;
        shotSession.ProjectileType = msg.GetProjectileType()
            .ToString() ?? "Unknown";

        // TODO [TO20240922] Get the units of measure. That might belong to the shot?
    }

    public async Task<int> Read(string filename)
    {
        _logger.Information("Processing file {filename}", filename);

        var fitListener = new FitListener();
        var decodeDemo = new Decode();
        decodeDemo.MesgEvent += fitListener.OnMesg;

        _logger.Information("Decoding...");
        await using var fitSource = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        decodeDemo.Read(fitSource);

        var fitMessages = fitListener.FitMessages;
        var xero = new XeroC1();

        
        _logger.Verbose("Reading DeviceInfoMesgs");
        foreach (var msg in fitMessages.DeviceInfoMesgs)
        {
            ParseDeviceInfoMesg(xero, msg);
        }
        
        var shotSession = new ShotSession();
        foreach (var msg in fitMessages.ChronoShotSessionMesgs)
        {
            ParseChronoShotSessionMessage(shotSession, msg);
        }
        
        xero.Sessions.Add(shotSession);
        _logger.Information(xero.ToString());
        return 0;
    }
}
