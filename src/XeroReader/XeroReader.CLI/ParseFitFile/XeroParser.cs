using Dynastream.Fit;
using net.opgenorth.xero.device;

namespace net.opgenorth.xero.ParseFitFile
{
    public class XeroParser
    {
        const int ExpectedFileType = 54;

        readonly ILogger _logger;
        readonly FitListener _fitListener = new();
        FitMessages _fitMessages;
        HashSet<string> RecordFieldNames = new();
        HashSet<string> RecordDeveloperFieldNames = new();
        ShotSession _shotSession;

        public XeroParser(ILogger logger)
        {
            _logger = logger.ForContext<XeroParser>();
        }

        public ShotSession Decode(Stream inputStrea)
        {
            _shotSession = new ShotSession();

            var decoder = new Decode();
            // Check that this is a FIT file
            if (!decoder.IsFIT(inputStrea))
            {
                throw new FileTypeException($"Expected FIT File type {ExpectedFileType}, received a non FIT file.");
            }

            var msgBroadCasgter = new MesgBroadcaster();
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

        void OnDeviceInfoMsg(object sender, MesgEventArgs e)
        {
            var msg = (DeviceInfoMesg)e.mesg;
            var serialNumber = msg?.GetSerialNumber() ?? 0;

            _shotSession.SerialNumber = serialNumber;
        }

        void OnChronoShotSessionMsg(object sender, MesgEventArgs e)
        {
            var msg = (ChronoShotSessionMesg)e.mesg;
            var dt = msg.GetTimestamp().GetDateTime();

            var f = msg.Fields.First(f => f.Name == "MinSpeed");
            var s = msg.GetMaxSpeed() ?? 0f;
            _shotSession.SessionTimestamp = dt;
            _shotSession.ProjectileWeight = msg.GetGrainWeight() ?? 0;
            _shotSession.ProjectileType = msg.GetProjectileType().ToString() ?? "Unknown";
        }

        void OnChronoShotMsg(object sender, MesgEventArgs e)
        {
            var msg = (ChronoShotDataMesg)e.mesg;

            var shot = new Shot
            {
                ShotNumber = (int)msg.GetShotNum()!,
                Timestamp = msg.GetTimestamp().GetDateTime(),
                Speed = new ShotSpeed(msg.GetShotSpeed() ?? 0f)
            };
            _shotSession.AddShot(shot);
        }

        void OnRecordMsg(object sender, MesgEventArgs e)
        {
            foreach (var field in e.mesg.Fields)
            {
                if (field.Name.ToLower() != "unknown")
                {
                    RecordFieldNames.Add(field.Name);
                }
            }

            foreach (var devField in e.mesg.DeveloperFields)
            {
                RecordDeveloperFieldNames.Add(devField.Name);
            }
        }

        void OnFileIdMsg(object sender, MesgEventArgs e)
        {
            var f = (FileIdMesg)e.mesg;
            var t = f.GetType();

            if (ExpectedFileType != (int)t!)
            {
                throw new FileTypeException($"Expected FIT File type {ExpectedFileType}, received {t}.");
            }
        }
    }
}
