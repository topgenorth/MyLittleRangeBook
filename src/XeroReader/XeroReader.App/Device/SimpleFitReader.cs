using Dynastream.Fit;

namespace net.opgenorth.xero.Device

{
    public class SimpleFitReader
    {
        readonly ILogger _logger;

        public SimpleFitReader(ILogger logger)
        {
            _logger = logger;
        }

        void ParseDeviceInfoMesg(XeroC1 xero, DeviceInfoMesg? msg)
        {
            if (msg is null) return;
            if (!msg.Name.Equals("DeviceInfo", StringComparison.OrdinalIgnoreCase)) return;

            xero.SoftwareVersion = msg?.GetSoftwareVersion() ?? 0.0f;
            xero.SerialNumber = msg?.GetSerialNumber() ?? 0;
            xero.Manufacturer = msg?.GetManufacturer() ?? 0;
        }

        void ParseChronoShotSessionMessage(ShotSession shotSession, ChronoShotSessionMesg msg)
        {
            var dt = msg.GetTimestamp().GetDateTime();

            var f = msg.Fields.First(f => f.Name == "MinSpeed");
            var s= msg.GetMaxSpeed() ?? 0f;
            shotSession.SessionTimestamp = dt;
            shotSession.ProjectileWeight = msg.GetGrainWeight() ?? 0;
            shotSession.ProjectileType = msg.GetProjectileType().ToString() ?? "Unknown";
            shotSession.Units = f.GetUnits();

            // TODO [TO20240922] Get the units of measure. That might belong to the shot?
        }

        public async Task<int> Read(string filename)
        {
            _logger.Information("Processing file {filename}", filename);

            var xero = new XeroC1();
            var shotSession = new ShotSession();
            xero.Sessions.Add(shotSession);
            
            var fitListener = new FitListener();
            var decodeDemo = new Decode();
            decodeDemo.MesgEvent += fitListener.OnMesg;

            _logger.Information("Decoding...", filename);

            await using var fitSource = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            decodeDemo.Read(fitSource);
            var fitMessages = fitListener.FitMessages;

            _logger.Verbose("Reading DeviceInfoMesgs");
            foreach (var msg in fitMessages.DeviceInfoMesgs)
            {
                ParseDeviceInfoMesg(xero, msg);
            }

            foreach (var msg in fitMessages.ChronoShotSessionMesgs)
            {
                ParseChronoShotSessionMessage(shotSession, msg);
            }

            foreach (var msg in fitMessages.ChronoShotDataMesgs)
            {
                ParseChronoShotDataMesgs(shotSession, msg);
            }

            _logger.Information(xero.ToString());
            _logger.Information(xero.Sessions[0].ToString());

            return 0;
        }

        void ParseChronoShotDataMesgs(ShotSession shotSession, ChronoShotDataMesg msg)
        {
            var shot = new Shot
            {
                ShotNumber = (int)msg.GetShotNum(), 
                Timestamp = msg.GetTimestamp().GetDateTime(),
                Speed = msg.GetShotSpeed() ?? 0f,
            };

            shotSession.AddShot(shot);
        }
    }
}
