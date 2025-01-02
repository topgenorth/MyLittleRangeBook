using Dynastream.Fit;
using net.opgenorth.xero.device;
using DateTime = System.DateTime;
using File = Dynastream.Fit.File;

namespace net.opgenorth.xero.GarminFit;

/// <summary>
///     Xero C1 parser for FIT files.
/// </summary>
public class XeroParser
{
    private const int ExpectedFileType = 54;
    private readonly FitListener _fitListener = new();

    private readonly ILogger _logger;
    private readonly HashSet<string> RecordDeveloperFieldNames = new();
    private readonly HashSet<string> RecordFieldNames = new();
    private FitMessages _fitMessages;
    private ShotSession _shotSession;

    public XeroParser(ILogger logger) => _logger = logger.ForContext<XeroParser>();

    public ShotSession Decode(Stream inputStrea)
    {
        _shotSession = new ShotSession();

        Decode decoder = new();
        // Check that this is a FIT file
        if (!decoder.IsFIT(inputStrea))
        {
            throw new FileTypeException($"Expected FIT File type {ExpectedFileType}, received a non FIT file.");
        }

        MesgBroadcaster msgBroadCasgter = new();
        decoder.MesgEvent += msgBroadCasgter.OnMesg;
        msgBroadCasgter.DeviceInfoMesgEvent += OnDeviceInfoMsg;
        msgBroadCasgter.FileIdMesgEvent += OnFileIdMsg;
        msgBroadCasgter.RecordMesgEvent += OnRecordMsg;
        msgBroadCasgter.ChronoShotDataMesgEvent += OnChronoShotMsg;
        msgBroadCasgter.ChronoShotSessionMesgEvent += OnChronoShotSessionMsg;
        decoder.MesgEvent += _fitListener.OnMesg;

        if (decoder.Read(inputStrea))
        {
            return _shotSession;
        }

        throw new FileTypeException("Could not parse the file.");
    }

    private void OnDeviceInfoMsg(object sender, MesgEventArgs e)
    {
        DeviceInfoMesg? msg = (DeviceInfoMesg)e.mesg;
        uint serialNumber = msg?.GetSerialNumber() ?? 0;

        _shotSession.SerialNumber = serialNumber;
    }

    private void OnChronoShotSessionMsg(object sender, MesgEventArgs e)
    {
        ChronoShotSessionMesg? msg = (ChronoShotSessionMesg)e.mesg;
        DateTime dt = msg.GetTimestamp().GetDateTime();

        Field? f = msg.Fields.First(f => f.Name == "MinSpeed");
        float s = msg.GetMaxSpeed() ?? 0f;
        _shotSession.DateTimeUtc = dt;

        float w = msg.GetGrainWeight() ?? 0f;
        _shotSession.ProjectileWeight = Convert.ToInt32(w);
        _shotSession.ProjectileType = msg.GetProjectileType().ToString() ?? "Unknown";
    }

    private void OnChronoShotMsg(object sender, MesgEventArgs e)
    {
        ChronoShotDataMesg? msg = (ChronoShotDataMesg)e.mesg;

        Shot shot = new()
        {
            ShotNumber = (int)msg.GetShotNum()!,
            DateTimeUtc = msg.GetTimestamp().GetDateTime(),
            Speed = new ShotSpeed(msg.GetShotSpeed() ?? 0f)
        };
        _shotSession.AddShot(shot);
    }

    private void OnRecordMsg(object sender, MesgEventArgs e)
    {
        foreach (Field? field in e.mesg.Fields)
        {
            if (field.Name.ToLower() != "unknown")
            {
                RecordFieldNames.Add(field.Name);
            }
        }

        foreach (DeveloperField? devField in e.mesg.DeveloperFields)
        {
            RecordDeveloperFieldNames.Add(devField.Name);
        }
    }

    private void OnFileIdMsg(object sender, MesgEventArgs e)
    {
        FileIdMesg? f = (FileIdMesg)e.mesg;
        File? t = f.GetType();

        if (ExpectedFileType != (int)t!)
        {
            throw new FileTypeException($"Expected FIT File type {ExpectedFileType}, received {t}.");
        }
    }
}
